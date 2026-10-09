using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Bộ kiểm thử cho dịch vụ quản lý Phân công giảng dạy (PhanCongGiangDayDichVu).
/// </summary>
public class PhanCongGiangDayDichVuTests
{
    [Fact]
    public async Task ThemPhanCong_NhanSuKhongTonTai_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new PhanCongGiangDayDichVu(db);

        var pc = new PhanCongGiangDay
        {
            MaPhanCong = "PC_01",
            NhanSuId = 9999,
            MonHocId = 1,
            LopHocId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemPhanCongAsync(pc));
    }

    [Fact]
    public async Task ThemPhanCong_HopLe_LuuThanhCong()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "K_01", TenKhoa = "Khoa 1" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();

        var lop = new LopHoc { MaLop = "L_01", TenLop = "Lớp 1", NienKhoa = "2024-2028", KhoaId = khoa.KhoaId };
        var mh = new MonHoc { MaMonHoc = "M_01", TenMonHoc = "Môn 1" };
        var ns = new NhanSu { MaNhanSu = "NS_01", HoVaTen = "Giảng viên A", EmailCongViec = "a@test.local" };
        db.LopHocs.Add(lop);
        db.MonHocs.Add(mh);
        db.NhanSus.Add(ns);
        await db.SaveChangesAsync();

        var dichVu = new PhanCongGiangDayDichVu(db);
        var pc = new PhanCongGiangDay
        {
            MaPhanCong = "PC2026_01",
            NhanSuId = ns.NhanSuId,
            MonHocId = mh.MonHocId,
            LopHocId = lop.LopHocId,
            HocKy = "Học kỳ 1",
            NamHoc = "2026-2027",
            VaiTroGiangDay = "Giảng viên chính"
        };

        // Act
        var ketQua = await dichVu.ThemPhanCongAsync(pc);

        // Assert
        Assert.True(ketQua.PhanCongId > 0);
        Assert.Equal("PC2026_01", ketQua.MaPhanCong);
    }
}
