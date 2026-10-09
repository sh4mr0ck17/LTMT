using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Bộ kiểm thử cho dịch vụ quản lý Chức vụ (ChucVuDichVu).
/// </summary>
public class ChucVuDichVuTests
{
    [Fact]
    public async Task ThemChucVu_HopLe_LuuThanhCong()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new ChucVuDichVu(db);

        var cv = new ChucVu { MaChucVu = "CV_TK", TenChucVu = "Trưởng khoa" };

        // Act
        var ketQua = await dichVu.ThemChucVuAsync(cv);

        // Assert
        Assert.True(ketQua.ChucVuId > 0);
        Assert.Equal("CV_TK", ketQua.MaChucVu);
    }

    [Fact]
    public async Task ThemChucVu_TrungMa_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new ChucVuDichVu(db);
        await dichVu.ThemChucVuAsync(new ChucVu { MaChucVu = "CV_01", TenChucVu = "Chức vụ 1" });

        var cvTrung = new ChucVu { MaChucVu = "CV_01", TenChucVu = "Chức vụ trùng" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemChucVuAsync(cvTrung));
    }
}
