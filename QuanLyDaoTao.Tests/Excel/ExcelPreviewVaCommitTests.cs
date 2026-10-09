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
}
