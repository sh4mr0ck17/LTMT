using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.Excel;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Excel;

/// <summary>
/// Bộ kiểm thử Nhóm D: Quản lý phiên xem trước (Preview) và tính toàn vẹn Transaction khi Commit dữ liệu.
/// </summary>
public class ExcelPreviewVaCommitTests
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
    public async Task Preview_KhongLamThayDoiDatabase()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã khoa";
            ws.Cell(1, 2).Value = "Tên khoa";

            ws.Cell(2, 1).SetValue("K_PREVIEW");
            ws.Cell(2, 2).SetValue("Khoa Xem Trước");
        });

        // Act - Chạy bước kiểm tra và preview
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test.xlsx");

        // Assert
        Assert.True(ketQua.LaHopLeToanBo);
        // Xác nhận trong DB vẫn chưa hề có bản ghi nào được ghi
        var khoaDb = await db.Khoas.FirstOrDefaultAsync(k => k.MaKhoa == "K_PREVIEW");
        Assert.Null(khoaDb);
    }

    [Fact]
    public async Task Commit_DongHopLe_LuuDungVaoDatabase()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã khoa";
            ws.Cell(1, 2).Value = "Tên khoa";

            ws.Cell(2, 1).SetValue("K_COMMIT");
            ws.Cell(2, 2).SetValue("Khoa Đã Commit");
        });

        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test.xlsx");
        var danhSachHopLe = ketQua.DanhSachDongDuLieu.Where(d => d.LaHopLe).ToList();

        // Act - Thực hiện commit
        var ketQuaCommit = await excelDichVu.CommitDuLieuExcelAsync("Khoa", danhSachHopLe, "tester");

        // Assert
        Assert.True(ketQuaCommit.ThanhCong);
        Assert.Equal(1, ketQuaCommit.SoBanGhiDaThem);

        var khoaDb = await db.Khoas.FirstOrDefaultAsync(k => k.MaKhoa == "K_COMMIT");
        Assert.NotNull(khoaDb);
        Assert.Equal("Khoa Đã Commit", khoaDb.TenKhoa);
    }

    [Fact]
    public void LuuVaLayPhienPreview_HoatDongDung()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        var ketQuaKiemTra = new KetQuaKiemTraExcel { TenTep = "test.xlsx", TenModule = "Khoa", TongSoDong = 5 };

        // Act
        var phien = excelDichVu.LuuPhienPreview("Khoa", ketQuaKiemTra, "test_user");
        var phienLay = excelDichVu.LayPhienPreview(phien.MaPhien);

        // Assert
        Assert.NotNull(phienLay);
        Assert.Equal("Khoa", phienLay.TenModule);
        Assert.Equal("test_user", phienLay.NguoiTao);

        excelDichVu.XoaPhienPreview(phien.MaPhien);
        var phienSauXoa = excelDichVu.LayPhienPreview(phien.MaPhien);
        Assert.Null(phienSauXoa);
    }

    /// <summary>
    /// Chế độ linh hoạt KHÔNG được bịa dữ liệu định danh hay tham chiếu: thiếu ngày sinh, giới tính, mã lớp → dòng lỗi,
    /// tuyệt đối không tự gán học viên vào "lớp khả dụng đầu tiên".
    /// </summary>
    [Fact]
    public async Task KiemTra_CheDoLinhHoat_ThieuNgaySinhGioiTinhMaLop_LaDongLoiKhongTuGanLop()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "KHOA_CNTT", TenKhoa = "CNTT" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();
        db.LopHocs.Add(new LopHoc { MaLop = "B1D57", TenLop = "Lớp B1D57", KhoaId = khoa.KhoaId, NienKhoa = "2025-2029" });
        await db.SaveChangesAsync();
        var excelDichVu = new ExcelDichVu(db, new NhatKyDichVu(db));

        using var stream = TaoWorkbookInMemory(ws =>
        {
            string[] tieuDe = ["Mã học viên", "Họ và tên", "Ngày sinh", "Giới tính", "Email", "Số điện thoại", "Địa chỉ", "Mã lớp", "Trạng thái"];
            for (var c = 0; c < tieuDe.Length; c++) { ws.Cell(1, c + 1).Value = tieuDe[c]; }
            ws.Cell(2, 2).SetValue("Nguyễn Thị Linh Hoạt");
            ws.Cell(2, 7).SetValue("Hà Nội, Việt Nam");
        });

        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("HocVien", stream, "partial.xlsx", choPhepThieuTruong: true);

        Assert.Equal(0, ketQua.SoDongHopLe);
        Assert.Equal(1, ketQua.SoDongLoi);
        Assert.Contains(ketQua.DanhSachLoi, l => l.TenTruong == "Mã lớp" && l.MaLoi.StartsWith("ERR_"));
        Assert.Contains(ketQua.DanhSachLoi, l => l.TenTruong == "Ngày sinh" && l.MaLoi.StartsWith("ERR_"));
    }

    /// <summary>Chế độ linh hoạt vẫn tự sinh MÃ HỆ THỐNG (mã học viên) và trạng thái mặc định khi để trống.</summary>
    [Fact]
    public async Task Commit_CheDoLinhHoat_ThieuMaHocVien_TuSinhMaVaLuuThanhCong()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "KHOA_CNTT", TenKhoa = "CNTT" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();
        var lop = new LopHoc { MaLop = "B1D57", TenLop = "Lớp B1D57", KhoaId = khoa.KhoaId, NienKhoa = "2025-2029" };
        db.LopHocs.Add(lop);
        await db.SaveChangesAsync();
        var excelDichVu = new ExcelDichVu(db, new NhatKyDichVu(db));

        using var stream = TaoWorkbookInMemory(ws =>
        {
            string[] tieuDe = ["Mã học viên", "Họ và tên", "Ngày sinh", "Giới tính", "Email", "Số điện thoại", "Địa chỉ", "Mã lớp", "Trạng thái"];
            for (var c = 0; c < tieuDe.Length; c++) { ws.Cell(1, c + 1).Value = tieuDe[c]; }
            ws.Cell(2, 2).SetValue("Nguyễn Thị Linh Hoạt");
            ws.Cell(2, 3).SetValue("12/03/2006");
            ws.Cell(2, 4).SetValue("Nữ");
            ws.Cell(2, 7).SetValue("Hà Nội, Việt Nam");
            ws.Cell(2, 8).SetValue("B1D57");
        });

        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("HocVien", stream, "partial.xlsx", choPhepThieuTruong: true);
        Assert.Equal(1, ketQua.SoDongHopLe);
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi.StartsWith("WARN_"));

        var ketQuaCommit = await excelDichVu.CommitDuLieuExcelAsync("HocVien", ketQua.DanhSachDongDuLieu.Where(d => d.LaHopLe).ToList(), "tester");
        Assert.True(ketQuaCommit.ThanhCong);

        var hvDb = await db.HocViens.SingleAsync(h => h.HoVaTen == "Nguyễn Thị Linh Hoạt");
        Assert.StartsWith("HV_", hvDb.MaHocVien);
        Assert.Equal(lop.LopHocId, hvDb.LopHocId);
        Assert.Equal("Đang học", hvDb.TrangThai);
    }
}
