using System.Data.Common;
using Microsoft.AspNetCore.Components;
using QuanLyDaoTao.Models.ToChuc;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Components.Pages;

/// <summary>
/// Trang cây tổ chức: canvas chỉ thể hiện cấu trúc ở mức đọc được (giới hạn số thẻ, nhân sự không vẽ thành thẻ,
/// chế độ tập trung), dữ liệu chi tiết nằm ở panel bên phải.
/// </summary>
public partial class CayToChucPage
{
    /// <summary>Số cấp được mở bởi nút "Mở cấp 2" (Học viện → Khoa → Nhóm).</summary>
    private const int SoCapMoNhanh = 2;

    /// <summary>Số dòng mỗi trang của bảng bản ghi trực thuộc.</summary>
    private const int SoDongMoiTrangBang = 10;

    /// <summary>Số thẻ tối đa khi tự mở các cấp lúc mới vào trang (đọc rõ ở 100%).</summary>
    private const int SoTheMacDinh = 15;

    /// <summary>Mã nhánh sơ đồ tổ chức.</summary>
    private const string NhanhToChuc = "to-chuc";

    /// <summary>Mã nhánh sơ đồ đào tạo.</summary>
    private const string NhanhDaoTao = "dao-tao";

    private string _nhanh = NhanhToChuc;

    /// <summary>Loại bản ghi lọc được theo từng sơ đồ.</summary>
    private IReadOnlyList<string> CacLoaiLoc => _nhanh == NhanhToChuc
        ? ["Khoa", "Bộ môn", "Nhân sự"]
        : ["Hệ đào tạo", "Khóa", "Lớp học", "Học viên"];

    /// <summary>Chọn sơ đồ qua đường dẫn (?so-do=dao-tao).</summary>
    [SupplyParameterFromQuery(Name = "so-do")]
    public string? SoDoTuDuongDan { get; set; }

    /// <summary>Chọn sơ đồ qua alias vung (?vung=to-chuc hoặc ?vung=dao-tao).</summary>
    [SupplyParameterFromQuery(Name = "vung")]
    public string? VungTuDuongDan { get; set; }

    /// <summary>Truy xuất nhanh lọc trực tiếp theo KhoaId (?khoaId=1).</summary>
    [SupplyParameterFromQuery(Name = "khoaId")]
    public int? KhoaIdTuDuongDan { get; set; }

    /// <summary>Truy xuất nhanh lọc trực tiếp theo BoMonId (?boMonId=1).</summary>
    [SupplyParameterFromQuery(Name = "boMonId")]
    public int? BoMonIdTuDuongDan { get; set; }

    /// <summary>Truy xuất nhanh lọc trực tiếp theo LopId (?lopId=1).</summary>
    [SupplyParameterFromQuery(Name = "lopId")]
    public int? LopIdTuDuongDan { get; set; }

    /// <summary>Truy xuất nhanh lọc trực tiếp theo Hệ đào tạo (?he=ChinhQuy).</summary>
    [SupplyParameterFromQuery(Name = "he")]
    public string? HeTuDuongDan { get; set; }
    [Inject] private ICayToChucDichVu CayToChucDichVu { get; set; } = default!;
    [Inject] private ILogger<CayToChucPage> NhatKy { get; set; } = default!;
    private NutToChuc? _goc;
    private NutToChuc? _nutChon;
    private readonly Dictionary<string, NutToChuc> _cacNut = new();
    private readonly Dictionary<string, NutToChuc> _chaCuaNut = new();
    private readonly HashSet<string> _nutHienThi = new();
    private readonly HashSet<string> _nutKhop = new();
    private readonly HashSet<string> _nutMo = new();
    private readonly Dictionary<string, int> _soConHienThi = new();
    private HashSet<string> _duongDanChon = new();
    private HashSet<string> _nutLuonHien = new();
    private string _tuKhoa = string.Empty;
    private string _loai = string.Empty;
    private string _trangThai = string.Empty;
    private string _tuKhoaTrongNhanh = string.Empty;
    private string? _loiTaiDuLieu;
    private string? _thongBaoGioiHan;
    private bool _coDangTai;
    private bool _coHienChiTiet;
    private bool _coTapTrung;
    private bool _anThuMuc;

