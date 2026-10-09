using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Bộ kiểm thử cho dịch vụ quản lý Trình độ chuyên môn (TrinhDoChuyenMonDichVu).
/// </summary>
public class TrinhDoChuyenMonDichVuTests
{
    [Fact]
    public async Task ThemTrinhDo_HopLe_LuuThanhCong()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new TrinhDoChuyenMonDichVu(db);

        var td = new TrinhDoChuyenMon { MaTrinhDo = "TD_TS", TenTrinhDo = "Tiến sĩ" };

        // Act
        var ketQua = await dichVu.ThemTrinhDoAsync(td);

        // Assert
        Assert.True(ketQua.TrinhDoChuyenMonId > 0);
        Assert.Equal("TD_TS", ketQua.MaTrinhDo);
    }

    [Fact]
    public async Task ThemTrinhDo_TrungMa_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new TrinhDoChuyenMonDichVu(db);
        await dichVu.ThemTrinhDoAsync(new TrinhDoChuyenMon { MaTrinhDo = "TD_01", TenTrinhDo = "Tiến sĩ" });

        var tdTrung = new TrinhDoChuyenMon { MaTrinhDo = "TD_01", TenTrinhDo = "Tiến sĩ trùng" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemTrinhDoAsync(tdTrung));
    }
}
