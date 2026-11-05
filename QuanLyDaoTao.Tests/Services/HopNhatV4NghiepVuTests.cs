using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Kiểm thử các quy tắc nghiệp vụ và phân quyền hợp nhất từ LTMT-demo (v4).
/// </summary>
public class HopNhatV4NghiepVuTests
{
    /// <summary>
    /// Dịch vụ phân quyền giả trả về người dùng cố định.
    /// </summary>
    private sealed class PhanQuyenGia(NguoiDungHienTai nguoiDung) : IPhanQuyenDichVu
    {
        public Task<NguoiDungHienTai> LayNguoiDungAsync() => Task.FromResult(nguoiDung);

        public Task<NguoiDungHienTai> DamBaoQuyenGhiAsync(string vung, int? khoaId) =>
            nguoiDung.CoQuyenGhi(vung, khoaId)
                ? Task.FromResult(nguoiDung)
                : throw new UnauthorizedAccessException("Không có quyền.");

        public Task<NguoiDungHienTai> DamBaoQuanTriAsync() =>
            nguoiDung.LaQuanTri ? Task.FromResult(nguoiDung) : throw new UnauthorizedAccessException("Không có quyền.");
    }

    private static NguoiDungHienTai QuanLy(string vung, int? khoaId = null) =>
        new(true, "quanly@test.local", VaiTroMacDinh.QuanLy, vung, khoaId);

    private static readonly NguoiDungHienTai NguoiXem = new(true, "xem@test.local", VaiTroMacDinh.NguoiXem, null, null);

    private static async Task<(Khoa Khoa, LopHoc Lop)> TaoKhoaVaLopAsync(QuanLyDaoTaoDbContext db, int siSoToiDa = 50)
    {
        var khoa = new Khoa { MaKhoa = "K1", TenKhoa = "Khoa 1" };
        db.Khoas.Add(khoa);
        await db.SaveChangesAsync();
        var lop = new LopHoc { MaLop = "B1D57", TenLop = "Lớp B1D57", NienKhoa = "2025-2029", KhoaId = khoa.KhoaId, KhoaHoc = "D57", SoLuongToiDa = siSoToiDa };
        db.LopHocs.Add(lop);
        await db.SaveChangesAsync();
        return (khoa, lop);
    }

    private static HocVien TaoHocVien(string ma, int lopId, string? chucVu = null, string? cccd = null, string? sdt = null) => new()
    {
        MaHocVien = ma,
        HoVaTen = "Nguyễn Văn " + ma,
        NgaySinh = new DateTime(2005, 1, 1),
        GioiTinh = "Nam",
        LopHocId = lopId,
        ChucVuLop = chucVu,
        SoCCCD = cccd,
        SoDienThoai = sdt
    };

    [Theory]
    [InlineData("AT, PBTLCĐ", new[] { "AT", "PBTLCĐ" })]
    [InlineData("B trưởng", new[] { "BT" })]
    [InlineData("A phó; BP ĐL", new[] { "AP", "BPĐL" })]
    [InlineData("Học viên", new string[0])]
    public void TachChucVuLop_ChuanHoaVietTatVaTenDayDu(string dauVao, string[] kyVong)
    {
        Assert.Equal(kyVong, QuyTacNghiepVu.TachChucVuLop(dauVao));
    }

    [Theory]
    [InlineData("B12D57", "Chính quy", "D57", true)]
    [InlineData("B12D56", "Chính quy", "D57", false)]
    [InlineData("LOP_A", "Chính quy", "D57", false)]
    [InlineData("LOP_A", "Chính quy", null, true)]
    [InlineData("B3K6", "Văn bằng 2", "K6", true)]
    [InlineData("MA CO KHOANG", "Văn bằng 2", null, false)]
    public void KiemTraMaLop_TheoHeVaKhoa(string maLop, string he, string? khoa, bool hopLe)
    {
        Assert.Equal(hopLe, QuyTacNghiepVu.KiemTraMaLop(maLop, he, khoa) is null);
    }

    [Fact]
    public async Task ThemHocVien_VuotChiTieuBTruong_BiChan()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var (_, lop) = await TaoKhoaVaLopAsync(db);
        var dichVu = new HocVienDichVu(db);
        await dichVu.ThemHocVienAsync(TaoHocVien("HV1", lop.LopHocId, "BT"));

