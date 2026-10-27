using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.Excel;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Excel;

/// <summary>
/// Bộ kiểm thử xác thực tính năng Dynamic Excel Import/Export tự động đồng bộ theo Live CRUD.
/// </summary>
public class ExcelDynamicCustomFieldsTests
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
    public async Task TaiTepMau_CoTruongTuyChinh_XuatHienCotDongTrongTemplate()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Thêm trường tùy chỉnh "Chứng chỉ" vào bảng NhanSu (mô phỏng thao tác từ Live CRUD)
        db.TruongTuyChinhs.Add(new TruongTuyChinh
        {
            TenBang = "NhanSu",
            MaTruong = "ChungChi",
            TenHienThi = "Chứng chỉ",
            KieuDuLieu = "VanBan",
            CoHoatDong = true,
            LaBatBuoc = false,
            ThuTuHienThi = 1
        });
        await db.SaveChangesAsync();

        // Act
        var bytes = excelDichVu.TaoTepMauExcel("NhanSu");

        // Assert
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();

        var lastCol = ws.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
        Assert.True(lastCol > 0);

        // Cột cuối cùng phải là trường tùy chỉnh "Chứng chỉ"
        var tenCotCuoi = ws.Cell(1, lastCol).GetString().Trim();
        Assert.Equal("Chứng chỉ", tenCotCuoi);

        // Header trường động có màu nền xanh lam nổi bật (#0D6EFD)
        var bgColor = ws.Cell(1, lastCol).Style.Fill.BackgroundColor;
        Assert.Equal(13, bgColor.Color.R);
        Assert.Equal(110, bgColor.Color.G);
        Assert.Equal(253, bgColor.Color.B);

        // Dòng hướng dẫn (Row 2) mô tả trường tùy chỉnh
        var huongDan = ws.Cell(2, lastCol).GetString().Trim();
        Assert.Contains("[Tùy chọn]", huongDan);
        Assert.Contains("Chứng chỉ", huongDan);
    }

    [Fact]
    public async Task XuatDanhSachDuLieu_CoTruongTuyChinh_XuatDungGiaTriThucTe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Thêm trường tùy chỉnh "Chứng chỉ"
        var truong = new TruongTuyChinh
        {
            TenBang = "NhanSu",
            MaTruong = "ChungChi",
            TenHienThi = "Chứng chỉ",
            KieuDuLieu = "VanBan",
            CoHoatDong = true,
            ThuTuHienThi = 1
        };
        db.TruongTuyChinhs.Add(truong);

        // Thêm cán bộ
        var ns = new NhanSu
        {
            MaNhanSu = "CB_TEST_01",
            HoVaTen = "Nguyễn Văn Test",
            NgaySinh = new DateTime(1985, 1, 1),
            GioiTinh = "Nam",
            Email = "test@c500.edu.vn",
            TrangThai = "Đang làm việc"
        };
        db.NhanSus.Add(ns);
        await db.SaveChangesAsync();

        // Gắn giá trị tùy chỉnh cho cán bộ
        db.GiaTriTruongTuyChinhs.Add(new GiaTriTruongTuyChinh
        {
            TruongTuyChinhId = truong.TruongTuyChinhId,
            TenBang = "NhanSu",
            KhoaBanGhi = ns.NhanSuId.ToString(),
            GiaTri = "IELTS 8.0",
            NgayCapNhat = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        // Act
        var bytes = await excelDichVu.XuatDanhSachDuLieuExcelAsync("NhanSu");

        // Assert
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);
        var ws = wb.Worksheets.First();

        var lastCol = ws.Row(1).LastCellUsed()?.Address.ColumnNumber ?? 0;
        Assert.Equal("Chứng chỉ", ws.Cell(1, lastCol).GetString().Trim());

        // Dòng dữ liệu (Row 2) phải chứa giá trị "IELTS 8.0"
        var giaTriXuat = ws.Cell(2, lastCol).GetString().Trim();
        Assert.Equal("IELTS 8.0", giaTriXuat);
    }

    [Fact]
    public async Task NhapExcel_CoTruongTuyChinh_NhanDienHeaderVaCommitVaoDatabase()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Thêm trường tùy chỉnh "Chứng chỉ"
        var truong = new TruongTuyChinh
        {
            TenBang = "NhanSu",
            MaTruong = "ChungChi",
            TenHienThi = "Chứng chỉ",
            KieuDuLieu = "VanBan",
            CoHoatDong = true,
            ThuTuHienThi = 1
        };
        db.TruongTuyChinhs.Add(truong);
        await db.SaveChangesAsync();

        // Tạo file Excel có cột chuẩn + cột tùy chỉnh "Chứng chỉ"
        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã nhân sự";
            ws.Cell(1, 2).Value = "Họ và tên";
            ws.Cell(1, 3).Value = "Ngày sinh";
            ws.Cell(1, 4).Value = "Giới tính";
            ws.Cell(1, 5).Value = "Email";
            ws.Cell(1, 6).Value = "Chứng chỉ"; // Cột tùy chỉnh động

            ws.Cell(2, 1).SetValue("CB_IMPORT_01");
            ws.Cell(2, 2).SetValue("Trần Thị Test");
            ws.Cell(2, 3).SetValue("15/08/1990");
            ws.Cell(2, 4).SetValue("Nữ");
            ws.Cell(2, 5).SetValue("tranthitest@c500.edu.vn");
            ws.Cell(2, 6).SetValue("IELTS 7.5 - Thạc sĩ");
        });

        // Act 1: Kiểm tra & Xem trước
        var ketQuaPreview = await excelDichVu.KiemTraVaDocTepExcelAsync("NhanSu", stream, "import_test.xlsx");

        // Assert 1: Preview hợp lệ, nhận diện đúng cột Chứng chỉ
        Assert.True(ketQuaPreview.LaHopLeToanBo);
        Assert.Contains("Chứng chỉ", ketQuaPreview.DanhSachCot);
        Assert.Equal("IELTS 7.5 - Thạc sĩ", ketQuaPreview.DanhSachDongDuLieu[0].DuLieuCot["Chứng chỉ"]);

        // Act 2: Commit vào cơ sở dữ liệu
        var ketQuaCommit = await excelDichVu.CommitDuLieuExcelAsync("NhanSu", ketQuaPreview.DanhSachDongDuLieu, "AdminTester");

        // Assert 2: Commit thành công
        Assert.True(ketQuaCommit.ThanhCong);
        Assert.Equal(1, ketQuaCommit.SoBanGhiDaThem);

        // Xác nhận bản ghi cán bộ đã được lưu
        var nsDb = await db.NhanSus.FirstOrDefaultAsync(n => n.MaNhanSu == "CB_IMPORT_01");
        Assert.NotNull(nsDb);

        // Xác nhận giá trị trường tùy chỉnh đã được lưu vào bảng GiaTriTruongTuyChinhs
        var giaTriCustomDb = await db.GiaTriTruongTuyChinhs.FirstOrDefaultAsync(g =>
            g.TruongTuyChinhId == truong.TruongTuyChinhId &&
            g.TenBang == "NhanSu" &&
            g.KhoaBanGhi == nsDb.NhanSuId.ToString());

        Assert.NotNull(giaTriCustomDb);
        Assert.Equal("IELTS 7.5 - Thạc sĩ", giaTriCustomDb.GiaTri);
    }
}
