using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.ToChuc;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>Kiểm tra quan hệ và số liệu cây bằng SQLite, cùng bộ lọc giữ ngữ cảnh.</summary>
public class CayToChucDichVuTests
{
    /// <summary>Không bỏ sót nhân sự chỉ có bộ môn và không đếm trùng hồ sơ có cả khoa lẫn bộ môn.</summary>
    [Fact]
    public async Task LayCay_PhanNhanSuTheoBoMon_KhongTrungKhongMatHoSo()
    {
        using var db = TaoCoSoDuLieu();
        var khoa = new Khoa { MaKhoa = "K01", TenKhoa = "Khoa Đào tạo" };
        var khoaKhac = new Khoa { MaKhoa = "K02", TenKhoa = "Khoa khác" };
        var chucVu = new ChucVu { MaChucVu = "GV", TenChucVu = "Giảng viên" };
        var boMon = new BoMon { MaBoMon = "BM01", TenBoMon = "Bộ môn một", Khoa = khoa };
        db.AddRange(khoa, khoaKhac, boMon, chucVu);
        await db.SaveChangesAsync();
        var trucTiep = TaoNhanSu("NS01", khoa.KhoaId);
        var coCaHai = TaoNhanSu("NS02", khoa.KhoaId, boMon.BoMonId);
        var chiCoBoMon = TaoNhanSu("NS03", null, boMon.BoMonId);
        chiCoBoMon.ChucVuId = chucVu.ChucVuId;
        var lechKhoa = TaoNhanSu("NS04", khoaKhac.KhoaId, boMon.BoMonId);
        var chuaPhan = TaoNhanSu("NS05");
        var daNghi = TaoNhanSu("NS06", khoa.KhoaId);
        daNghi.TrangThai = "Đã nghỉ việc";
        db.AddRange(trucTiep, coCaHai, chiCoBoMon, lechKhoa, chuaPhan, daNghi);
        await db.SaveChangesAsync();

        var goc = await new CayToChucDichVu(db).LayCayAsync();
        var cacNut = LayTatCa(goc).ToList();
        var nutBoMon = cacNut.Single(n => n.Loai == "Bộ môn");
        var nutKhoa = cacNut.Single(n => n.Ma == "K01");

        Assert.Equal(6, goc.SoNhanSu);
        Assert.Equal(6, cacNut.Count(n => n.Loai == "Nhân sự"));
        Assert.Equal(3, nutBoMon.SoNhanSu);
        Assert.Equal(5, nutKhoa.SoNhanSu);
        Assert.Contains(nutBoMon.Con, n => n.Ma == "NS03" && n.MoTa == "Giảng viên");
        Assert.DoesNotContain(nutBoMon.Con, n => n.Ma == "NS01");
        Assert.Contains(cacNut, n => n.Ma == "NS06" && n.CoHoatDong == false);
        Assert.NotNull(cacNut.Single(n => n.Ma == "NS04").CanhBao);
        Assert.NotNull(cacNut.Single(n => n.Ma == "NS05").CanhBao);
        Assert.Equal(2, goc.SoCanRaSoat);
        Assert.Equal(khoaKhac.KhoaId, (await db.NhanSus.AsNoTracking().SingleAsync(n => n.MaNhanSu == "NS04")).KhoaId);
    }

    /// <summary>Sĩ số chỉ tính đang học, không cộng hồ sơ đã tốt nghiệp vào sức chứa.</summary>
    [Fact]
    public async Task LayCay_SiSoDangHoc_CanhBaoVuotSucChuaVaGiuLopNgung()
    {
        using var db = TaoCoSoDuLieu();
        var khoa = new Khoa { MaKhoa = "K01", TenKhoa = "Khoa một" };
        var lop = new LopHoc { MaLop = "L01", TenLop = "Lớp một", NienKhoa = "2026-2030", Khoa = khoa, SoLuongToiDa = 1 };
        var lopNgung = new LopHoc { MaLop = "L02", TenLop = "Lớp hai", NienKhoa = "2020-2024", Khoa = khoa, CoHoatDong = false };
        db.AddRange(khoa, lop, lopNgung);
        await db.SaveChangesAsync();
        db.HocViens.AddRange(
            new HocVien { MaHocVien = "HV01", HoVaTen = "Học viên một", LopHocId = lop.LopHocId },
            new HocVien { MaHocVien = "HV02", HoVaTen = "Học viên hai", LopHocId = lop.LopHocId },
            new HocVien { MaHocVien = "HV03", HoVaTen = "Học viên ba", LopHocId = lop.LopHocId, TrangThai = "Đã tốt nghiệp" });
        await db.SaveChangesAsync();

        var goc = await new CayToChucDichVu(db).LayCayAsync();
        var nutLop = LayTatCa(goc).Single(n => n.Ma == "L01");

        Assert.Equal(2, goc.SoLop);
        Assert.Equal(2, goc.SoHocVien);
        Assert.Equal(2, nutLop.SiSo);
        Assert.Equal(1, nutLop.SucChua);
        Assert.NotNull(nutLop.CanhBao);
        Assert.Equal(1, goc.SoCanRaSoat);
        Assert.Contains(LayTatCa(goc), n => n.Ma == "L02" && n.CoHoatDong == false);
    }

