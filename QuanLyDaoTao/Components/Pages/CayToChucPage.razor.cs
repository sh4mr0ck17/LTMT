using System.Data.Common;
using Microsoft.AspNetCore.Components;
using QuanLyDaoTao.Models.ToChuc;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Components.Pages;

/// <summary>Điều hướng cơ cấu tổ chức và theo dõi các hồ sơ trực thuộc.</summary>
public partial class CayToChucPage
{
    [Inject] private ICayToChucDichVu CayToChucDichVu { get; set; } = default!;
    [Inject] private ILogger<CayToChucPage> NhatKy { get; set; } = default!;
    private NutToChuc? _goc;
    private NutToChuc? _nutChon;
    private readonly Dictionary<string, NutToChuc> _cacNut = new();
    private readonly Dictionary<string, NutToChuc> _chaCuaNut = new();
    private readonly HashSet<string> _nutHienThi = new();
    private readonly HashSet<string> _nutKhop = new();
    private readonly HashSet<string> _nutMo = new();
    private HashSet<string> _duongDanChon = new();
    private string _tuKhoa = string.Empty;
    private string _loai = string.Empty;
    private string _trangThai = string.Empty;
    private string? _loiTaiDuLieu;
    private bool _coDangTai;
    private bool _coHienChiTiet;
    private int _lanCanKhung;
    private DateTimeOffset? _lanCapNhat;
    private bool CoBoLoc => !string.IsNullOrWhiteSpace(_tuKhoa) || _loai != "" || _trangThai != "";

    /// <summary>Tải cơ cấu ban đầu.</summary>
    protected override async Task OnInitializedAsync() => await TaiDuLieuAsync();

    private async Task TaiDuLieuAsync()
    {
        if (_coDangTai) { return; }
        _coDangTai = true;
        _loiTaiDuLieu = null;
        try
        {
            var khoaNutCu = _nutChon?.KhoaNut;
            var lanDau = _goc == null;
            var gocMoi = await CayToChucDichVu.LayCayAsync();
            _goc = gocMoi;
            _cacNut.Clear();
            _chaCuaNut.Clear();
            LapChiMuc(gocMoi, null);
            _nutMo.RemoveWhere(k => !_cacNut.ContainsKey(k));
            if (lanDau) { _nutMo.Add(gocMoi.KhoaNut); }
            _nutChon = khoaNutCu != null ? _cacNut.GetValueOrDefault(khoaNutCu) : gocMoi;
            _lanCapNhat = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
            ApDungBoLoc();
        }
        catch (DbException loi)
        {
            NhatKy.LogError(loi, "Không thể tải cây tổ chức.");
            _loiTaiDuLieu = "Không thể đọc dữ liệu tổ chức.";
        }
        catch (InvalidOperationException loi)
        {
            NhatKy.LogError(loi, "Không thể dựng cây tổ chức.");
            _loiTaiDuLieu = "Không thể tải cơ cấu tổ chức lúc này.";
        }
        finally { _coDangTai = false; }
    }

    private void LapChiMuc(NutToChuc nut, NutToChuc? cha)
    {
        _cacNut.Add(nut.KhoaNut, nut);
        if (cha != null) { _chaCuaNut.Add(nut.KhoaNut, cha); }
        foreach (var con in nut.Con) { LapChiMuc(con, nut); }
    }

    private void ApDungBoLoc()
    {
        _nutHienThi.Clear();
        _nutKhop.Clear();
        if (_goc == null) { return; }
        var ketQua = BoLocCayToChuc.Loc(_goc, _tuKhoa, _loai, _trangThai);
        _nutHienThi.UnionWith(ketQua.HienThi);
        _nutKhop.UnionWith(ketQua.Khop);
        if (CoBoLoc)
        {
            foreach (var khoa in _nutHienThi.Where(k => _cacNut[k].Con.Count > 0)) { _nutMo.Add(khoa); }
        }
        if (_nutChon == null || !_nutHienThi.Contains(_nutChon.KhoaNut) || (CoBoLoc && !_nutKhop.Contains(_nutChon.KhoaNut)))
        {
            _nutChon = _cacNut.Values.FirstOrDefault(n => _nutKhop.Contains(n.KhoaNut))
                ?? (_nutHienThi.Contains(_goc.KhoaNut) ? _goc : null);
        }
        _duongDanChon = _nutChon == null ? new() : LayDuongDan(_nutChon).Select(n => n.KhoaNut).ToHashSet();
        _lanCanKhung++;
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
        _nutChon = nut;
        _coHienChiTiet = true;
        var duongDan = LayDuongDan(nut);
        _duongDanChon = duongDan.Select(n => n.KhoaNut).ToHashSet();
        foreach (var cha in duongDan) { _nutMo.Add(cha.KhoaNut); }
    }

    private void DoiTrangThaiNhanh(NutToChuc nut)
    {
        if (!_nutMo.Remove(nut.KhoaNut)) { _nutMo.Add(nut.KhoaNut); }
        _nutChon = nut;
        _duongDanChon = LayDuongDan(nut).Select(n => n.KhoaNut).ToHashSet();
    }

    private void MoTatCa()
    {
        foreach (var khoa in _nutHienThi) { _nutMo.Add(khoa); }
        _lanCanKhung++;
    }

    private void ThuGon()
    {
        _nutMo.Clear();
        if (_goc != null && _nutHienThi.Contains(_goc.KhoaNut)) { _nutMo.Add(_goc.KhoaNut); ChonNut(_goc); }
        _coHienChiTiet = false;
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
}
