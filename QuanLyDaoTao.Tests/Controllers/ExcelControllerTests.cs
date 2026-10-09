using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Controllers;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Controllers;

/// <summary>
/// Bộ kiểm thử cho bộ điều khiển ExcelController.
/// </summary>
public class ExcelControllerTests
{
    [Fact]
    public void TaiTepMau_ModuleKhoa_TraVeFileExcel()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);
        var controller = new ExcelController(excelDichVu, nhatKy);

        // Act
        var result = controller.TaiTepMau("Khoa");

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
        Assert.Equal("MauNhap_Khoa.xlsx", fileResult.FileDownloadName);
        Assert.True(fileResult.FileContents.Length > 0);
    }

    [Fact]
    public void TaiTepMau_ModuleNhanSu_TraVeFileExcel()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);
        var controller = new ExcelController(excelDichVu, nhatKy);

        // Act
        var result = controller.TaiTepMau("NhanSu");

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("MauNhap_NhanSu.xlsx", fileResult.FileDownloadName);
        Assert.True(fileResult.FileContents.Length > 0);
    }

    [Fact]
    public void TaiTepMau_ModuleKhongHopLe_TraVeNotFound()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);
        var controller = new ExcelController(excelDichVu, nhatKy);

        // Act
        var result = controller.TaiTepMau("ModuleKhongTonTai");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task KiemTraTep_KhongCoTep_TraVeModelError()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);
        var controller = new ExcelController(excelDichVu, nhatKy);

        // Act
        var result = await controller.KiemTraTep("Khoa", null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("NhapExcel", viewResult.ViewName);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task KiemTraTep_TepKhongPhaiXlsx_TraVeModelError()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);
        var controller = new ExcelController(excelDichVu, nhatKy);

        var formFile = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "tepExcel", "du_lieu.txt");

        // Act
        var result = await controller.KiemTraTep("Khoa", formFile);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("NhapExcel", viewResult.ViewName);
        Assert.False(controller.ModelState.IsValid);
    }
}