    /// <summary>Cây trống vẫn có gốc và số liệu bằng không.</summary>
    [Fact]
    public async Task LayCay_ChuaCoDuLieu_TraGocRong()
    {
        using var db = TaoCoSoDuLieu();
        var goc = await new CayToChucDichVu(db).LayCayAsync();
        Assert.Empty(goc.Con);
        Assert.Equal(0, goc.SoNhanSu);
        Assert.Equal(0, goc.SoHocVien);
    }

    /// <summary>Tìm kiếm tiếng Việt không dấu và không phân biệt hoa thường.</summary>
    [Theory]
    [InlineData("dao tao", true)]
    [InlineData("ĐÀO TẠO", true)]
    [InlineData("k01", true)]
    [InlineData("  dien  ", true)]
    [InlineData("khong ton tai", false)]
    public void TimKiem_TiengVietCoDauVaKhongDau(string tuKhoa, bool ketQua)
    {
        var nut = new NutToChuc { Ten = "Khoa Đào tạo", Ma = "K01", MoTa = "Cơ điện" };
        Assert.Equal(ketQua, nut.KhopTuKhoa(tuKhoa));
    }

    /// <summary>Lọc kết hợp vẫn giữ gốc, khoa và bộ môn chứa hồ sơ khớp.</summary>
    [Fact]
    public void Loc_KetHopTenLoaiTrangThai_GiuDuongDanCha()
    {
        var goc = TaoCayMau();
        var ketQua = BoLocCayToChuc.Loc(goc, "nguyen", "Nhân sự", "hoat-dong");
        Assert.Equal(new[] { "ns1" }, ketQua.Khop.OrderBy(k => k));
        Assert.True(ketQua.HienThi.SetEquals(new[] { "goc", "khoa", "nhom", "bm", "ns1" }));
        Assert.Equal(2, goc.SoNhanSu);
    }

    /// <summary>Tìm khoa rồi lọc nhân sự phải đi xuyên qua nhóm điều hướng.</summary>
    [Fact]
    public void Loc_TenKhoaVaLoaiNhanSu_HienThiHoSoTrongKhoa()
    {
        var ketQua = BoLocCayToChuc.Loc(TaoCayMau(), "dao tao", "Nhân sự");
        Assert.True(ketQua.Khop.SetEquals(new[] { "ns1", "ns2" }));
        Assert.Contains("nhom", ketQua.HienThi);
        Assert.DoesNotContain("nhom", ketQua.Khop);
    }

    /// <summary>Cảnh báo và ngừng hoạt động là hai điều kiện riêng biệt.</summary>
    [Fact]
    public void Loc_CanRaSoatVaNgungHoatDong_TraDungHoSo()
    {
        var goc = TaoCayMau();
        Assert.True(BoLocCayToChuc.Loc(goc, trangThai: "ra-soat").Khop.SetEquals(new[] { "ns1" }));
        Assert.True(BoLocCayToChuc.Loc(goc, trangThai: "ngung").Khop.SetEquals(new[] { "ns2" }));
    }

    /// <summary>Không khớp phải có kết quả trống; xóa lọc khôi phục cả nhóm trống.</summary>
    [Fact]
    public void Loc_KhongKhopVaXoaLoc_KhoiPhucCayDayDu()
    {
        var goc = TaoCayMau();
        var khongKhop = BoLocCayToChuc.Loc(goc, "khong ton tai");
        Assert.Empty(khongKhop.HienThi);
        Assert.Empty(khongKhop.Khop);
        var tatCa = BoLocCayToChuc.Loc(goc);
        Assert.Equal(LayTatCa(goc).Count(), tatCa.HienThi.Count);
        Assert.Contains("rong", tatCa.HienThi);
    }

    private static QuanLyDaoTaoDbContext TaoCoSoDuLieu()
    {
        var options = new DbContextOptionsBuilder<QuanLyDaoTaoDbContext>().UseSqlite("Data Source=:memory:").Options;
        var db = new QuanLyDaoTaoDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();
        return db;
    }

    private static NhanSu TaoNhanSu(string ma, int? khoaId = null, int? boMonId = null) => new()
    {
        MaNhanSu = ma, HoVaTen = ma, Email = $"{ma}@test.local", KhoaId = khoaId, BoMonId = boMonId
    };

    private static IEnumerable<NutToChuc> LayTatCa(NutToChuc nut)
    {
        yield return nut;
        foreach (var con in nut.Con)
        {
            foreach (var chau in LayTatCa(con)) { yield return chau; }
        }
    }

    private static NutToChuc TaoCayMau() => new()
    {
        KhoaNut = "goc", Loai = "Học viện", Ten = "C500", Con = new()
        {
            new()
            {
                KhoaNut = "khoa", Loai = "Khoa", Ten = "Khoa Đào tạo", CoHoatDong = true, Con = new()
                {
                    new()
                    {
                        KhoaNut = "nhom", Loai = "Nhóm", Ten = "Bộ môn", Con = new()
                        {
                            new()
                            {
                                KhoaNut = "bm", Loai = "Bộ môn", Ten = "Bộ môn một", CoHoatDong = true, Con = new()
                                {
                                    new() { KhoaNut = "ns1", Loai = "Nhân sự", Ten = "Nguyễn Văn An", CoHoatDong = true, CanhBao = "Khoa không khớp" },
                                    new() { KhoaNut = "ns2", Loai = "Nhân sự", Ten = "Nguyễn Văn Bình", CoHoatDong = false }
                                }
                            }
                        }
                    },
                    new() { KhoaNut = "rong", Loai = "Nhóm", Ten = "Lớp học" }
                }
            }
        }
    };
}
