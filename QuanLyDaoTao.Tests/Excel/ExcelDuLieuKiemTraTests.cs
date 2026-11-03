using ClosedXML.Excel;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Excel;

/// <summary>
/// Bộ kiểm thử Nhóm C: Xác thực kiểu dữ liệu, định dạng, trùng lặp và khóa ngoại trong tệp Excel.
/// </summary>
public class ExcelDuLieuKiemTraTests
{
    private static MemoryStream TaoWorkbookInMemory(Action<IXLWorksheet> setupAction)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("DuLieu");
        setupAction(worksheet);
        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    [Fact]
    public async Task KiemTraDuLieu_NgaySinhKhongHopLe_BaoLoi()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Chuẩn bị lớp học có sẵn
        var khoa = new Khoa { MaKhoa = "CNTT", TenKhoa = "CNTT" };
        var lop = new LopHoc { MaLop = "L_K20", TenLop = "Lớp K20", NienKhoa = "2024-2028", Khoa = khoa };
        db.LopHocs.Add(lop);
        await db.SaveChangesAsync();

        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã học viên";
            ws.Cell(1, 2).Value = "Họ và tên";
            ws.Cell(1, 3).Value = "Ngày sinh";
            ws.Cell(1, 4).Value = "Giới tính";
            ws.Cell(1, 5).Value = "Mã lớp";

            ws.Cell(2, 1).SetValue("HV01");
            ws.Cell(2, 2).SetValue("Nguyễn Văn A");
            ws.Cell(2, 3).SetValue("32/15/2005"); // Ngày sai
            ws.Cell(2, 4).SetValue("Nam");
            ws.Cell(2, 5).SetValue("L_K20");
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("HocVien", stream, "test.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi == "ERR_INVALID_DATE_FORMAT");
    }

    [Fact]
    public async Task KiemTraDuLieu_EmailKhongHopLe_BaoLoi()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã nhân sự";
            ws.Cell(1, 2).Value = "Họ và tên";
            ws.Cell(1, 3).Value = "Ngày sinh";
            ws.Cell(1, 4).Value = "Giới tính";
            ws.Cell(1, 5).Value = "Email công việc";

            ws.Cell(2, 1).SetValue("NS01");
            ws.Cell(2, 2).SetValue("Trần Thị Mai");
            ws.Cell(2, 3).SetValue("20/11/1988");
            ws.Cell(2, 4).SetValue("Nữ");
            ws.Cell(2, 5).SetValue("abc@"); // Email sai định dạng
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("NhanSu", stream, "test.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi == "ERR_INVALID_EMAIL_FORMAT");
    }

    [Fact]
    public async Task KiemTraDuLieu_TruongBatBuocBiTrong_BaoLoi()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã khoa";
            ws.Cell(1, 2).Value = "Tên khoa";