        var loi = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dichVu.ThemHocVienAsync(TaoHocVien("HV2", lop.LopHocId, "B trưởng")));
        Assert.Contains("B trưởng", loi.Message);
    }

    [Fact]
    public async Task CapNhatHocVien_GiuChucVuCuDaVuotChiTieu_VanLuuDuoc()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var (_, lop) = await TaoKhoaVaLopAsync(db);
        // Dữ liệu cũ đã có 2 B trưởng (vượt chỉ tiêu) trước khi áp quy tắc.
        db.HocViens.AddRange(TaoHocVien("HV1", lop.LopHocId, "BT"), TaoHocVien("HV2", lop.LopHocId, "BT"));
        await db.SaveChangesAsync();
        var dichVu = new HocVienDichVu(db);

        var hv = db.HocViens.Single(h => h.MaHocVien == "HV2");
        var banSua = TaoHocVien("HV2", lop.LopHocId, "BT");
        banSua.HocVienId = hv.HocVienId;
        banSua.HoVaTen = "Tên đã sửa";

        var ketQua = await dichVu.CapNhatHocVienAsync(banSua);
        Assert.Equal("Tên đã sửa", ketQua.HoVaTen);
    }

    [Fact]
    public async Task ThemHocVien_LopDaDuSiSo_BiChan()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var (_, lop) = await TaoKhoaVaLopAsync(db, siSoToiDa: 1);
        var dichVu = new HocVienDichVu(db);
        await dichVu.ThemHocVienAsync(TaoHocVien("HV1", lop.LopHocId));

        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemHocVienAsync(TaoHocVien("HV2", lop.LopHocId)));
    }

    [Theory]
    [InlineData("12345678901", null)]
    [InlineData(null, "0123456789")]
    [InlineData(null, "091234567")]
    public async Task ThemHocVien_SaiDinhDangCccdHoacSdt_BiChan(string? cccd, string? sdt)
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var (_, lop) = await TaoKhoaVaLopAsync(db);
        var dichVu = new HocVienDichVu(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => dichVu.ThemHocVienAsync(TaoHocVien("HV1", lop.LopHocId, cccd: cccd, sdt: sdt)));
    }

    [Fact]
    public async Task ThemHocVien_TrungCccdVoiCanBo_BiChan()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var (khoa, lop) = await TaoKhoaVaLopAsync(db);
        db.NhanSus.Add(new NhanSu { MaNhanSu = "CB1", HoVaTen = "Cán bộ", Email = "cb1@test.local", NgaySinh = new DateTime(1980, 1, 1), SoCCCD = "001080000001", KhoaId = khoa.KhoaId });
        await db.SaveChangesAsync();
        var dichVu = new HocVienDichVu(db);

        var loi = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dichVu.ThemHocVienAsync(TaoHocVien("HV1", lop.LopHocId, cccd: "001080000001")));
        Assert.Contains("cán bộ", loi.Message);
    }

    [Fact]
    public async Task ThemHocVien_QuanLyVungToChuc_BiTuChoi()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var (_, lop) = await TaoKhoaVaLopAsync(db);
        var dichVu = new HocVienDichVu(db, new PhanQuyenGia(QuanLy(VungQuanLy.ToChuc)));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => dichVu.ThemHocVienAsync(TaoHocVien("HV1", lop.LopHocId)));
    }

    [Fact]
    public async Task ThemHocVien_QuanLyDaoTaoKhacKhoa_BiTuChoi_DungKhoa_ThanhCong()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var (khoa, lop) = await TaoKhoaVaLopAsync(db);

        var dichVuKhacKhoa = new HocVienDichVu(db, new PhanQuyenGia(QuanLy(VungQuanLy.DaoTao, khoa.KhoaId + 100)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => dichVuKhacKhoa.ThemHocVienAsync(TaoHocVien("HV1", lop.LopHocId)));

        var dichVuDungKhoa = new HocVienDichVu(db, new PhanQuyenGia(QuanLy(VungQuanLy.DaoTao, khoa.KhoaId)));
        var ketQua = await dichVuDungKhoa.ThemHocVienAsync(TaoHocVien("HV2", lop.LopHocId));
        Assert.True(ketQua.HocVienId > 0);
    }

    [Fact]
    public async Task LayDanhSachHocVien_NguoiXem_CheThongTinCaNhan()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var (_, lop) = await TaoKhoaVaLopAsync(db);
        var hv = TaoHocVien("HV1", lop.LopHocId, cccd: "001205001234", sdt: "0912345678");
        hv.Email = "hv1@test.local";
        hv.DiaChi = "Phường A, Hà Nội";
        db.HocViens.Add(hv);
        await db.SaveChangesAsync();

        var danhSach = await new HocVienDichVu(db, new PhanQuyenGia(NguoiXem)).LayDanhSachAsync();

        var ketQua = Assert.Single(danhSach);
        Assert.EndsWith("234", ketQua.SoCCCD);
        Assert.DoesNotContain("001205", ketQua.SoCCCD);
        Assert.DoesNotContain("0912", ketQua.SoDienThoai);
        Assert.Equal("Hà Nội", ketQua.DiaChi);
        Assert.Equal("001205001234", db.HocViens.Single().SoCCCD);
    }

    [Fact]
    public async Task ThemNhanSu_KhoaDaCoTruongKhoa_BiChan()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa = new Khoa { MaKhoa = "K1", TenKhoa = "Khoa 1" };
        var truongKhoa = new ChucVu { MaChucVu = "CV_TK", TenChucVu = "Trưởng khoa" };
        db.AddRange(khoa, truongKhoa);
        await db.SaveChangesAsync();
        var dichVu = new NhanSuDichVu(db, new NhatKyDichVu(db));

        NhanSu Tao(string ma) => new()
        {
            MaNhanSu = ma, HoVaTen = "Cán bộ " + ma, Email = ma + "@test.local", NgaySinh = new DateTime(1980, 1, 1),
            KhoaId = khoa.KhoaId, ChucVuId = truongKhoa.ChucVuId
        };

        await dichVu.ThemNhanSuAsync(Tao("CB1"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemNhanSuAsync(Tao("CB2")));
    }

    [Fact]
    public async Task ThemNhanSu_BoMonKhongThuocKhoa_BiChan()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var khoa1 = new Khoa { MaKhoa = "K1", TenKhoa = "Khoa 1" };
        var khoa2 = new Khoa { MaKhoa = "K2", TenKhoa = "Khoa 2" };
        db.AddRange(khoa1, khoa2);
        await db.SaveChangesAsync();
        var boMon = new BoMon { MaBoMon = "BM1", TenBoMon = "Bộ môn 1", KhoaId = khoa2.KhoaId };
        db.Add(boMon);
        await db.SaveChangesAsync();
        var dichVu = new NhanSuDichVu(db, new NhatKyDichVu(db));

        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.ThemNhanSuAsync(new NhanSu
        {
            MaNhanSu = "CB1", HoVaTen = "Cán bộ", Email = "cb1@test.local", NgaySinh = new DateTime(1980, 1, 1),
            KhoaId = khoa1.KhoaId, BoMonId = boMon.BoMonId
        }));
    }

    [Fact]
    public async Task CapNhatNhanSu_HocHamKhongGhiDeChuyenMon()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new NhanSuDichVu(db, new NhatKyDichVu(db));
        var ns = await dichVu.ThemNhanSuAsync(new NhanSu
        {
            MaNhanSu = "CB1", HoVaTen = "Cán bộ", Email = "cb1@test.local", NgaySinh = new DateTime(1980, 1, 1),
            ChuyenMonDaoTao = "An ninh mạng"
        });

        var banSua = new NhanSu
        {
            NhanSuId = ns.NhanSuId, MaNhanSu = "CB1", HoVaTen = "Cán bộ", Email = "cb1@test.local", NgaySinh = new DateTime(1980, 1, 1),
            ChuyenMonDaoTao = "An ninh mạng", HocHam = "Phó giáo sư", CapBacHam = "Trung tá"
        };
        var ketQua = await dichVu.CapNhatNhanSuAsync(banSua);

        Assert.Equal("An ninh mạng", ketQua.ChuyenMonDaoTao);
        Assert.Equal("Phó giáo sư", ketQua.HocHam);
        Assert.Equal("Trung tá", ketQua.CapBacHam);
    }

    [Fact]
    public async Task ThemKhoa_QuanLyKhongPhaiQuanTri_BiTuChoi()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var dichVu = new KhoaDichVu(db, new PhanQuyenGia(QuanLy(VungQuanLy.ToChuc)));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => dichVu.ThemKhoaAsync(new Khoa { MaKhoa = "K9", TenKhoa = "Khoa 9" }));
    }

    [Fact]
    public async Task CapNhatLop_HaSiSoToiDaDuoiSiSoThucTe_BiChan()
    {
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var (_, lop) = await TaoKhoaVaLopAsync(db);
        db.HocViens.AddRange(TaoHocVien("HV1", lop.LopHocId), TaoHocVien("HV2", lop.LopHocId));
        await db.SaveChangesAsync();
        var dichVu = new LopHocDichVu(db);

        var banSua = new LopHoc
        {
            LopHocId = lop.LopHocId, MaLop = lop.MaLop, TenLop = lop.TenLop, NienKhoa = lop.NienKhoa,
            KhoaId = lop.KhoaId, KhoaHoc = "D57", SoLuongToiDa = 1
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() => dichVu.CapNhatLopHocAsync(banSua));
    }

    [Fact]
    public void NguoiDungHienTai_QuanLyGioiHanKhoa_KhongGhiBanGhiKhongGanKhoa()
    {
        var quanLy = QuanLy(VungQuanLy.ToChuc, khoaId: 3);
        Assert.True(quanLy.CoQuyenGhi(VungQuanLy.ToChuc, 3));
        Assert.False(quanLy.CoQuyenGhi(VungQuanLy.ToChuc, 4));
        Assert.False(quanLy.CoQuyenGhi(VungQuanLy.ToChuc, null));
        Assert.False(quanLy.CoQuyenGhi(VungQuanLy.DaoTao, 3));
        Assert.False(NguoiXem.CoQuyenGhi(VungQuanLy.DaoTao, 3));
    }
}
