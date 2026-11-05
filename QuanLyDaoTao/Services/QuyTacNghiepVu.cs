using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Quy tắc nghiệp vụ thuần (không truy cập CSDL) dùng chung cho dịch vụ, Excel và giao diện.
/// </summary>
public static partial class QuyTacNghiepVu
{
    /// <summary>
    /// Số chữ số của CCCD.
    /// </summary>
    public const int DoDaiCccd = 12;

    /// <summary>
    /// Số phút hiển thị nhãn "Mới" sau khi tạo bản ghi.
    /// </summary>
    public const int SoPhutNhanMoi = 15;

    /// <summary>
    /// Số phút hiển thị nhãn "Vừa cập nhật" sau khi sửa bản ghi.
    /// </summary>
    public const int SoPhutNhanVuaCapNhat = 2;

    /// <summary>
    /// Năm sinh nhỏ nhất được chấp nhận.
    /// </summary>
    public const int NamSinhToiThieu = 1900;

    /// <summary>
    /// Văn hóa tiếng Việt dùng để so sánh, sắp xếp.
    /// </summary>
    public static readonly CultureInfo VanHoaViet = CultureInfo.GetCultureInfo("vi-VN");

    /// <summary>
    /// Bộ so sánh chuỗi tiếng Việt không phân biệt hoa thường.
    /// </summary>
    public static readonly StringComparer SoSanhTiengViet = StringComparer.Create(VanHoaViet, true);

    /// <summary>
    /// Các hệ đào tạo được hỗ trợ.
    /// </summary>
    public static readonly IReadOnlyList<string> DanhSachHeDaoTao = ["Chính quy", "Văn bằng 2", "Liên thông", "Bồi dưỡng"];

    /// <summary>Giới tính hợp lệ cho hồ sơ cán bộ và học viên.</summary>
    public static readonly IReadOnlyList<string> DanhSachGioiTinh = ["Nam", "Nữ"];

    /// <summary>Trạng thái công tác chuẩn của cán bộ (một danh sách dùng chung cho biểu mẫu, Studio và kiểm tra).</summary>
    public static readonly IReadOnlyList<string> DanhSachTrangThaiCanBo = ["Đang làm việc", "Tạm nghỉ", "Đi học", "Đã nghỉ việc"];

    /// <summary>Trạng thái học tập chuẩn của học viên.</summary>
    public static readonly IReadOnlyList<string> DanhSachTrangThaiHocVien = ["Đang học", "Bảo lưu", "Thôi học", "Đã tốt nghiệp"];

    /// <summary>Số năm tối đa của một niên khóa.</summary>
    public const int SoNamNienKhoaToiDa = 6;

    /// <summary>
    /// Chuẩn hóa niên khóa về dạng "YYYY-YYYY" (chấp nhận "2025 - 2029", "2025–2029").
    /// Trả về null nếu sai định dạng hoặc độ dài không hợp lý (1 đến <see cref="SoNamNienKhoaToiDa"/> năm).
    /// </summary>
    public static string? ChuanHoaNienKhoa(string? nienKhoa)
    {
        var khop = BieuThucNienKhoa().Match(nienKhoa ?? string.Empty);
        if (!khop.Success)
        {
            return null;
        }

        var dau = int.Parse(khop.Groups[1].Value, CultureInfo.InvariantCulture);
        var cuoi = int.Parse(khop.Groups[2].Value, CultureInfo.InvariantCulture);
        return cuoi - dau is >= 1 and <= SoNamNienKhoaToiDa && dau >= 1950 ? $"{dau}-{cuoi}" : null;
    }

    [GeneratedRegex(@"^\s*(\d{4})\s*[-–—]\s*(\d{4})\s*$")]
    private static partial Regex BieuThucNienKhoa();

    /// <summary>
    /// Danh mục 34 tỉnh/thành sau sắp xếp đơn vị hành chính 2025.
    /// </summary>
    public static readonly IReadOnlyList<string> DanhSachTinhThanh =
    [
        "Hà Nội", "Cao Bằng", "Tuyên Quang", "Điện Biên", "Lai Châu", "Sơn La", "Lào Cai", "Thái Nguyên",
        "Lạng Sơn", "Quảng Ninh", "Bắc Ninh", "Phú Thọ", "Hải Phòng", "Hưng Yên", "Ninh Bình", "Thanh Hóa",
        "Nghệ An", "Hà Tĩnh", "Quảng Trị", "Huế", "Đà Nẵng", "Quảng Ngãi", "Gia Lai", "Khánh Hòa", "Đắk Lắk",
        "Lâm Đồng", "Đồng Nai", "Thành phố Hồ Chí Minh", "Tây Ninh", "Đồng Tháp", "Vĩnh Long", "An Giang",
        "Cần Thơ", "Cà Mau"
    ];

