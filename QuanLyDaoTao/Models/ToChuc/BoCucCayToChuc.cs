using System.Globalization;

namespace QuanLyDaoTao.Models.ToChuc;

/// <summary>Vị trí một thẻ trong sơ đồ, tính theo tọa độ canvas trước khi thu phóng.</summary>
/// <param name="Nut">Bản ghi tổ chức.</param>
/// <param name="X">Tọa độ ngang.</param>
/// <param name="Y">Tọa độ dọc.</param>
public record ViTriNutToChuc(NutToChuc Nut, double X, double Y)
{
    /// <summary>Tọa độ CSS độc lập thiết lập ngôn ngữ máy chủ.</summary>
    public string KieuDang => FormattableString.Invariant($"left:{X}px;top:{Y}px");
}

/// <summary>Đường nối quan hệ cha con trong sơ đồ.</summary>
/// <param name="Cha">Khóa nút cấp trên.</param>
/// <param name="Con">Khóa nút trực thuộc.</param>
/// <param name="DuongVe">Đường cong SVG nối hai cạnh thẻ.</param>
public record DuongNoiToChuc(string Cha, string Con, string DuongVe);

/// <summary>Tự xếp các nhánh từ trái sang phải, dành vùng riêng để thẻ không đè nhau.</summary>
public class BoCucCayToChuc
{
    /// <summary>Chiều rộng mỗi thẻ.</summary>
    public const int ChieuRongNut = 256;
    /// <summary>Chiều cao mỗi thẻ.</summary>
    public const int ChieuCaoNut = 152;
    private const int KhoangCachNgang = 80;
    private const int KhoangCachDoc = 24;
    private const int LeCanvas = 36;
    /// <summary>Thẻ đang hiển thị, không bao gồm con của nhánh đã thu gọn.</summary>
    public List<ViTriNutToChuc> CacNut { get; } = new();
    /// <summary>Các quan hệ được hiển thị.</summary>
    public List<DuongNoiToChuc> CacDuongNoi { get; } = new();
    /// <summary>Chiều rộng vùng chứa toàn sơ đồ.</summary>
    public double ChieuRong { get; private set; }
    /// <summary>Chiều cao vùng chứa toàn sơ đồ.</summary>
    public double ChieuCao { get; private set; }
    /// <summary>Kích thước CSS của vùng vẽ.</summary>
    public string KieuDang => FormattableString.Invariant($"width:{ChieuRong}px;height:{ChieuCao}px");
    /// <summary>Dấu nhận biết thay đổi hình học để giữ khung nhìn khi chỉ đổi lựa chọn.</summary>
    public string DauBoCuc => string.Join(";", CacNut.Select(n => string.Join(":", n.Nut.KhoaNut,
        n.X.ToString(CultureInfo.InvariantCulture), n.Y.ToString(CultureInfo.InvariantCulture))));

    /// <summary>Tính bố cục cho phần cây phù hợp với bộ lọc và trạng thái mở nhánh.</summary>
    public static BoCucCayToChuc Tao(NutToChuc goc, ISet<string> nutHienThi, ISet<string> nutMo)
    {
        ArgumentNullException.ThrowIfNull(goc);
        ArgumentNullException.ThrowIfNull(nutHienThi);
        ArgumentNullException.ThrowIfNull(nutMo);
        var ketQua = new BoCucCayToChuc();
        if (!nutHienThi.Contains(goc.KhoaNut)) { return ketQua; }
        var chieuCaoNhanh = new Dictionary<string, double>();
        var cacCon = new Dictionary<string, List<NutToChuc>>();

        double DoNhanh(NutToChuc nut)
        {
            var con = nutMo.Contains(nut.KhoaNut)
                ? nut.Con.Where(n => nutHienThi.Contains(n.KhoaNut)).ToList() : new List<NutToChuc>();
            cacCon.Add(nut.KhoaNut, con);
            var chieuCao = con.Count == 0 ? ChieuCaoNut
                : Math.Max(ChieuCaoNut, con.Sum(DoNhanh) + (con.Count - 1) * KhoangCachDoc);
            chieuCaoNhanh.Add(nut.KhoaNut, chieuCao);
            return chieuCao;
        }

        void XepNhanh(NutToChuc nut, int cap, double dauNhanh, ViTriNutToChuc? cha)
        {
            var x = LeCanvas + cap * (ChieuRongNut + KhoangCachNgang);
            var y = dauNhanh + (chieuCaoNhanh[nut.KhoaNut] - ChieuCaoNut) / 2;
            var viTri = new ViTriNutToChuc(nut, x, y);
            ketQua.CacNut.Add(viTri);
            if (cha != null)
            {
                var xCha = cha.X + ChieuRongNut;
                var yCha = cha.Y + ChieuCaoNut / 2;
                var yCon = y + ChieuCaoNut / 2;
                var xGiua = (xCha + x) / 2;
                var duongVe = FormattableString.Invariant($"M {xCha} {yCha} C {xGiua} {yCha}, {xGiua} {yCon}, {x} {yCon}");
                ketQua.CacDuongNoi.Add(new(cha.Nut.KhoaNut, nut.KhoaNut, duongVe));
            }
            foreach (var con in cacCon[nut.KhoaNut])
            {
                XepNhanh(con, cap + 1, dauNhanh, viTri);
                dauNhanh += chieuCaoNhanh[con.KhoaNut] + KhoangCachDoc;
            }
        }

        ketQua.ChieuCao = DoNhanh(goc) + LeCanvas * 2;
        XepNhanh(goc, 0, LeCanvas, null);
        ketQua.ChieuRong = ketQua.CacNut.Max(n => n.X) + ChieuRongNut + LeCanvas;
        return ketQua;
    }
}