            ws.Cell(2, 1).SetValue("K_01");
            ws.Cell(2, 2).SetValue(""); // Tên khoa bị trống
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi == "ERR_REQUIRED_FIELD_EMPTY" && l.TenTruong == "Tên khoa");
    }

    [Fact]
    public async Task KiemTraDuLieu_MaTrungTrongFile_BaoLoi()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã khoa";
            ws.Cell(1, 2).Value = "Tên khoa";

            ws.Cell(2, 1).SetValue("K_TRUNG");
            ws.Cell(2, 2).SetValue("Khoa 1");

            ws.Cell(3, 1).SetValue("K_TRUNG"); // Trùng mã
            ws.Cell(3, 2).SetValue("Khoa 2");
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi == "ERR_DUPLICATE_KEY_IN_FILE" && l.SoDong == 3);
    }

    [Fact]
    public async Task KiemTraDuLieu_MaDaTonTaiTrongDb_BaoLoi()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        db.Khoas.Add(new Khoa { MaKhoa = "K_CO_SAN", TenKhoa = "Khoa Đã Có" });
        await db.SaveChangesAsync();

        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã khoa";
            ws.Cell(1, 2).Value = "Tên khoa";

            ws.Cell(2, 1).SetValue("K_CO_SAN"); // Mã đã có trong DB
            ws.Cell(2, 2).SetValue("Khoa Trùng DB");
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi == "ERR_DUPLICATE_KEY_IN_DB");
    }

    [Fact]
    public async Task KiemTraDuLieu_KhoaNgoaiKhongTonTai_BaoLoi()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Nhập lớp học với mã khoa không tồn tại
        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã lớp";
            ws.Cell(1, 2).Value = "Tên lớp";
            ws.Cell(1, 3).Value = "Niên khóa";
            ws.Cell(1, 4).Value = "Mã khoa";

            ws.Cell(2, 1).SetValue("L01");
            ws.Cell(2, 2).SetValue("Lớp 1");
            ws.Cell(2, 3).SetValue("2024-2028");
            ws.Cell(2, 4).SetValue("KHOA_KHONG_TON_TAI"); // Khoa không tồn tại
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("LopHoc", stream, "test.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi == "ERR_FOREIGN_KEY_NOT_FOUND" && l.TenTruong == "Mã khoa");
    }

    [Fact]
    public async Task KiemTraDuLieu_GiuSo0DauTienTrongMa()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã nhân sự";
            ws.Cell(1, 2).Value = "Họ và tên";
            ws.Cell(1, 3).Value = "Ngày sinh";
            ws.Cell(1, 4).Value = "Giới tính";
            ws.Cell(1, 5).Value = "Email công việc";
            ws.Cell(1, 6).Value = "Số CCCD";

            ws.Cell(2, 1).SetValue("00123"); // Mã có số 0 đầu
            ws.Cell(2, 2).SetValue("Trần Văn A");
            ws.Cell(2, 3).SetValue("10/05/1990");
            ws.Cell(2, 4).SetValue("Nam");
            ws.Cell(2, 5).SetValue("a@test.local");
            ws.Cell(2, 6).SetValue("001188000123"); // CCCD có số 0 đầu
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("NhanSu", stream, "test.xlsx");

        // Assert
        Assert.True(ketQua.LaHopLeToanBo);
        var dong = ketQua.DanhSachDongDuLieu[0];
        Assert.Equal("00123", dong.DuLieuCot["Mã nhân sự"]);
        Assert.Equal("001188000123", dong.DuLieuCot["Số CCCD"]);
    }

    [Fact]
    public async Task KiemTraDuLieu_MotDongNhieuLoi_BaoTatCaLoi()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Dòng vừa có ngày sai vừa có email sai
        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã nhân sự";
            ws.Cell(1, 2).Value = "Họ và tên";
            ws.Cell(1, 3).Value = "Ngày sinh";
            ws.Cell(1, 4).Value = "Giới tính";
            ws.Cell(1, 5).Value = "Email công việc";

            ws.Cell(2, 1).SetValue("NS01");
            ws.Cell(2, 2).SetValue("Trần Thị B");
            ws.Cell(2, 3).SetValue("ngày_sai"); // Lỗi 1
            ws.Cell(2, 4).SetValue("Nữ");
            ws.Cell(2, 5).SetValue("email_sai"); // Lỗi 2
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("NhanSu", stream, "test.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        var dong = ketQua.DanhSachDongDuLieu[0];
        Assert.False(dong.LaHopLe);
        Assert.Equal(2, dong.DanhSachLoi.Count); // Báo đầy đủ 2 lỗi trên dòng!
    }

    [Fact]
    public async Task KiemTraDuLieu_GhiNhanLoiTheoTungCot_TruongThoaManVaTruongBaoLoi()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        var khoa = new Khoa { MaKhoa = "CNTT", TenKhoa = "Công nghệ Thông tin" };
        var lop = new LopHoc { MaLop = "CNTT_K20A", TenLop = "Lớp K20A", NienKhoa = "2024-2028", Khoa = khoa };
        db.Khoas.Add(khoa);
        db.LopHocs.Add(lop);
        await db.SaveChangesAsync();

        // Dòng có: Họ tên đúng, Ngày sinh đúng, Email đúng nhưng Giới tính sai và Mã lớp sai
        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã học viên";
            ws.Cell(1, 2).Value = "Họ và tên";
            ws.Cell(1, 3).Value = "Ngày sinh";
            ws.Cell(1, 4).Value = "Giới tính";
            ws.Cell(1, 5).Value = "Email";
            ws.Cell(1, 6).Value = "Mã lớp";

            ws.Cell(2, 1).SetValue("HV20260001");
            ws.Cell(2, 2).SetValue("Nguyễn Văn An"); // Thỏa mãn
            ws.Cell(2, 3).SetValue("15/08/2005");   // Thỏa mãn
            ws.Cell(2, 4).SetValue("Gay");          // Sai giới tính -> Báo đỏ
            ws.Cell(2, 5).SetValue("an@email.com"); // Thỏa mãn
            ws.Cell(2, 6).SetValue("LOP_KHONG_CO"); // Sai mã lớp -> Báo đỏ
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("HocVien", stream, "test.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        var dong = ketQua.DanhSachDongDuLieu[0];
        Assert.False(dong.LaHopLe);

        // Các trường sai phải được ghi nhận lỗi theo cột (Báo đỏ)
        Assert.True(dong.CotCoLoi("Giới tính"));
        Assert.True(dong.CotCoLoi("Mã lớp"));
        Assert.False(dong.CotThoaMan("Giới tính"));
        Assert.False(dong.CotThoaMan("Mã lớp"));

        // Các trường đúng phải THỎA MÃN (Hiện xanh lá)
        Assert.False(dong.CotCoLoi("Mã học viên"));
        Assert.True(dong.CotThoaMan("Mã học viên"));
        Assert.False(dong.CotCoLoi("Họ và tên"));
        Assert.True(dong.CotThoaMan("Họ và tên"));
        Assert.False(dong.CotCoLoi("Ngày sinh"));
        Assert.True(dong.CotThoaMan("Ngày sinh"));
        Assert.False(dong.CotCoLoi("Email"));
        Assert.True(dong.CotThoaMan("Email"));
    }

    [Fact]
    public async Task KiemTraDuLieu_NgayThangDinhDangSaCh_KhongBiLoiDoDai()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        var khoa = new Khoa { MaKhoa = "CNTT", TenKhoa = "CNTT" };
        var lop = new LopHoc { MaLop = "L_K20", TenLop = "Lớp K20", NienKhoa = "2024-2028", Khoa = khoa };
        db.LopHocs.Add(lop);
        await db.SaveChangesAsync();

        // Ngày sinh dạng DateTime trong Excel ClosedXML trả chuỗi 22 ký tự ("15/08/2006 12:00:00 SA")
        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã học viên";
            ws.Cell(1, 2).Value = "Họ và tên";
            ws.Cell(1, 3).Value = "Ngày sinh";
            ws.Cell(1, 4).Value = "Giới tính";
            ws.Cell(1, 5).Value = "Mã lớp";

            ws.Cell(2, 1).SetValue("HV01");
            ws.Cell(2, 2).SetValue("Nguyễn Văn B");
            ws.Cell(2, 3).SetValue(new DateTime(2006, 8, 15)); // ClosedXML ghi cell DateTime
            ws.Cell(2, 4).SetValue("Nam");
            ws.Cell(2, 5).SetValue("L_K20");
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("HocVien", stream, "test.xlsx");

        // Assert: Không được báo lỗi độ dài (ERR_MAX_LENGTH_EXCEEDED) mà phải chuẩn hóa thành 15/08/2006
        Assert.True(ketQua.LaHopLeToanBo);
        var dong = ketQua.DanhSachDongDuLieu[0];
        Assert.False(dong.CotCoLoi("Ngày sinh"));
        Assert.True(dong.CotThoaMan("Ngày sinh"));
        Assert.Equal("15/08/2006", dong.DuLieuCot["Ngày sinh"]);
    }

    [Fact]
    public async Task KiemTraDuLieu_SoDienThoaiVaTrangThaiSai_BaoLoiTheoCot()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        var khoa = new Khoa { MaKhoa = "CNTT", TenKhoa = "CNTT" };
        var lop = new LopHoc { MaLop = "L_K20", TenLop = "Lớp K20", NienKhoa = "2024-2028", Khoa = khoa };
        db.LopHocs.Add(lop);
        await db.SaveChangesAsync();

        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã học viên";
            ws.Cell(1, 2).Value = "Họ và tên";
            ws.Cell(1, 3).Value = "Ngày sinh";
            ws.Cell(1, 4).Value = "Giới tính";
            ws.Cell(1, 5).Value = "Mã lớp";
            ws.Cell(1, 6).Value = "Số điện thoại";
            ws.Cell(1, 7).Value = "Trạng thái";

            ws.Cell(2, 1).SetValue("HV01");
            ws.Cell(2, 2).SetValue("Nguyễn Văn C");
            ws.Cell(2, 3).SetValue("10/10/2005");
            ws.Cell(2, 4).SetValue("Nam");
            ws.Cell(2, 5).SetValue("L_K20");
            ws.Cell(2, 6).SetValue("091234567811"); // 12 số -> Báo lỗi
            ws.Cell(2, 7).SetValue("lol");          // Trạng thái vô nghĩa -> Báo lỗi
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("HocVien", stream, "test.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        var dong = ketQua.DanhSachDongDuLieu[0];
        Assert.True(dong.CotCoLoi("Số điện thoại"));
        Assert.True(dong.CotCoLoi("Trạng thái"));
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi == "ERR_INVALID_PHONE_NUMBER");
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi == "ERR_INVALID_LOGIC_VALUE");
    }
}
