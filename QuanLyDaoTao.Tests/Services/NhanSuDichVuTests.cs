using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Bộ kiểm thử cho dịch vụ quản lý Hồ sơ Nhân sự (NhanSuDichVu).
/// </summary>
public class NhanSuDichVuTests
{
    [Fact]
    public async Task ThemNhanSu_HopLe_LuuThanhCong()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKyDichVu = new NhatKyDichVu(db);
        var dichVu = new NhanSuDichVu(db, nhatKyDichVu);

        var ns = new NhanSu
        {
            MaNhanSu = "NS0001",
            HoVaTen = "Nguyễn Văn Tuấn",
            NgaySinh = new DateTime(1985, 3, 10),
            GioiTinh = "Nam",
            EmailCongViec = "tuan.nv@test.local",
            TrangThaiCongTac = "Đang làm việc"
        };

        // Act
        var ketQua = await dichVu.ThemNhanSuAsync(ns);

        // Assert
        Assert.True(ketQua.NhanSuId > 0);
        Assert.Equal("NS0001", ketQua.MaNhanSu);
    }

    [Fact]
    public async Task ThemNhanSu_TrungMa_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKyDichVu = new NhatKyDichVu(db);
        var dichVu = new NhanSuDichVu(db, nhatKyDichVu);

        await dichVu.ThemNhanSuAsync(new NhanSu
        {
            MaNhanSu = "NS_TRUNG",
            HoVaTen = "Người 1",
            NgaySinh = new DateTime(1985, 1, 1),
            GioiTinh = "Nam",
            EmailCongViec = "email1@test.local"
        });

        var nsTrung = new NhanSu
        {
            MaNhanSu = "NS_TRUNG",
            HoVaTen = "Người 2",
            NgaySinh = new DateTime(1986, 2, 2),
            GioiTinh = "Nữ",
            EmailCongViec = "email2@test.local"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemNhanSuAsync(nsTrung));
    }

    [Fact]
    public async Task ThemNhanSu_TrungEmailCongViec_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKyDichVu = new NhatKyDichVu(db);
        var dichVu = new NhanSuDichVu(db, nhatKyDichVu);

        await dichVu.ThemNhanSuAsync(new NhanSu
        {
            MaNhanSu = "NS_01",
            HoVaTen = "Người 1",
            NgaySinh = new DateTime(1985, 1, 1),
            GioiTinh = "Nam",
            EmailCongViec = "trung.email@test.local"
        });

        var nsTrungEmail = new NhanSu
        {
            MaNhanSu = "NS_02",
            HoVaTen = "Người 2",
            NgaySinh = new DateTime(1986, 2, 2),
            GioiTinh = "Nữ",
            EmailCongViec = "trung.email@test.local"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemNhanSuAsync(nsTrungEmail));
    }

    [Fact]
    public async Task XoaNhanSu_CoPhanCongGiangDay_NemNgoaiLe()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKyDichVu = new NhatKyDichVu(db);
        var dichVu = new NhanSuDichVu(db, nhatKyDichVu);

        var ns = await dichVu.ThemNhanSuAsync(new NhanSu
        {
            MaNhanSu = "NS_PC",
            HoVaTen = "Giảng viên A",
            NgaySinh = new DateTime(1980, 1, 1),
            GioiTinh = "Nam",
            EmailCongViec = "gv.a@test.local"
        });

        var khoa = new Khoa { MaKhoa = "K_01", TenKhoa = "Khoa 1" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();

        var lop = new LopHoc { MaLop = "L_01", TenLop = "Lớp 1", NienKhoa = "2024-2028", KhoaId = khoa.KhoaId };
        var mh = new MonHoc { MaMonHoc = "M_01", TenMonHoc = "Môn 1" };
        db.LopHocs.Add(lop);
        db.MonHocs.Add(mh);
        await db.SaveChangesAsync();

        db.PhanCongGiangDays.Add(new PhanCongGiangDay
        {
            MaPhanCong = "PC_01",
            NhanSuId = ns.NhanSuId,
            MonHocId = mh.MonHocId,
            LopHocId = lop.LopHocId,
            HocKy = "Học kỳ 1",
            NamHoc = "2026-2027"
        });
        await db.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.XoaNhanSuAsync(ns.NhanSuId));
    }
}
