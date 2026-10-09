using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Bộ kiểm thử cho dịch vụ quản lý Lớp học (LopHocDichVu).
/// </summary>
public class LopHocDichVuTests
{
    [Fact]
    public async Task ThemLopHoc_HopLe_LuuThanhCong()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "K_01", TenKhoa = "Khoa 1" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();

        var dichVu = new LopHocDichVu(db);
        var lop = new LopHoc
        {
            MaLop = "CNTT_K20A",
            TenLop = "Lớp K20A",
            NienKhoa = "2024-2028",
            KhoaId = khoa.KhoaId,
            SoLuongToiDa = 50
        };

        // Act
        var ketQua = await dichVu.ThemLopHocAsync(lop);

        // Assert
        Assert.True(ketQua.LopHocId > 0);
        Assert.Equal("CNTT_K20A", ketQua.MaLop);
    }

    [Fact]
    public async Task ThemLopHoc_TrungMa_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "K_01", TenKhoa = "Khoa 1" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();

        var dichVu = new LopHocDichVu(db);
        await dichVu.ThemLopHocAsync(new LopHoc { MaLop = "L_01", TenLop = "Lớp 1", NienKhoa = "2024-2028", KhoaId = khoa.KhoaId });

        var lopTrung = new LopHoc { MaLop = "L_01", TenLop = "Lớp Trùng", NienKhoa = "2024-2028", KhoaId = khoa.KhoaId };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemLopHocAsync(lopTrung));
    }

    [Fact]
    public async Task XoaLopHoc_CoHocVien_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "K_01", TenKhoa = "Khoa 1" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();

        var dichVu = new LopHocDichVu(db);
        var lop = await dichVu.ThemLopHocAsync(new LopHoc { MaLop = "L_01", TenLop = "Lớp 1", NienKhoa = "2024-2028", KhoaId = khoa.KhoaId });

        db.HocViens.Add(new HocVien
        {
            MaHocVien = "HV_01",
            HoVaTen = "Học viên A",
            NgaySinh = new DateTime(2005, 1, 1),
            GioiTinh = "Nam",
            LopHocId = lop.LopHocId
        });
        await db.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.XoaLopHocAsync(lop.LopHocId));
    }

    [Fact]
    public async Task LayDanhSachNienKhoa_TraVePhanBiet()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "K_01", TenKhoa = "Khoa 1" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();

        var dichVu = new LopHocDichVu(db);
        await dichVu.ThemLopHocAsync(new LopHoc { MaLop = "L_01", TenLop = "Lớp 1", NienKhoa = "2024-2028", KhoaId = khoa.KhoaId });
        await dichVu.ThemLopHocAsync(new LopHoc { MaLop = "L_02", TenLop = "Lớp 2", NienKhoa = "2024-2028", KhoaId = khoa.KhoaId });
        await dichVu.ThemLopHocAsync(new LopHoc { MaLop = "L_03", TenLop = "Lớp 3", NienKhoa = "2023-2027", KhoaId = khoa.KhoaId });

        // Act
        var nienKhoas = await dichVu.LayDanhSachNienKhoaAsync();

        // Assert
        Assert.Equal(2, nienKhoas.Count);
        Assert.Contains("2024-2028", nienKhoas);
        Assert.Contains("2023-2027", nienKhoas);
    }
}
