using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.Tests.Helpers;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>
/// Bộ kiểm thử cho dịch vụ Quản trị dữ liệu trực tuyến (QuanTriDuLieuDichVu) trên tất cả 11 nhóm hệ thống.
/// </summary>
public class QuanTriDuLieuDichVuTests
{
    [Fact]
    public async Task LayDanhSachBangHeThong_TraVeDayDu11Nhom()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var dichVu = new QuanTriDuLieuDichVu(db, nhatKy);

        // Act
        var danhSach = await dichVu.LayDanhSachBangHeThongAsync();

        // Assert
        Assert.Equal(11, danhSach.Count);
        Assert.Contains(danhSach, x => x.TenBang == "Khoa");
        Assert.Contains(danhSach, x => x.TenBang == "BoMon");
        Assert.Contains(danhSach, x => x.TenBang == "LopHoc");
        Assert.Contains(danhSach, x => x.TenBang == "HocVien");
        Assert.Contains(danhSach, x => x.TenBang == "NhanSu");
        Assert.Contains(danhSach, x => x.TenBang == "MonHoc");
        Assert.Contains(danhSach, x => x.TenBang == "PhanCongGiangDay");
        Assert.Contains(danhSach, x => x.TenBang == "ChucVu");
        Assert.Contains(danhSach, x => x.TenBang == "TrinhDoChuyenMon");
        Assert.Contains(danhSach, x => x.TenBang == "TaiKhoan");
        Assert.Contains(danhSach, x => x.TenBang == "NhatKyHeThong");
    }

    [Fact]
    public async Task ThemCapNhatVaXoaTruongTuyChinh_LuuTrucTiepVaoDb()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var dichVu = new QuanTriDuLieuDichVu(db, nhatKy);

        // Act 1: Thêm trường tùy chỉnh mới cho nhóm Khoa
        var truong = await dichVu.ThemTruongTuyChinhAsync(new TruongTuyChinh
        {
            TenBang = "Khoa",
            MaTruong = "SoQuyetDinh",
            TenHienThi = "Số Quyết Định Thành Lập",
            KieuDuLieu = "VanBan",
            GiaTriMacDinh = "QD-C500",
            CoHoatDong = true
        }, "admin");

        // Assert 1
        Assert.True(truong.TruongTuyChinhId > 0);
        var dsTruong = await dichVu.LayDanhSachTruongTuyChinhAsync("Khoa");
        Assert.Single(dsTruong);
        Assert.Equal("SoQuyetDinh", dsTruong[0].MaTruong);

        // Act 2: Cập nhật tên hiển thị
        truong.TenHienThi = "Số Quyết Định Bộ Công An";
        await dichVu.CapNhatTruongTuyChinhAsync(truong, "admin");
        var dsSauSua = await dichVu.LayDanhSachTruongTuyChinhAsync("Khoa");
        Assert.Equal("Số Quyết Định Bộ Công An", dsSauSua[0].TenHienThi);

        // Act 3: Xóa trường
        var kqXoa = await dichVu.XoaTruongTuyChinhAsync(truong.TruongTuyChinhId, "admin");
        Assert.True(kqXoa);
        Assert.Empty(await dichVu.LayDanhSachTruongTuyChinhAsync("Khoa"));
    }

    [Fact]
    public async Task ThemNhanhVaCapNhatNhanhBanGhi_KemTruongTuyChinh_DongBoDbChinhXac()
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var dichVu = new QuanTriDuLieuDichVu(db, nhatKy);

        // Thêm 1 trường tùy chỉnh vào bảng Khoa
        await dichVu.ThemTruongTuyChinhAsync(new TruongTuyChinh
        {
            TenBang = "Khoa",
            MaTruong = "KhoiNghiepVu",
            TenHienThi = "Khối Nghiệp Vụ",
            KieuDuLieu = "VanBan",
            CoHoatDong = true
        });

        // Act 1: Thêm nhanh 1 Khoa kèm giá trị trường tùy chỉnh
        var idKhoaMoi = await dichVu.ThemNhanhBanGhiAsync("Khoa", new Dictionary<string, string?>
        {
            ["MaKhoa"] = "K_ANND",
            ["TenKhoa"] = "Khoa Nghiệp vụ An ninh",
            ["DiaDiem"] = "Tòa nhà C500",
            ["Custom_KhoiNghiepVu"] = "An ninh Điều tra"
        }, "admin");

        Assert.False(string.IsNullOrEmpty(idKhoaMoi));

        // Act 2: Cập nhật trực tiếp trường gốc lẫn trường tùy chỉnh
        await dichVu.CapNhatNhanhBanGhiAsync("Khoa", idKhoaMoi, new Dictionary<string, string?>
        {
            ["MaKhoa"] = "K_ANND",
            ["TenKhoa"] = "Khoa Nghiệp vụ An ninh Nội bộ",
            ["DiaDiem"] = "Khu A - C500",
            ["Custom_KhoiNghiepVu"] = "An ninh Chính trị Nội bộ"
        }, "admin");

        // Assert: Kiểm tra dữ liệu đọc ra từ bảng trực tuyến
        var bang = await dichVu.LayDuLieuBangTrucTuyenAsync("Khoa");
        Assert.Single(bang.DanhSachDong);
        var dong = bang.DanhSachDong[0];
        Assert.Equal("Khoa Nghiệp vụ An ninh Nội bộ", dong["TenKhoa"]);
        Assert.Equal("Khu A - C500", dong["DiaDiem"]);
        Assert.Equal("An ninh Chính trị Nội bộ", dong["Custom_KhoiNghiepVu"]);
    }

    [Theory]
    [InlineData("Khoa")]
    [InlineData("BoMon")]
    [InlineData("LopHoc")]
    [InlineData("HocVien")]
    [InlineData("NhanSu")]
    [InlineData("MonHoc")]
    [InlineData("PhanCongGiangDay")]
    [InlineData("ChucVu")]
    [InlineData("TrinhDoChuyenMon")]
    [InlineData("TaiKhoan")]
    [InlineData("NhatKyHeThong")]
    public async Task LayDuLieuBangTrucTuyen_TatCa11Nhom_TraVeCauTrucHopLe(string tenBang)
    {
        // Arrange
        using var db = CoSoDuLieuTestHelper.TaoDbContextAo();
        var nhatKy = new NhatKyDichVu(db);
        var dichVu = new QuanTriDuLieuDichVu(db, nhatKy);

        // Act
        var ketQua = await dichVu.LayDuLieuBangTrucTuyenAsync(tenBang);

        // Assert
        Assert.Equal(tenBang, ketQua.TenBang);
        Assert.NotEmpty(ketQua.TenKhoaChinh);
        Assert.NotEmpty(ketQua.DanhSachCot);
    }
}
