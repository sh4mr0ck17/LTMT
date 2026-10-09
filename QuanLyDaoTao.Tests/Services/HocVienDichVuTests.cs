using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Bộ kiểm thử cho dịch vụ quản lý Học viên (HocVienDichVu).
/// </summary>
public class HocVienDichVuTests
{
    [Fact]
    public async Task ThemHocVien_LopKhongTonTai_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new HocVienDichVu(db);
        var hv = new HocVien
        {
            MaHocVien = "HV_01",
            HoVaTen = "Nguyễn Văn A",
            NgaySinh = new DateTime(2005, 5, 20),
            GioiTinh = "Nam",
            LopHocId = 9999
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemHocVienAsync(hv));
    }

    [Fact]
    public async Task ThemHocVien_HopLe_LuuThanhCong()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "K_01", TenKhoa = "Khoa 1" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();

        var lop = new LopHoc { MaLop = "L_01", TenLop = "Lớp 1", NienKhoa = "2024-2028", KhoaId = khoa.KhoaId };
        db.LopHocs.Add(lop);
        await db.SaveChangesAsync();

        var dichVu = new HocVienDichVu(db);
        var hv = new HocVien
        {
            MaHocVien = "HV2026001",
            HoVaTen = "Trần Văn Bình",
            NgaySinh = new DateTime(2005, 10, 15),
            GioiTinh = "Nam",
            Email = "binh@test.local",
            LopHocId = lop.LopHocId
        };

        // Act
        var ketQua = await dichVu.ThemHocVienAsync(hv);

        // Assert
        Assert.True(ketQua.HocVienId > 0);
        Assert.Equal("HV2026001", ketQua.MaHocVien);
    }

    [Fact]
    public async Task ThemHocVien_TrungMa_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "K_01", TenKhoa = "Khoa 1" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();

        var lop = new LopHoc { MaLop = "L_01", TenLop = "Lớp 1", NienKhoa = "2024-2028", KhoaId = khoa.KhoaId };
        db.LopHocs.Add(lop);
        await db.SaveChangesAsync();

        var dichVu = new HocVienDichVu(db);
        await dichVu.ThemHocVienAsync(new HocVien
        {
            MaHocVien = "HV_TRUNG",
            HoVaTen = "Học viên 1",
            NgaySinh = new DateTime(2005, 1, 1),
            GioiTinh = "Nam",
            LopHocId = lop.LopHocId
        });

        var hvTrung = new HocVien
        {
            MaHocVien = "HV_TRUNG",
            HoVaTen = "Học viên 2",
            NgaySinh = new DateTime(2005, 2, 2),
            GioiTinh = "Nữ",
            LopHocId = lop.LopHocId
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemHocVienAsync(hvTrung));
    }
}
