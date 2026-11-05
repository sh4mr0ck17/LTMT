using QuanLyDaoTao.Models.ToChuc;
using Xunit;

namespace QuanLyDaoTao.Tests.Services;

/// <summary>Kiểm tra hình học và quan hệ trên canvas tổ chức.</summary>
public class BoCucCayToChucTests
{
    /// <summary>Nhánh đóng không vẽ các hậu duệ dù chúng phù hợp bộ lọc.</summary>
    [Fact]
    public void Tao_NhanhDong_ChiVeCapDangMo()
    {
        var goc = TaoCay();
        var tatCa = LayTatCa(goc).Select(n => n.KhoaNut).ToHashSet();
        var boCuc = BoCucCayToChuc.Tao(goc, tatCa, new HashSet<string> { "goc" });
        Assert.Equal(3, boCuc.CacNut.Count);
        Assert.Equal(2, boCuc.CacDuongNoi.Count);
        Assert.DoesNotContain(boCuc.CacNut, n => n.Nut.KhoaNut == "bm1");
    }

    /// <summary>Các thẻ trong cây rộng và sâu không giao nhau, tất cả nằm trong vùng vẽ.</summary>
    [Fact]
    public void Tao_MoTatCa_KhongChongTheVaKhongMatQuanHe()
    {
        var goc = TaoCay();
        var tatCa = LayTatCa(goc).Select(n => n.KhoaNut).ToHashSet();
        var boCuc = BoCucCayToChuc.Tao(goc, tatCa, tatCa);
        // Nhân sự không vẽ thành thẻ (giai đoạn 2), các nút còn lại đều có mặt.
        var soVeDuoc = LayTatCa(goc).Count(n => !BoCucCayToChuc.LaLoaiKhongVe(n.Loai));
        Assert.Equal(soVeDuoc, boCuc.CacNut.Count);
        Assert.Equal(soVeDuoc - 1, boCuc.CacDuongNoi.Count);
        foreach (var nut in boCuc.CacNut)
        {
            Assert.InRange(nut.X, 0, boCuc.ChieuRong - BoCucCayToChuc.ChieuRongNut);
            Assert.InRange(nut.Y, 0, boCuc.ChieuCao - BoCucCayToChuc.ChieuCaoNut);
            foreach (var khac in boCuc.CacNut.Where(n => n != nut))
            {
                var coGiao = nut.X < khac.X + BoCucCayToChuc.ChieuRongNut && nut.X + BoCucCayToChuc.ChieuRongNut > khac.X
                    && nut.Y < khac.Y + BoCucCayToChuc.ChieuCaoNut && nut.Y + BoCucCayToChuc.ChieuCaoNut > khac.Y;
                Assert.False(coGiao, $"{nut.Nut.KhoaNut} giao với {khac.Nut.KhoaNut}");
            }
        }
        foreach (var duong in boCuc.CacDuongNoi)
        {
            var cha = boCuc.CacNut.Single(n => n.Nut.KhoaNut == duong.Cha);
            var con = boCuc.CacNut.Single(n => n.Nut.KhoaNut == duong.Con);
            Assert.True(cha.X + BoCucCayToChuc.ChieuRongNut < con.X);
            Assert.Contains(cha.Nut.Con, n => n.KhoaNut == con.Nut.KhoaNut);
        }
    }

    /// <summary>Sau lọc chỉ vẽ nhánh có kết quả, vẫn nối đủ đường cấp trên.</summary>
    [Fact]
    public void Tao_LocNhanSu_ChiVeKetQuaVaCapTren()
    {
        var goc = TaoCay();
        var ketQua = BoLocCayToChuc.Loc(goc, "Nguyễn");
        var boCuc = BoCucCayToChuc.Tao(goc, ketQua.HienThi, ketQua.HienThi);
        // Nhân sự khớp không thành thẻ; canvas dừng ở bộ môn chứa họ, nhân sự xem ở bảng panel chi tiết.
        Assert.Contains("ns1", ketQua.Khop);
        Assert.True(boCuc.CacNut.Select(n => n.Nut.KhoaNut).ToHashSet().SetEquals(new[] { "goc", "k1", "bm1" }));
        Assert.Equal(2, boCuc.CacDuongNoi.Count);
    }

    /// <summary>Không có kết quả tạo vùng vẽ trống, không lỗi tính toán kích thước.</summary>
    [Fact]
    public void Tao_KhongCoKetQua_TraBoCucRong()
    {
        var boCuc = BoCucCayToChuc.Tao(TaoCay(), new HashSet<string>(), new HashSet<string>());
        Assert.Empty(boCuc.CacNut);
        Assert.Empty(boCuc.CacDuongNoi);
        Assert.Equal(0, boCuc.ChieuRong);
        Assert.Equal(0, boCuc.ChieuCao);
    }

    /// <summary>Cùng một trạng thái cho cùng vị trí; thu gọn giảm diện tích sơ đồ.</summary>
    [Fact]
    public void Tao_LapLaiVaThuGon_BoCucOnDinh()
    {
        var goc = TaoCay();
        var tatCa = LayTatCa(goc).Select(n => n.KhoaNut).ToHashSet();
        var dayDu = BoCucCayToChuc.Tao(goc, tatCa, tatCa);
        var lapLai = BoCucCayToChuc.Tao(goc, tatCa, tatCa);
        var thuGon = BoCucCayToChuc.Tao(goc, tatCa, new HashSet<string> { "goc" });
        Assert.Equal(dayDu.DauBoCuc, lapLai.DauBoCuc);
        Assert.True(thuGon.ChieuRong < dayDu.ChieuRong);
        Assert.True(thuGon.ChieuCao < dayDu.ChieuCao);
    }

