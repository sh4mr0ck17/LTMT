using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Bộ kiểm thử cho dịch vụ quản lý Khoa đào tạo (KhoaDichVu).
/// </summary>
public class KhoaDichVuTests
{
    [Fact]
    public async Task ThemKhoa_HopLe_LuuThanhCong()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new KhoaDichVu(db);
        var khoaMoi = new Khoa
        {
            MaKhoa = "KHOA_CNTT",
            TenKhoa = "Khoa Công nghệ Thông tin",
            Email = "cntt@test.local",
            CoHoatDong = true
        };

        // Act
        var ketQua = await dichVu.ThemKhoaAsync(khoaMoi);

        // Assert
        Assert.True(ketQua.KhoaId > 0);
        Assert.Equal("KHOA_CNTT", ketQua.MaKhoa);
        var khoaDb = await dichVu.LayTheoMaAsync("KHOA_CNTT");
        Assert.NotNull(khoaDb);
        Assert.Equal("Khoa Công nghệ Thông tin", khoaDb.TenKhoa);
    }

    [Fact]
    public async Task ThemKhoa_TrungMa_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new KhoaDichVu(db);
        await dichVu.ThemKhoaAsync(new Khoa { MaKhoa = "KHOA_01", TenKhoa = "Khoa 1" });

        var khoaTrung = new Khoa { MaKhoa = "KHOA_01", TenKhoa = "Khoa Trùng" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemKhoaAsync(khoaTrung));
        Assert.Contains("KHOA_01", ex.Message);
    }

    [Fact]
    public async Task CapNhatKhoa_KhongTonTai_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new KhoaDichVu(db);
        var khoaAo = new Khoa { KhoaId = 9999, MaKhoa = "KHOA_99", TenKhoa = "Khoa Không Tồn Tại" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => dichVu.CapNhatKhoaAsync(khoaAo));
    }

    [Fact]
    public async Task CapNhatKhoa_TrungMaVoiKhoaKhac_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new KhoaDichVu(db);
        var k1 = await dichVu.ThemKhoaAsync(new Khoa { MaKhoa = "KHOA_A", TenKhoa = "Khoa A" });
        var k2 = await dichVu.ThemKhoaAsync(new Khoa { MaKhoa = "KHOA_B", TenKhoa = "Khoa B" });

        // Cố tình sửa k2 thành mã của k1
        k2.MaKhoa = "KHOA_A";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.CapNhatKhoaAsync(k2));
    }

    [Fact]
    public async Task XoaKhoa_KhongCoDuLieuCon_XoaThanhCong()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new KhoaDichVu(db);
        var k = await dichVu.ThemKhoaAsync(new Khoa { MaKhoa = "KHOA_XOA", TenKhoa = "Khoa Cần Xóa" });

        // Act
        var ketQua = await dichVu.XoaKhoaAsync(k.KhoaId);

        // Assert
        Assert.True(ketQua);
        var khoaDb = await dichVu.LayTheoIdAsync(k.KhoaId);
        Assert.Null(khoaDb);
    }

    [Fact]
    public async Task XoaKhoa_CoBoMonTrucThuoc_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new KhoaDichVu(db);
        var k = await dichVu.ThemKhoaAsync(new Khoa { MaKhoa = "KHOA_BM", TenKhoa = "Khoa Có Bộ Môn" });

        db.BoMons.Add(new BoMon { MaBoMon = "BM_01", TenBoMon = "Bộ Môn 1", KhoaId = k.KhoaId });
        await db.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.XoaKhoaAsync(k.KhoaId));
        Assert.Contains("bộ môn", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task XoaKhoa_CoLopHocTrucThuoc_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new KhoaDichVu(db);
        var k = await dichVu.ThemKhoaAsync(new Khoa { MaKhoa = "KHOA_LH", TenKhoa = "Khoa Có Lớp Học" });

        db.LopHocs.Add(new LopHoc { MaLop = "L_01", TenLop = "Lớp 1", NienKhoa = "2024-2028", KhoaId = k.KhoaId });
        await db.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.XoaKhoaAsync(k.KhoaId));
        Assert.Contains("lớp học", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task XoaKhoa_CoNhanSuTrucThuoc_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new KhoaDichVu(db);
        var k = await dichVu.ThemKhoaAsync(new Khoa { MaKhoa = "KHOA_NS", TenKhoa = "Khoa Có Nhân Sự" });

        db.NhanSus.Add(new NhanSu
        {
            MaNhanSu = "NS_01",
            HoVaTen = "Nguyễn Văn A",
            EmailCongViec = "a@test.local",
            KhoaId = k.KhoaId
        });
        await db.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.XoaKhoaAsync(k.KhoaId));
        Assert.Contains("nhân sự", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LayDanhSach_LocTheoTuKhoa_TraVeDung()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new KhoaDichVu(db);
        await dichVu.ThemKhoaAsync(new Khoa { MaKhoa = "CNTT", TenKhoa = "Công nghệ thông tin" });
        await dichVu.ThemKhoaAsync(new Khoa { MaKhoa = "DTVT", TenKhoa = "Điện tử viễn thông" });

        // Act
        var ketQua = await dichVu.LayDanhSachAsync("Điện tử");

        // Assert
        Assert.Single(ketQua);
        Assert.Equal("DTVT", ketQua[0].MaKhoa);
    }
}
