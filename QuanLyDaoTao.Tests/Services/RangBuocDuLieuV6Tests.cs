using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Ràng buộc dữ liệu đợt 6: một nguồn danh mục chuẩn, niên khóa thống nhất theo khóa,
/// và Studio ghi Cán bộ / Học viên / Lớp qua cùng quy tắc với biểu mẫu nghiệp vụ.
/// </summary>
public class RangBuocDuLieuV6Tests
{
    private static async Task<Khoa> TaoKhoaAsync(QuanLyDaoTao.Data.QuanLyDaoTaoDbContext db)
    {
        var khoa = new Khoa { MaKhoa = "KHOA_CNTT", TenKhoa = "CNTT" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();
        return khoa;
    }

    [Theory]
    [InlineData("2025-2029", "2025-2029")]
    [InlineData(" 2025 – 2029 ", "2025-2029")]
    [InlineData("2025-2025", null)]
    [InlineData("2029-2025", null)]
    [InlineData("2025-2040", null)]
    [InlineData("K57", null)]
    public void ChuanHoaNienKhoa_ChiNhanDangYyyyYyyyHopLy(string vao, string? ra) =>
        Assert.Equal(ra, QuyTacNghiepVu.ChuanHoaNienKhoa(vao));

    [Fact]
    public async Task LopCungKhoa_PhaiCungNienKhoa_DeTrongThiTuLayTheoKhoa()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = await TaoKhoaAsync(db);
        var dichVu = new LopHocDichVu(db);
        await dichVu.ThemLopHocAsync(new LopHoc { MaLop = "B1D57", KhoaId = khoa.KhoaId, NienKhoa = "2025-2029", SoLuongToiDa = 50 });

        var lopTuLay = await dichVu.ThemLopHocAsync(new LopHoc { MaLop = "B2D57", KhoaId = khoa.KhoaId, NienKhoa = "", SoLuongToiDa = 50 });
        Assert.Equal("D57", lopTuLay.KhoaHoc);
        Assert.Equal("2025-2029", lopTuLay.NienKhoa);

        var loi = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dichVu.ThemLopHocAsync(new LopHoc { MaLop = "B3D57", KhoaId = khoa.KhoaId, NienKhoa = "2024-2028", SoLuongToiDa = 50 }));
        Assert.Contains("cùng niên khóa", loi.Message);
    }

    [Fact]
    public async Task Lop_HeNgoaiDanhMuc_BiChan()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = await TaoKhoaAsync(db);
        await Assert.ThrowsAsync<InvalidOperationException>(() => new LopHocDichVu(db)
            .ThemLopHocAsync(new LopHoc { MaLop = "TC01", HeDaoTao = "Tại chức tự đặt", KhoaId = khoa.KhoaId, NienKhoa = "2025-2029", SoLuongToiDa = 40 }));
    }

    [Fact]
    public async Task CanBo_TrangThaiNgoaiDanhSach_BiChan()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new NhanSuDichVu(db, new NhatKyDichVu(db));
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemNhanSuAsync(new NhanSu
        {
            MaNhanSu = "CB01", HoVaTen = "Nguyễn Văn A", NgaySinh = new DateTime(1985, 1, 1), GioiTinh = "Nam",
            Email = "a@c500.edu.vn", TrangThai = "Đang công tác ở đâu đó"
        }));
    }

    [Fact]
    public async Task Studio_SuaCanBo_DiQuaQuyTacNghiepVu_VaLuuDungEmailCccd()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var nhanSu = new NhanSuDichVu(db, nhatKy);
        var studio = new QuanTriDuLieuDichVu(db, nhatKy, nhanSu, new HocVienDichVu(db), new LopHocDichVu(db));
        var ns = await nhanSu.ThemNhanSuAsync(new NhanSu
        {
            MaNhanSu = "CB01", HoVaTen = "Nguyễn Văn A", NgaySinh = new DateTime(1985, 1, 1), GioiTinh = "Nam", Email = "a@c500.edu.vn"
        });
        var id = ns.NhanSuId.ToString();

        // Trước đây Studio đọc theo tên bí danh nên sửa email / CCCD bị bỏ qua âm thầm.
        await studio.CapNhatNhanhBanGhiAsync("NhanSu", id, new Dictionary<string, string?> { ["Email"] = "moi@c500.edu.vn", ["SoCCCD"] = "001085001234" }, "tester");
        var sauSua = await db.NhanSus.AsNoTracking().SingleAsync();
        Assert.Equal("moi@c500.edu.vn", sauSua.Email);
        Assert.Equal("001085001234", sauSua.SoCCCD);

        // Quy tắc nghiệp vụ được áp dụng y như biểu mẫu.
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            studio.CapNhatNhanhBanGhiAsync("NhanSu", id, new Dictionary<string, string?> { ["SoCCCD"] = "123" }, "tester"));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            studio.CapNhatNhanhBanGhiAsync("NhanSu", id, new Dictionary<string, string?> { ["GioiTinh"] = "Khác" }, "tester"));
    }

    [Fact]
    public async Task Studio_KhongCoDichVuNghiepVu_TuChoiGhiThangBangNghiepVu()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var studio = new QuanTriDuLieuDichVu(db, new NhatKyDichVu(db));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            studio.ThemNhanhBanGhiAsync("HocVien", new Dictionary<string, string?> { ["HoVaTen"] = "A" }, "tester"));
    }

    [Fact]
    public async Task Studio_CotHocVien_KhopBieuMau_CoCccdChucVuTrangThai()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var bang = await new QuanTriDuLieuDichVu(db, new NhatKyDichVu(db)).LayDuLieuBangTrucTuyenAsync("HocVien");
        var cot = bang.DanhSachCot.Select(c => c.MaCot).ToList();
        Assert.Contains("SoCCCD", cot);
        Assert.Contains("ChucVuLop", cot);
        Assert.Contains("TrangThai", cot);
        Assert.DoesNotContain("CoHoatDong", cot);
        Assert.Equal(QuyTacNghiepVu.DanhSachGioiTinh, bang.DanhSachCot.Single(c => c.MaCot == "GioiTinh").DanhSachLuaChon.Select(l => l.GiaTri));
    }
}