    /// <summary>
    /// Danh mục cấp bậc hàm.
    /// </summary>
    public static readonly IReadOnlyList<string> DanhSachCapBacHam =
        ["Đại tướng", "Thượng tướng", "Trung tướng", "Thiếu tướng", "Đại tá", "Thượng tá", "Trung tá", "Thiếu tá",
         "Đại úy", "Thượng úy", "Trung úy", "Thiếu úy", "Không có"];

    /// <summary>
    /// Danh mục học hàm.
    /// </summary>
    public static readonly IReadOnlyList<string> DanhSachHocHam = ["Giáo sư", "Phó giáo sư"];

    /// <summary>
    /// Chức vụ học viên chuẩn hóa và chỉ tiêu tối đa trong một lớp (null = không giới hạn).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, int?> ChiTieuChucVuLop = new Dictionary<string, int?>
    {
        ["BT"] = 1,
        ["BP"] = 2,
        ["AT"] = 4,
        ["AP"] = 4,
        ["BTCĐ"] = 1,
        ["PBTCĐ"] = 2,
        ["UVĐ"] = 3,
        ["BPĐL"] = 1,
        ["BPHC"] = 1,
        ["PBTLCĐ"] = null,
        ["UVLCĐ"] = null,
        ["UVCĐ"] = null,
        ["TTTPN"] = null,
        ["TTTNC"] = null
    };

    /// <summary>
    /// Tên đầy đủ của các chức vụ học viên viết tắt.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> TenDayDuChucVuLop = new Dictionary<string, string>
    {
        ["BT"] = "B trưởng",
        ["BP"] = "B phó",
        ["AT"] = "A trưởng",
        ["AP"] = "A phó",
        ["BTCĐ"] = "Bí thư chi đoàn",
        ["PBTCĐ"] = "Phó bí thư chi đoàn",
        ["UVĐ"] = "Ủy viên đoàn",
        ["BPĐL"] = "B phó đời sống",
        ["BPHC"] = "B phó hành chính",
        ["PBTLCĐ"] = "Phó bí thư liên chi đoàn",
        ["UVLCĐ"] = "Ủy viên liên chi đoàn",
        ["UVCĐ"] = "Ủy viên chi đoàn",
        ["TTTPN"] = "Tổ trưởng tổ phụ nữ",
        ["TTTNC"] = "Tổ trưởng thanh niên"
    };

