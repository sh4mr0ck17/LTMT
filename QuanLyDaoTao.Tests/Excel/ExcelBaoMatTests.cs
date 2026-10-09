using ClosedXML.Excel;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Excel;

/// <summary>
/// Bộ kiểm thử Nhóm E: Bảo mật tệp Excel, chống Formula Injection và giới hạn tài nguyên.
/// </summary>
public class ExcelBaoMatTests
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
    public async Task XuatExcel_BaoVeChongFormulaInjection()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Tạo khoa có tên cố tình chứa công thức Excel độc hại
        var khoaDocHai = new Khoa
        {
            MaKhoa = "=SUM(1+1)",
            TenKhoa = "+cmd|' /C calc'!A0",
            MoTa = "-2+3*4",
            Email = "@danger@test.local"
        };
        db.Khoas.Add(khoaDocHai);
        await db.SaveChangesAsync();

        // Act - Xuất dữ liệu ra Excel
        var excelBytes = await excelDichVu.XuatDanhSachDuLieuExcelAsync("Khoa");

        // Assert - Đọc lại tệp vừa xuất và kiểm tra xem có được prepend dấu nháy đơn không
        using var ms = new MemoryStream(excelBytes);
        using var workbook = new XLWorkbook(ms);
        var ws = workbook.Worksheets.First();

        var oMaKhoa = ws.Cell(2, 1).GetString();
        var oTenKhoa = ws.Cell(2, 2).GetString();
        var oMoTa = ws.Cell(2, 3).GetString();
        Assert.False(ws.Cell(2, 1).HasFormula);
        Assert.False(ws.Cell(2, 2).HasFormula);
        Assert.False(ws.Cell(2, 3).HasFormula);
        Assert.False(ws.Cell(2, 5).HasFormula);
        Assert.Equal("=SUM(1+1)", oMaKhoa);
        Assert.Equal("+cmd|' /C calc'!A0", oTenKhoa);
    }

    [Fact]
    public async Task KiemTraExcel_GioiHan1000Dong_BaoLoi()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Tạo tệp Excel có 1005 dòng dữ liệu (> 1000)
        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã khoa";
            ws.Cell(1, 2).Value = "Tên khoa";

            for (int r = 2; r <= 1006; r++)
            {
                ws.Cell(r, 1).SetValue($"K_{r}");
                ws.Cell(r, 2).SetValue($"Khoa {r}");
            }
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test_1000.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi == "ERR_MAX_ROWS_EXCEEDED");
    }
}
