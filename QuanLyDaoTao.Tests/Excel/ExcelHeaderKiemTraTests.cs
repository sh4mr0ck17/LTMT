using ClosedXML.Excel;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Excel;

/// <summary>
/// Bộ kiểm thử Nhóm B: Xác thực và kiểm tra nghiêm ngặt Header Excel trước khi đọc dữ liệu.
/// </summary>
public class ExcelHeaderKiemTraTests
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
    public async Task KiemTraHeader_FileDungTemplate_HopLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Tạo file đúng template Khoa
        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã khoa";
            ws.Cell(1, 2).Value = "Tên khoa";
            ws.Cell(1, 3).Value = "Mô tả";
            ws.Cell(1, 4).Value = "Số điện thoại";
            ws.Cell(1, 5).Value = "Email";
            ws.Cell(1, 6).Value = "Địa điểm";
            ws.Cell(1, 7).Value = "Trạng thái";

            // 1 dòng dữ liệu hợp lệ
            ws.Cell(2, 1).SetValue("K_TEST");
            ws.Cell(2, 2).SetValue("Khoa Thử Nghiệm");
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test.xlsx");

        // Assert
        Assert.True(ketQua.LaHopLeToanBo);
        Assert.Empty(ketQua.DanhSachLoi);
        Assert.Equal(1, ketQua.TongSoDong);
        Assert.Equal(1, ketQua.SoDongHopLe);
    }

    [Fact]
    public async Task KiemTraHeader_ThayDoiThuTuCot_VanNhanDienHopLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Hoán đổi vị trí Tên khoa trước, Mã khoa sau
        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Tên khoa";
            ws.Cell(1, 2).Value = "Mã khoa";

            ws.Cell(2, 1).SetValue("Khoa Công nghệ");
            ws.Cell(2, 2).SetValue("K_CN");
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test.xlsx");

        // Assert
        Assert.True(ketQua.LaHopLeToanBo);
        Assert.Empty(ketQua.DanhSachLoi);
        Assert.Equal(1, ketQua.SoDongHopLe);
        var dong = ketQua.DanhSachDongDuLieu[0];
        Assert.Equal("K_CN", dong.DuLieuCot["Mã khoa"]);
        Assert.Equal("Khoa Công nghệ", dong.DuLieuCot["Tên khoa"]);
    }

    [Fact]
    public async Task KiemTraHeader_ThieuHeaderBatBuoc_BaoLoiChinhXac()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Thiếu cột "Tên khoa" (bắt buộc)
        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã khoa";
            ws.Cell(1, 2).Value = "Email";

            ws.Cell(2, 1).SetValue("K_01");
            ws.Cell(2, 2).SetValue("k1@test.local");
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi == "ERR_MISSING_REQUIRED_HEADER" && l.TenTruong == "Tên khoa");
    }

    [Fact]
    public async Task KiemTraHeader_HeaderTrungLap_BaoLoiTrung()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Cột "Mã khoa" bị xuất hiện 2 lần
        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã khoa";
            ws.Cell(1, 2).Value = "Tên khoa";
            ws.Cell(1, 3).Value = "Mã khoa";

            ws.Cell(2, 1).SetValue("K_01");
            ws.Cell(2, 2).SetValue("Khoa 1");
            ws.Cell(2, 3).SetValue("K_01");
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi == "ERR_DUPLICATE_HEADER");
    }

    [Fact]
    public async Task KiemTraHeader_HeaderLaKhongThuocSchema_BaoLoiCotLa()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var excelDichVu = new ExcelDichVu(db, nhatKy);

        // Có cột lạ "Cột không rõ"
        using var stream = TaoWorkbookInMemory(ws =>
        {
            ws.Cell(1, 1).Value = "Mã khoa";
            ws.Cell(1, 2).Value = "Tên khoa";
            ws.Cell(1, 3).Value = "Cột không rõ";

            ws.Cell(2, 1).SetValue("K_01");
            ws.Cell(2, 2).SetValue("Khoa 1");
            ws.Cell(2, 3).SetValue("123");
        });

        // Act
        var ketQua = await excelDichVu.KiemTraVaDocTepExcelAsync("Khoa", stream, "test.xlsx");

        // Assert
        Assert.False(ketQua.LaHopLeToanBo);
        Assert.Contains(ketQua.DanhSachLoi, l => l.MaLoi == "ERR_UNKNOWN_HEADER" && l.TenTruong == "Cột không rõ");
    }
}
