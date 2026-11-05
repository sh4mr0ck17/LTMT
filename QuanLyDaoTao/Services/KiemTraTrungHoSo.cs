using System.Text;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Dịch vụ kiểm tra và phân biệt hồ sơ nhập lặp, hồ sơ mâu thuẫn và trùng trường định danh.
/// Kế thừa và tích hợp từ repository LTMT-demo (nhánh feat/optimize).
/// </summary>
public static class KiemTraTrungHoSo
{
    /// <summary>
    /// Kiểm tra trùng lặp hồ sơ cán bộ giảng viên đối chiếu với danh sách cán bộ và học viên hiện có.
    /// </summary>
    public static string? KiemTraCanBo(NhanSu moi, IEnumerable<NhanSu> danhSachCanBo, IEnumerable<HocVien> danhSachHocVien)
    {
        ArgumentNullException.ThrowIfNull(moi);
        var cacHoSo = danhSachCanBo
            .Where(x => x.NhanSuId != moi.NhanSuId && x.MaNhanSu != moi.MaNhanSu)
            .Select(x => new HoSoDinhDanh(x.HoVaTen, x.SoDienThoai ?? "", x.SoCCCD ?? "", GiongNhau(CacTruongCanBo(moi), CacTruongCanBo(x))))
            .Concat(danhSachHocVien.Select(x => new HoSoDinhDanh(x.HoVaTen, x.SoDienThoai ?? "", "", false)));

        return TimLoi(moi.HoVaTen, moi.SoDienThoai ?? "", moi.SoCCCD ?? "", cacHoSo);
    }

    /// <summary>
    /// Kiểm tra trùng lặp hồ sơ học viên đối chiếu với danh sách học viên và cán bộ hiện có.
    /// </summary>
    public static string? KiemTraHocVien(HocVien moi, IEnumerable<HocVien> danhSachHocVien, IEnumerable<NhanSu> danhSachCanBo)
    {
        ArgumentNullException.ThrowIfNull(moi);
        var cacHoSo = danhSachHocVien
            .Where(x => x.HocVienId != moi.HocVienId && x.MaHocVien != moi.MaHocVien)
            .Select(x => new HoSoDinhDanh(x.HoVaTen, x.SoDienThoai ?? "", "", GiongNhau(CacTruongHocVien(moi), CacTruongHocVien(x))))
            .Concat(danhSachCanBo.Select(x => new HoSoDinhDanh(x.HoVaTen, x.SoDienThoai ?? "", x.SoCCCD ?? "", false)));

        return TimLoi(moi.HoVaTen, moi.SoDienThoai ?? "", "", cacHoSo);
    }

    private static string?[] CacTruongCanBo(NhanSu x) =>
    [
        x.HoVaTen,
        x.NgaySinh.ToString("yyyy-MM-dd"),
        x.GioiTinh,
        x.DiaChi,
        x.SoDienThoai,
        x.SoCCCD,
        x.Email,
        x.KhoaId?.ToString(),
        x.BoMonId?.ToString(),
        x.ChucVuId?.ToString(),
        x.TrinhDoChuyenMonId?.ToString(),
        x.HocHam,
        x.HocVi,
        x.ChuyenMonDaoTao
    ];

    private static string?[] CacTruongHocVien(HocVien x) =>
    [
        x.HoVaTen,
        x.NgaySinh.ToString("yyyy-MM-dd"),
        x.GioiTinh,
        x.SoDienThoai,
        x.Email,
        x.LopHocId.ToString(),
        x.DiaChi
    ];

    private static string ChuanHoa(string? x) => string.Join(" ",
        (x ?? "").Normalize(NormalizationForm.FormC).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static bool GiongNhau(string?[] a, string?[] b)
        => a.Select(ChuanHoa).SequenceEqual(b.Select(ChuanHoa), StringComparer.OrdinalIgnoreCase);

    private sealed record HoSoDinhDanh(string HoTen, string SoDienThoai, string SoCCCD, bool TrungToanBo);

    private static string? TimLoi(string hoTen, string soDienThoai, string soCCCD, IEnumerable<HoSoDinhDanh> cacHoSo)
    {
        var dt = soDienThoai.Trim();
        var cccd = soCCCD.Trim();

        var trung = cacHoSo.Where(x => 
            (!string.IsNullOrEmpty(dt) && x.SoDienThoai == dt) || 
            (!string.IsNullOrEmpty(cccd) && x.SoCCCD == cccd)).ToList();

        if (trung.Any(x => x.TrungToanBo))
        {
            return "Hồ sơ trùng toàn bộ thông tin với bản ghi đã có trong hệ thống (không tính mã hồ sơ). Vui lòng không tạo bản ghi lặp.";
        }

        if (!string.IsNullOrEmpty(dt) && !string.IsNullOrEmpty(cccd) &&
            trung.Any(x => x.SoDienThoai == dt && x.SoCCCD == cccd &&
            string.Equals(ChuanHoa(x.HoTen), ChuanHoa(hoTen), StringComparison.OrdinalIgnoreCase)))
        {
            return "Họ tên, số điện thoại và số CCCD trùng hồ sơ đã có trong hệ thống, nhưng các thông tin khác bị mâu thuẫn. Vui lòng kiểm tra lại.";
        }

        var truongTrung = new List<string>();
        if (!string.IsNullOrEmpty(cccd) && trung.Any(x => x.SoCCCD == cccd))
        {
            truongTrung.Add("Số CCCD");
        }
        if (!string.IsNullOrEmpty(dt) && trung.Any(x => x.SoDienThoai == dt))
        {
            truongTrung.Add("Số điện thoại");
        }

        return truongTrung.Count == 0 ? null : string.Join(" và ", truongTrung) + " đã được sử dụng cho hồ sơ khác trong hệ thống. Vui lòng kiểm tra lại.";
    }
}