    private static readonly IReadOnlyDictionary<string, string> _biDanhChucVuLop =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["B trưởng"] = "BT",
            ["B phó"] = "BP",
            ["A trưởng"] = "AT",
            ["A phó"] = "AP",
            ["BP ĐL"] = "BPĐL",
            ["BTĐ"] = "BTCĐ",
            ["PBT"] = "PBTCĐ"
        };

    /// <summary>
    /// Chuẩn hóa chuỗi: Unicode FormC, gộp khoảng trắng, cắt hai đầu.
    /// </summary>
    public static string ChuanHoaChuoi(string? giaTri) => string.Join(' ',
        (giaTri ?? string.Empty).Normalize(NormalizationForm.FormC)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    /// <summary>
    /// Chuẩn hóa mã nghiệp vụ: bỏ khoảng trắng hai đầu và viết hoa.
    /// </summary>
    public static string ChuanHoaMa(string? ma) => (ma ?? string.Empty).Trim().ToUpperInvariant();

    /// <summary>
    /// Số điện thoại di động Việt Nam hợp lệ: 10 số, bắt đầu 03/05/07/08/09.
    /// </summary>
    public static bool LaSoDienThoaiHopLe(string? soDienThoai) =>
        !string.IsNullOrWhiteSpace(soDienThoai) && BieuThucSoDienThoai().IsMatch(soDienThoai.Trim());

    /// <summary>
    /// CCCD hợp lệ: đúng 12 chữ số.
    /// </summary>
    public static bool LaCccdHopLe(string? soCccd) =>
        !string.IsNullOrWhiteSpace(soCccd) && BieuThucCccd().IsMatch(soCccd.Trim());

    /// <summary>
    /// Kiểm tra ngày sinh trong khoảng hợp lệ (từ 1900 đến hôm nay).
    /// </summary>
    public static bool LaNgaySinhHopLe(DateTime ngaySinh) =>
        ngaySinh.Year >= NamSinhToiThieu && ngaySinh.Date <= DateTime.Today;

    /// <summary>
    /// Tách và chuẩn hóa danh sách chức vụ học viên (hỗ trợ viết tắt, tên đầy đủ, ghép bằng dấu phẩy).
    /// </summary>
    public static IReadOnlyList<string> TachChucVuLop(string? chucVu)
    {
        if (string.IsNullOrWhiteSpace(chucVu))
        {
            return [];
        }

        return chucVu.Split([',', ';', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ChuanHoaChuoi)
            .Where(x => x.Length > 0 && !string.Equals(x, "Học viên", StringComparison.OrdinalIgnoreCase))
            .Select(x => _biDanhChucVuLop.TryGetValue(x, out var ma) ? ma : x.ToUpperInvariant().Replace(" ", string.Empty))
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Ghép danh sách chức vụ đã chuẩn hóa thành chuỗi lưu trữ.
    /// </summary>
    public static string? GhepChucVuLop(IEnumerable<string> danhSach)
    {
        var ketQua = string.Join(", ", danhSach);
        return ketQua.Length == 0 ? null : ketQua;
    }

    /// <summary>
    /// Kiểm tra mã lớp. Với hệ Chính quy có khai báo khóa dạng Dxx (ví dụ D57), mã lớp bắt buộc
    /// theo mẫu B{số}D{khóa} và khớp khóa (B12D57). Trường hợp khác chấp nhận mã tự do không chứa khoảng trắng.
    /// </summary>
    /// <returns>Thông báo lỗi, hoặc null nếu hợp lệ.</returns>
    public static string? KiemTraMaLop(string maLop, string heDaoTao, string? khoaHoc)
    {
        var ma = ChuanHoaMa(maLop);
        if (ma.Length == 0 || ma.Contains(' '))
        {
            return "Mã lớp không được để trống hoặc chứa khoảng trắng.";
        }

        var khoa = ChuanHoaMa(khoaHoc);
        if (heDaoTao == "Chính quy" && BieuThucKhoaChinhQuy().IsMatch(khoa))
        {
            var khop = BieuThucMaLopChinhQuy().Match(ma);
            if (!khop.Success)
            {
                return $"Lớp hệ Chính quy khóa {khoa} phải có mã theo mẫu B{{số lớp}}{khoa}, ví dụ B12{khoa}.";
            }

            if (!string.Equals(khop.Groups[2].Value, khoa, StringComparison.Ordinal))
            {
                return $"Mã lớp {ma} không thuộc khóa {khoa}.";
            }
        }

        return null;
    }

    /// <summary>
    /// Suy ra khóa học (D57) từ mã lớp Chính quy, null nếu không suy được.
    /// </summary>
    public static string? SuyRaKhoaHoc(string? maLop)
    {
        var khop = BieuThucMaLopChinhQuy().Match(ChuanHoaMa(maLop));
        return khop.Success ? khop.Groups[2].Value : null;
    }

    /// <summary>
    /// Khóa sắp xếp tên tiếng Việt: theo tên chính, rồi họ và tên đệm.
    /// </summary>
    public static string KhoaSapXepTen(string? hoVaTen)
    {
        var phan = ChuanHoaChuoi(hoVaTen).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var ten = phan.Length == 0 ? string.Empty : phan[^1] + " " + string.Join(' ', phan[..^1]);
        return Convert.ToHexString(VanHoaViet.CompareInfo.GetSortKey(ten, CompareOptions.IgnoreCase).KeyData);
    }

    /// <summary>
    /// Tìm kiếm thân thiện: khớp nếu chuỗi chứa từ khóa, không phân biệt hoa thường và dấu tiếng Việt.
    /// </summary>
    public static bool ChuaTuKhoa(string? giaTri, string? tuKhoa)
    {
        if (string.IsNullOrWhiteSpace(tuKhoa))
        {
            return true;
        }

        if (string.IsNullOrEmpty(giaTri))
        {
            return false;
        }

        return VanHoaViet.CompareInfo.IndexOf(ChuanHoaChuoi(giaTri), ChuanHoaChuoi(tuKhoa),
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
    }

    /// <summary>
    /// Che bớt chuỗi nhạy cảm, giữ lại số ký tự cuối (ví dụ CCCD → ********1234).
    /// </summary>
    public static string? CheThongTin(string? giaTri, int soKyTuGiuLai = 3)
    {
        if (string.IsNullOrEmpty(giaTri))
        {
            return giaTri;
        }

        var giuLai = Math.Min(soKyTuGiuLai, giaTri.Length);
        return new string('•', giaTri.Length - giuLai) + giaTri[^giuLai..];
    }

    [GeneratedRegex(@"^0[35789][0-9]{8}$")]
    private static partial Regex BieuThucSoDienThoai();

    [GeneratedRegex(@"^[0-9]{12}$")]
    private static partial Regex BieuThucCccd();

    [GeneratedRegex(@"^B([1-9][0-9]?)(D[0-9]{2})$")]
    private static partial Regex BieuThucMaLopChinhQuy();

    [GeneratedRegex(@"^D[0-9]{2}$")]
    private static partial Regex BieuThucKhoaChinhQuy();
}