    /// <summary>Chế độ mặc định là dạng cột: dễ theo dõi hơn canvas khi cây sâu nhiều cấp.</summary>
    private bool _cheDoCot = true;

    /// <summary>Đường dẫn từ gốc tới mục đang chọn, cho chế độ cột.</summary>
    private IReadOnlyList<NutToChuc> DuongDanCot => _goc is null ? [] : _nutChon is null ? [_goc] : LayDuongDan(_nutChon);

    private void DoiCheDo(bool cheDoCot)
    {
        _cheDoCot = cheDoCot;
        _coHienChiTiet = _nutChon is not null;
        _lanCanKhung++;
    }

    /// <summary>Gốc đang xem trên canvas khi người dùng "xem riêng nhánh"; null = toàn bộ sơ đồ.</summary>
    private NutToChuc? _gocXem;

    private NutToChuc GocCanvas => _gocXem ?? _goc!;

    /// <summary>Chú giải màu cho các loại thẻ của sơ đồ đang mở.</summary>
    private IReadOnlyList<(string Loai, string Nhan)> ChuGiaiMau => _nhanh == NhanhToChuc
        ? [("Học viện", "Học viện"), ("Khoa", "Khoa"), ("Bộ môn", "Bộ môn"), ("Nhân sự", "Cán bộ")]
        : [("Học viện", "Học viện"), ("Hệ đào tạo", "Hệ"), ("Khóa", "Niên khóa"), ("Lớp học", "Lớp"), ("Học viên", "Học viên")];

    private bool CoTheXemNhanh(NutToChuc nut) =>
        !BoCucCayToChuc.LaLoaiKhongVe(nut.Loai) && nut.KhoaNut != _gocXem?.KhoaNut && BoCucCayToChuc.LayConVeDuoc(nut, _nutHienThi).Any();

    /// <summary>Chỉ vẽ nhánh của một thẻ (bấm đúp hoặc nút "Xem riêng nhánh"): sơ đồ nhỏ lại, chữ to và dễ theo dõi.</summary>
    private void XemNhanh(NutToChuc nut)
    {
        if (_goc is null) { return; }
        if (BoCucCayToChuc.LaLoaiKhongVe(nut.Loai) && _chaCuaNut.TryGetValue(nut.KhoaNut, out var cha)) { nut = cha; }
        if (nut.KhoaNut != _goc.KhoaNut && !BoCucCayToChuc.LayConVeDuoc(nut, _nutHienThi).Any())
        {
            // Thẻ không có nhánh con để vẽ (bộ môn, lớp): chỉ chọn và xem danh sách ở panel.
            ChonNut(nut);
            return;
        }

        _gocXem = nut.KhoaNut == _goc.KhoaNut ? null : nut;
        _coTapTrung = false;
        _nutMo.Add(nut.KhoaNut);
        ChonNut(nut);
        _lanCanKhung++;
    }

    private void LenMotCap()
    {
        if (_gocXem is null) { return; }
        XemNhanh(_chaCuaNut.TryGetValue(_gocXem.KhoaNut, out var cha) ? cha : _goc!);
    }

    /// <summary>Đếm số nút thuộc một loại trong nhánh (không tính chính nút).</summary>
    private static int DemLoai(NutToChuc nut, string loai) => nut.Con.Sum(c => (c.Loai == loai ? 1 : 0) + DemLoai(c, loai));
    private int _lanCanKhung;
    private DateTimeOffset? _lanCapNhat;
    private bool CoBoLoc => !string.IsNullOrWhiteSpace(_tuKhoa) || _loai != "" || _trangThai != "";

    /// <summary>Nút được đánh dấu trên canvas: nhân sự không có thẻ nên dùng nút cha của họ.</summary>
    private string? KhoaNutTrenCanvas => _nutChon is null ? null
        : BoCucCayToChuc.LaLoaiKhongVe(_nutChon.Loai) && _chaCuaNut.TryGetValue(_nutChon.KhoaNut, out var cha) ? cha.KhoaNut
        : _nutChon.KhoaNut;