    /// <summary>Nhánh có 20 con: chỉ vẽ 8 thẻ đầu và một thẻ "+12 mục nữa".</summary>
    [Fact]
    public void Tao_NhanhNhieuCon_GioiHanVaThemTheConLai()
    {
        var goc = TaoCayNhieuLop(20);
        var tatCa = LayTatCa(goc).Select(n => n.KhoaNut).ToHashSet();

        var boCuc = BoCucCayToChuc.Tao(goc, tatCa, tatCa);

        var conCuaKhoa = boCuc.CacDuongNoi.Where(d => d.Cha == "k1").Select(d => d.Con).ToList();
        Assert.Equal(BoCucCayToChuc.SoConToiDaMoiNhanh + 1, conCuaKhoa.Count);
        var nutThem = Assert.Single(boCuc.CacNut, n => n.Nut.Loai == BoCucCayToChuc.LoaiNutThemMuc);
        Assert.Equal("+ 12 mục nữa", nutThem.Nut.Ten);
        Assert.Equal("k1", BoCucCayToChuc.LayKhoaChaCuaNutThemMuc(nutThem.Nut.KhoaNut));
    }

    /// <summary>Tăng số con hiển thị của nhánh làm thẻ "+N" giảm tương ứng.</summary>
    [Fact]
    public void Tao_TangSoConHienThi_GiamThePhanConLai()
    {
        var goc = TaoCayNhieuLop(20);
        var tatCa = LayTatCa(goc).Select(n => n.KhoaNut).ToHashSet();

        var boCuc = BoCucCayToChuc.Tao(goc, tatCa, tatCa, new Dictionary<string, int> { ["k1"] = 16 });

        Assert.Equal("+ 4 mục nữa", Assert.Single(boCuc.CacNut, n => n.Nut.Loai == BoCucCayToChuc.LoaiNutThemMuc).Nut.Ten);
    }

    /// <summary>Nút bắt buộc hiện (đang chọn / khớp tìm kiếm) vẫn được vẽ dù nằm ngoài ngưỡng.</summary>
    [Fact]
    public void Tao_NutLuonHienNgoaiNguong_VanDuocVe()
    {
        var goc = TaoCayNhieuLop(20);
        var tatCa = LayTatCa(goc).Select(n => n.KhoaNut).ToHashSet();

        var boCuc = BoCucCayToChuc.Tao(goc, tatCa, tatCa, nutLuonHien: new HashSet<string> { "lop19" });

        Assert.Contains(boCuc.CacNut, n => n.Nut.KhoaNut == "lop19");
        Assert.Equal("+ 11 mục nữa", Assert.Single(boCuc.CacNut, n => n.Nut.Loai == BoCucCayToChuc.LoaiNutThemMuc).Nut.Ten);
    }

    /// <summary>Số thẻ không phụ thuộc số nhân sự: thêm 500 nhân sự không làm tăng số thẻ.</summary>
    [Fact]
    public void Tao_NhieuNhanSu_SoTheKhongDoi()
    {
        var khongNhanSu = TaoCay();
        var coNhanSu = TaoCay();
        var boMon = coNhanSu.Con[0].Con[0];
        boMon.Con.AddRange(Enumerable.Range(1, 500).Select(i => new NutToChuc { KhoaNut = $"them{i}", Loai = "Nhân sự", Ten = $"Cán bộ {i}" }));

        var tatCa1 = LayTatCa(khongNhanSu).Select(n => n.KhoaNut).ToHashSet();
        var tatCa2 = LayTatCa(coNhanSu).Select(n => n.KhoaNut).ToHashSet();

        Assert.Equal(BoCucCayToChuc.Tao(khongNhanSu, tatCa1, tatCa1).CacNut.Count, BoCucCayToChuc.Tao(coNhanSu, tatCa2, tatCa2).CacNut.Count);
        Assert.Equal(502, coNhanSu.SoNhanSu);
    }

    private static NutToChuc TaoCayNhieuLop(int soLop) => new()
    {
        KhoaNut = "goc", Loai = "Học viện", Ten = "Học viện", Con = new()
        {
            new()
            {
                KhoaNut = "k1", Loai = "Khoa", Ten = "Khoa một",
                Con = Enumerable.Range(0, soLop).Select(i => new NutToChuc { KhoaNut = $"lop{i}", Loai = "Lớp học", Ten = $"Lớp {i}" }).ToList()
            }
        }
    };

    private static IEnumerable<NutToChuc> LayTatCa(NutToChuc nut)
    {
        yield return nut;
        foreach (var con in nut.Con)
        {
            foreach (var chau in LayTatCa(con)) { yield return chau; }
        }
    }

    private static NutToChuc TaoCay() => new()
    {
        KhoaNut = "goc", Loai = "Học viện", Ten = "Học viện", Con = new()
        {
            new()
            {
                KhoaNut = "k1", Loai = "Khoa", Ten = "Khoa một", Con = new()
                {
                    new()
                    {
                        KhoaNut = "bm1", Loai = "Bộ môn", Ten = "Bộ môn một", Con = new()
                        {
                            new() { KhoaNut = "ns1", Loai = "Nhân sự", Ten = "Nguyễn Văn An" },
                            new() { KhoaNut = "ns2", Loai = "Nhân sự", Ten = "Trần Văn Bình" }
                        }
                    },
                    new() { KhoaNut = "lop1", Loai = "Lớp học", Ten = "Lớp một" }
                }
            },
            new()
            {
                KhoaNut = "k2", Loai = "Khoa", Ten = "Khoa hai", Con = new()
                {
                    new() { KhoaNut = "bm2", Loai = "Bộ môn", Ten = "Bộ môn hai" }
                }
            }
        }
    };
}
