using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Bộ kiểm thử cho dịch vụ quản lý Bộ môn (BoMonDichVu).
/// </summary>
public class BoMonDichVuTests
{
    [Fact]
    public async Task ThemBoMon_KhoaKhongTonTai_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new BoMonDichVu(db);
        var boMon = new BoMon { MaBoMon = "BM_01", TenBoMon = "Bộ Môn 1", KhoaId = 9999 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemBoMonAsync(boMon));
    }

    [Fact]
    public async Task ThemBoMon_HopLe_LuuThanhCong()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "K_01", TenKhoa = "Khoa 1" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();

        var dichVu = new BoMonDichVu(db);
        var boMon = new BoMon { MaBoMon = "BM_01", TenBoMon = "Bộ Môn 1", KhoaId = khoa.KhoaId };

        // Act
        var ketQua = await dichVu.ThemBoMonAsync(boMon);

        // Assert
        Assert.True(ketQua.BoMonId > 0);
        Assert.Equal("BM_01", ketQua.MaBoMon);
    }

    [Fact]
    public async Task ThemBoMon_TrungMa_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "K_01", TenKhoa = "Khoa 1" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();

        var dichVu = new BoMonDichVu(db);
        await dichVu.ThemBoMonAsync(new BoMon { MaBoMon = "BM_01", TenBoMon = "Bộ Môn 1", KhoaId = khoa.KhoaId });

        var boMonTrung = new BoMon { MaBoMon = "BM_01", TenBoMon = "Bộ Môn Trùng", KhoaId = khoa.KhoaId };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemBoMonAsync(boMonTrung));
    }

    [Fact]
    public async Task XoaBoMon_CoNhanSu_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "K_01", TenKhoa = "Khoa 1" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();

        var dichVu = new BoMonDichVu(db);
        var bm = await dichVu.ThemBoMonAsync(new BoMon { MaBoMon = "BM_01", TenBoMon = "Bộ Môn 1", KhoaId = khoa.KhoaId });

        db.NhanSus.Add(new NhanSu
        {
            MaNhanSu = "NS_01",
            HoVaTen = "Nguyễn Văn B",
            EmailCongViec = "b@test.local",
            BoMonId = bm.BoMonId
        });
        await db.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.XoaBoMonAsync(bm.BoMonId));
    }
}
