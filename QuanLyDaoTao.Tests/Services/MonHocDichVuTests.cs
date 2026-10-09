using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Bộ kiểm thử cho dịch vụ quản lý Môn học (MonHocDichVu).
/// </summary>
public class MonHocDichVuTests
{
    [Fact]
    public async Task ThemMonHoc_HopLe_LuuThanhCong()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new MonHocDichVu(db);

        var mh = new MonHoc
        {
            MaMonHoc = "CSHARP_NET",
            TenMonHoc = "Lập trình C#",
            SoTinChi = 3,
            SoTietLyThuyet = 30,
            SoTietThucHanh = 15
        };

        // Act
        var ketQua = await dichVu.ThemMonHocAsync(mh);

        // Assert
        Assert.True(ketQua.MonHocId > 0);
        Assert.Equal("CSHARP_NET", ketQua.MaMonHoc);
    }

    [Fact]
    public async Task ThemMonHoc_TrungMa_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new MonHocDichVu(db);
        await dichVu.ThemMonHocAsync(new MonHoc { MaMonHoc = "MH_01", TenMonHoc = "Môn 1" });

        var mhTrung = new MonHoc { MaMonHoc = "MH_01", TenMonHoc = "Môn Trùng" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemMonHocAsync(mhTrung));
    }
}