    /// <summary>Tập nút hiển thị trên canvas: toàn bộ kết quả lọc, hoặc chỉ nhánh liên quan khi bật chế độ tập trung.</summary>
    private HashSet<string> NutHienThiCanvas
    {
        get
        {
            if (!_coTapTrung || _goc is null || KhoaNutTrenCanvas is null || !_cacNut.TryGetValue(KhoaNutTrenCanvas, out var tam))
            {
                return _nutHienThi;
            }

            var tapTrung = LayDuongDan(tam).Select(n => n.KhoaNut).ToHashSet();
            tapTrung.UnionWith(tam.Con.Select(n => n.KhoaNut));
            if (_chaCuaNut.TryGetValue(tam.KhoaNut, out var cha))
            {
                tapTrung.UnionWith(cha.Con.Select(n => n.KhoaNut));
            }

            tapTrung.IntersectWith(_nutHienThi);
            return tapTrung;
        }
    }

    /// <summary>Nhánh đang mở trên canvas; ở chế độ tập trung chỉ mở đường dẫn tới nút chọn để anh em ở dạng thu gọn.</summary>
    private HashSet<string> NutMoCanvas
    {
        get
        {
            if (!_coTapTrung || KhoaNutTrenCanvas is null || !_cacNut.TryGetValue(KhoaNutTrenCanvas, out var tam))
            {
                return _nutMo;
            }

            return LayDuongDan(tam).Select(n => n.KhoaNut).ToHashSet();
        }
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrWhiteSpace(VungTuDuongDan))
        {
            _nhanh = VungTuDuongDan.Equals(NhanhDaoTao, StringComparison.OrdinalIgnoreCase) || VungTuDuongDan.Equals("DaoTao", StringComparison.OrdinalIgnoreCase)
                ? NhanhDaoTao
                : NhanhToChuc;
        }
        else
        {
            _nhanh = SoDoTuDuongDan == NhanhDaoTao ? NhanhDaoTao : NhanhToChuc;
        }
        await TaiDuLieuAsync();
    }

    /// <summary>Chuyển giữa hai sơ đồ độc lập; đặt lại trạng thái mở, lọc và lựa chọn.</summary>
    private async Task DoiNhanhAsync(string nhanh)
    {
        if (_nhanh == nhanh || _coDangTai) { return; }
        _nhanh = nhanh;
        _goc = null;
        _gocXem = null;
        _nutChon = null;
        _nutMo.Clear();
        _soConHienThi.Clear();
        _tuKhoa = _loai = _trangThai = _tuKhoaTrongNhanh = string.Empty;
        _coTapTrung = false;
        _coHienChiTiet = false;
        await TaiDuLieuAsync();
    }

    /// <summary>Mở dần từng cấp theo chiều rộng cho tới khi chạm ngưỡng thẻ đọc rõ ở 100%.</summary>
    private void MoMacDinh(NutToChuc goc)
    {
        _nutMo.Add(goc.KhoaNut);
        var capHienTai = new List<NutToChuc> { goc };
        while (capHienTai.Count > 0)
        {
            var capKe = capHienTai.SelectMany(n => BoCucCayToChuc.LayConVeDuoc(n, _nutHienThi)).ToList();
            var thu = new HashSet<string>(_nutMo);
            thu.UnionWith(capKe.Where(n => BoCucCayToChuc.LayConVeDuoc(n, _nutHienThi).Any()).Select(n => n.KhoaNut));
            if (capKe.Count == 0 || BoCucCayToChuc.Tao(goc, _nutHienThi, thu, _soConHienThi).CacNut.Count > SoTheMacDinh) { break; }
            _nutMo.UnionWith(thu);
            capHienTai = capKe;
        }
    }

    private async Task TaiDuLieuAsync()
    {
        if (_coDangTai) { return; }
        _coDangTai = true;
        _loiTaiDuLieu = null;
        try
        {
            var khoaNutCu = _nutChon?.KhoaNut;
            var lanDau = _goc == null;
            var gocMoi = _nhanh == NhanhDaoTao
                ? await CayToChucDichVu.LayCayDaoTaoAsync(he: HeTuDuongDan, lopId: LopIdTuDuongDan)
                : await CayToChucDichVu.LayCayAsync(khoaId: KhoaIdTuDuongDan, boMonId: BoMonIdTuDuongDan);
            _goc = gocMoi;
            _cacNut.Clear();
            _chaCuaNut.Clear();
            LapChiMuc(gocMoi, null);
            _nutMo.RemoveWhere(k => !_cacNut.ContainsKey(k));
            _nutChon = khoaNutCu != null ? _cacNut.GetValueOrDefault(khoaNutCu) : gocMoi;
            _gocXem = _gocXem is null ? null : _cacNut.GetValueOrDefault(_gocXem.KhoaNut);
            _lanCapNhat = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
            ApDungBoLoc();
            if (lanDau)
            {
                _coHienChiTiet = _cheDoCot;
                MoMacDinh(gocMoi);
                _lanCanKhung++;
            }
        }
        catch (Exception loi)
        {
            NhatKy.LogError(loi, "Không thể tải cây tổ chức.");
            _loiTaiDuLieu = "Không thể tải cơ cấu tổ chức lúc này: " + loi.Message;
        }
        finally { _coDangTai = false; }
    }

    private void LapChiMuc(NutToChuc nut, NutToChuc? cha)
    {
        _cacNut[nut.KhoaNut] = nut;
        if (cha != null) { _chaCuaNut[nut.KhoaNut] = cha; }
        foreach (var con in nut.Con) { LapChiMuc(con, nut); }
    }

    private void ApDungBoLoc()
    {
        _nutHienThi.Clear();
        _nutKhop.Clear();
        _thongBaoGioiHan = null;
        if (_goc == null) { return; }
        var ketQua = BoLocCayToChuc.Loc(_goc, _tuKhoa, _loai, _trangThai);
        _nutHienThi.UnionWith(ketQua.HienThi);
        _nutKhop.UnionWith(ketQua.Khop);
        if (CoBoLoc)
        {
            // Chỉ mở các nhánh dẫn tới bản ghi khớp, không mở toàn bộ cây.
            foreach (var khop in _nutKhop.Select(k => _cacNut[k]))
            {
                foreach (var cha in LayDuongDan(khop).SkipLast(1)) { _nutMo.Add(cha.KhoaNut); }
            }
        }
        if (_nutChon == null || !_nutHienThi.Contains(_nutChon.KhoaNut) || (CoBoLoc && !_nutKhop.Contains(_nutChon.KhoaNut)))
        {
            _nutChon = _cacNut.Values.FirstOrDefault(n => _nutKhop.Contains(n.KhoaNut))
                ?? (_nutHienThi.Contains(_goc.KhoaNut) ? _goc : null);
        }

        if (CoBoLoc && _nutChon is not null && BoCucCayToChuc.LaLoaiKhongVe(_nutChon.Loai))
        {
            _coHienChiTiet = true;
        }

        CapNhatDuongDanChon();
        _lanCanKhung++;
    }

    private void CapNhatDuongDanChon()
    {
        _duongDanChon = _nutChon == null ? new() : LayDuongDan(_nutChon).Select(n => n.KhoaNut).ToHashSet();
        // Nút luôn hiện: đường dẫn tới nút chọn và đường dẫn tới các nút khớp bộ lọc (để tìm thấy là thấy trên sơ đồ).
        _nutLuonHien = new HashSet<string>(_duongDanChon);
        if (CoBoLoc)
        {
            foreach (var khop in _nutKhop.Select(k => _cacNut[k]))
            {
                _nutLuonHien.UnionWith(LayDuongDan(khop).Select(n => n.KhoaNut));
            }
        }
    }

    private List<NutToChuc> LayDuongDan(NutToChuc nut)
    {
        var duongDan = new List<NutToChuc> { nut };
        while (_chaCuaNut.TryGetValue(nut.KhoaNut, out var cha)) { duongDan.Add(cha); nut = cha; }
        duongDan.Reverse();
        return duongDan;
    }

    private void ChonNut(NutToChuc nut)
    {
        if (BoCucCayToChuc.LaNutThemMuc(nut.KhoaNut))
        {
            HienThemMuc(BoCucCayToChuc.LayKhoaChaCuaNutThemMuc(nut.KhoaNut));
            return;
        }

        // Chọn một thẻ nằm ngoài nhánh đang xem (từ cây thư mục / tìm kiếm) thì quay về toàn bộ sơ đồ.
        if (_gocXem is not null && !LayDuongDan(nut).Any(n => n.KhoaNut == _gocXem.KhoaNut))
        {
            _gocXem = null;
            _lanCanKhung++;
        }

        _nutChon = nut;
        _coHienChiTiet = true;
        _tuKhoaTrongNhanh = string.Empty;
        foreach (var cha in LayDuongDan(nut).Where(n => !BoCucCayToChuc.LaLoaiKhongVe(n.Loai))) { _nutMo.Add(cha.KhoaNut); }
        CapNhatDuongDanChon();
    }

    /// <summary>Bấm thẻ "+ N mục nữa": hiện thêm một bậc con và mở danh sách đầy đủ ở panel.</summary>
    private void HienThemMuc(string khoaCha)
    {
        if (!_cacNut.TryGetValue(khoaCha, out var cha)) { return; }
        var hienTai = _soConHienThi.GetValueOrDefault(khoaCha, BoCucCayToChuc.SoConToiDaMoiNhanh);
        _soConHienThi[khoaCha] = hienTai + BoCucCayToChuc.SoConToiDaMoiNhanh;
        _nutChon = cha;
        _coHienChiTiet = true;
        CapNhatDuongDanChon();
    }

    private void DoiTrangThaiNhanh(NutToChuc nut)
    {
        if (!_nutMo.Remove(nut.KhoaNut)) { _nutMo.Add(nut.KhoaNut); }
        _nutChon = nut;
        CapNhatDuongDanChon();
    }

    /// <summary>Mở tới cấp nhóm (Học viện → Khoa → Nhóm) nhưng dừng khi số thẻ vượt ngưỡng đọc được.</summary>
    private void MoTatCa()
    {
        if (_goc == null) { return; }
        _thongBaoGioiHan = null;
        var hangDoi = new Queue<(NutToChuc Nut, int Cap)>();
        hangDoi.Enqueue((_goc, 0));
        while (hangDoi.Count > 0)
        {
            var (nut, cap) = hangDoi.Dequeue();
            if (cap > SoCapMoNhanh || !_nutHienThi.Contains(nut.KhoaNut)) { continue; }
            _nutMo.Add(nut.KhoaNut);
            var soThe = BoCucCayToChuc.Tao(_goc, _nutHienThi, _nutMo, _soConHienThi, _nutLuonHien).CacNut.Count;
            if (soThe > BoCucCayToChuc.SoNutToiDaTrenCanvas)
            {
                _nutMo.Remove(nut.KhoaNut);
                _thongBaoGioiHan = $"Đã giới hạn ở {BoCucCayToChuc.SoNutToiDaTrenCanvas} thẻ để sơ đồ dễ đọc. Hãy chọn một nhánh để xem chi tiết.";
                _coTapTrung = true;
                break;
            }

            foreach (var con in BoCucCayToChuc.LayConVeDuoc(nut, _nutHienThi)) { hangDoi.Enqueue((con, cap + 1)); }
        }
        _lanCanKhung++;
    }

    private void ThuGon()
    {
        _nutMo.Clear();
        _soConHienThi.Clear();
        _thongBaoGioiHan = null;
        if (_goc != null && _nutHienThi.Contains(_goc.KhoaNut)) { _nutMo.Add(_goc.KhoaNut); ChonNut(_goc); }
        _coHienChiTiet = false;
        _lanCanKhung++;
    }

    private void DoiTapTrung()
    {
        _coTapTrung = !_coTapTrung;
        _lanCanKhung++;
    }

    private void XoaBoLoc()
    {
        _tuKhoa = _loai = _trangThai = string.Empty;
        ApDungBoLoc();
    }

    private void LocCanRaSoat()
    {
        _tuKhoa = _loai = string.Empty;
        _trangThai = "ra-soat";
        ApDungBoLoc();
        var nut = _cacNut.Values.FirstOrDefault(n => _nutKhop.Contains(n.KhoaNut));
        if (nut != null) { ChonNut(nut); }
    }

    /// <summary>Con trực tiếp của nút đang chọn hiển thị trong bảng (lọc theo bộ lọc chung và ô tìm trong nhánh).</summary>
    private List<NutToChuc> LayBanGhiTrucThuoc(NutToChuc nut) => nut.Con
        .Where(n => _nutHienThi.Contains(n.KhoaNut))
        .Where(n => string.IsNullOrWhiteSpace(_tuKhoaTrongNhanh) || n.KhopTuKhoa(_tuKhoaTrongNhanh))
        .ToList();
}
