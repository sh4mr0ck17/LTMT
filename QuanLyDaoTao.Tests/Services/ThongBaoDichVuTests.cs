using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Kiểm thử dịch vụ thông báo: phạm vi, mức độ, hạn hiển thị, ảnh đính kèm và theo dõi đã đọc.
/// </summary>
public class ThongBaoDichVuTests
{
    private sealed class PhanQuyenGia(NguoiDungHienTai nguoiDung) : IPhanQuyenDichVu
    {
        public NguoiDungHienTai NguoiDung { get; set; } = nguoiDung;

        public Task<NguoiDungHienTai> LayNguoiDungAsync() => Task.FromResult(NguoiDung);

        public Task<NguoiDungHienTai> DamBaoQuyenGhiAsync(string vung, int? khoaId) => Task.FromResult(NguoiDung);

        public Task<NguoiDungHienTai> DamBaoQuanTriAsync() => Task.FromResult(NguoiDung);
    }

    private static readonly NguoiDungHienTai Admin = new(true, "admin@test.local", VaiTroMacDinh.QuanTriVien, null, null);
    private static readonly NguoiDungHienTai NguoiXem = new(true, "xem@test.local", VaiTroMacDinh.NguoiXem, null, null);
    private static readonly NguoiDungHienTai QuanLy = new(true, "ql@test.local", VaiTroMacDinh.QuanLy, VungQuanLy.DaoTao, null);

    [Fact]
    public async Task DanhDauDaDoc_GiamSoChuaDocVaKhongTrungLap()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var phanQuyen = new PhanQuyenGia(Admin);
        var dichVu = new ThongBaoDichVu(db, phanQuyen);
        var tb = await dichVu.LuuAsync(new ThongBao { TieuDe = "Lịch họp", NoiDung = "Họp lúc 8h", MucDo = MucDoThongBao.Khan });

        Assert.Equal(1, await dichVu.DemChuaDocAsync(NguoiXem.TenDangNhap));
        Assert.Equal(0, await dichVu.DemChuaDocAsync(Admin.TenDangNhap));

        phanQuyen.NguoiDung = NguoiXem;
        await dichVu.DanhDauDaDocAsync(tb.ThongBaoId);
        await dichVu.DanhDauDaDocAsync(tb.ThongBaoId);

        Assert.Equal(0, await dichVu.DemChuaDocAsync(NguoiXem.TenDangNhap));
        var dong = Assert.Single(await dichVu.LayDanhSachAsync(new LocThongBao()));
        Assert.True(dong.DaDoc);
        Assert.Equal(1, dong.SoNguoiDoc);
    }

    [Fact]
    public async Task LayDanhSach_AnThongBaoHetHan_TruKhiBatBoLoc()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new ThongBaoDichVu(db, new PhanQuyenGia(Admin));
        db.ThongBaos.Add(new ThongBao { TieuDe = "Cũ", NoiDung = "x", NguoiDang = Admin.TenDangNhap, HetHanLuc = DateTime.UtcNow.AddDays(-1) });
        db.ThongBaos.Add(new ThongBao { TieuDe = "Mới", NoiDung = "y", NguoiDang = Admin.TenDangNhap });
        await db.SaveChangesAsync();

        Assert.Single(await dichVu.LayDanhSachAsync(new LocThongBao()));
        Assert.Equal(2, (await dichVu.LayDanhSachAsync(new LocThongBao { HienHetHan = true })).Count);
    }

    [Fact]
    public async Task Luu_QuanLyGuiToanHeThong_BiTuChoi()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new ThongBaoDichVu(db, new PhanQuyenGia(QuanLy));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            dichVu.LuuAsync(new ThongBao { TieuDe = "A", NoiDung = "B", PhamVi = "*" }));
    }

    [Fact]
    public async Task Luu_NguoiXem_BiTuChoi()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new ThongBaoDichVu(db, new PhanQuyenGia(NguoiXem));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => dichVu.LuuAsync(new ThongBao { TieuDe = "A", NoiDung = "B" }));
    }

    [Fact]
    public void TaoAnh_KiemTraChuKyTep()
    {
        byte[] png = [137, 80, 78, 71, 13, 10, 26, 10, 0, 0];
        Assert.StartsWith("data:image/png;base64,", ThongBaoDichVu.TaoAnh(png));
        Assert.Throws<InvalidOperationException>(() => ThongBaoDichVu.TaoAnh("<svg onload=x>"u8.ToArray()));
        Assert.Throws<InvalidOperationException>(() => ThongBaoDichVu.TaoAnh(new byte[ThongBaoDichVu.GioiHanAnh + 1]));
    }

    [Fact]
    public async Task LayDanhSach_GhimVaKhanLenTruoc()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new ThongBaoDichVu(db, new PhanQuyenGia(Admin));
        await dichVu.LuuAsync(new ThongBao { TieuDe = "Thường mới nhất", NoiDung = "a" });
        await dichVu.LuuAsync(new ThongBao { TieuDe = "Khẩn", NoiDung = "b", MucDo = MucDoThongBao.Khan });
        await dichVu.LuuAsync(new ThongBao { TieuDe = "Ghim", NoiDung = "c", DaGhim = true });

        var thuTu = (await dichVu.LayDanhSachAsync(new LocThongBao())).Select(d => d.ThongBao.TieuDe).ToList();

        Assert.Equal(["Ghim", "Khẩn", "Thường mới nhất"], thuTu);
    }
}
