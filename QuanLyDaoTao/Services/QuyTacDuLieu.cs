using System.Globalization;
using System.Text.RegularExpressions;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Quy tắc và tiện ích dùng chung cho dữ liệu đào tạo Học viện An ninh Nhân dân (C500):
/// Collation tiếng Việt, danh mục 34 tỉnh/thành, quy tắc chuyên ngành và sinh họ tên mô phỏng.
/// Kế thừa và mở rộng từ repository LTMT-demo (nhánh feat/optimize).
/// </summary>
public static class QuyTacDuLieu
{
    /// <summary>
    /// So sánh và khớp tên ở cuối họ tên tiếng Việt (bỏ qua khác biệt hoa/thường và khoảng trắng thừa).
    /// </summary>
    public static bool TenKhopTuKhoa(string hoTen, string tuKhoa)
    {
        if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(tuKhoa))
        {
            return false;
        }

        static string ChuanHoa(string giaTri) => string.Join(" ",
            giaTri.Normalize(System.Text.NormalizationForm.FormC)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        var cumTu = ChuanHoa(tuKhoa);
        return cumTu.Length == 0 || (" " + ChuanHoa(hoTen))
            .EndsWith(" " + cumTu, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Danh mục 34 tỉnh/thành chuẩn Việt Nam phục vụ quê quán và địa chỉ.
    /// </summary>
    public static readonly string[] TinhThanh =
    [
        "Hà Nội", "Cao Bằng", "Tuyên Quang", "Điện Biên", "Lai Châu",
        "Sơn La", "Lào Cai", "Thái Nguyên", "Lạng Sơn", "Quảng Ninh",
        "Bắc Ninh", "Phú Thọ", "Hải Phòng", "Hưng Yên", "Ninh Bình",
        "Thanh Hóa", "Nghệ An", "Hà Tĩnh", "Quảng Trị", "Huế",
        "Đà Nẵng", "Quảng Ngãi", "Gia Lai", "Khánh Hòa", "Đắk Lắk",
        "Lâm Đồng", "Đồng Nai", "Thành phố Hồ Chí Minh", "Tây Ninh", "Đồng Tháp",
        "Vĩnh Long", "An Giang", "Cần Thơ", "Cà Mau"
    ];

    /// <summary>
    /// Bộ so sánh chuỗi theo chuẩn văn hóa tiếng Việt (vi-VN).
    /// </summary>
    public static readonly StringComparer SoSanhTiengViet = StringComparer.Create(CultureInfo.GetCultureInfo("vi-VN"), true);

    /// <summary>
    /// Sinh khóa sắp xếp theo thứ tự: Tên chính trước, sau đó tới Họ và Tên đệm.
    /// Giúp bảng danh sách sắp xếp danh sách cán bộ, học viên tự nhiên theo thứ tự alphabet Việt Nam.
    /// </summary>
    public static string KhoaSapXep(string hoTen)
    {
        if (string.IsNullOrWhiteSpace(hoTen))
        {
            return string.Empty;
        }

        var phan = hoTen.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var ten = phan.Length == 0 ? "" : phan[^1] + " " + string.Join(" ", phan[..^1]);
        return Convert.ToHexString(CultureInfo.GetCultureInfo("vi-VN").CompareInfo.GetSortKey(ten, CompareOptions.IgnoreCase).KeyData);
    }

    /// <summary>
    /// Phân loại chuyên ngành đào tạo chính quy theo số thứ tự lớp C500.
    /// </summary>
    public static string ChuyenNganhChinhQuy(int so) => so switch
    {
        <= 3 => "Trinh sát an ninh",
        <= 7 => "Điều tra hình sự",
        8 => "Quản lý xuất nhập cảnh",
        <= 12 => "ANM & PCTPCNC",
        _ => "Công nghệ thông tin"
    };

    /// <summary>
    /// Kiểm tra tính hợp lệ của mã lớp, hệ đào tạo và khóa học C500.
    /// </summary>
    public static bool LopHopLe(string ma, string he, string khoa, string chuyenNganh)
    {
        if (he == "Chính quy")
        {
            var m = Regex.Match(ma, @"^B([1-9]|1[0-5])(D5[3-8])$");
            return m.Success && m.Groups[2].Value == khoa && ChuyenNganhChinhQuy(int.Parse(m.Groups[1].Value)) == chuyenNganh;
        }
        var vb = Regex.Match(ma, @"^B([1-9][0-9]*)(K[3-6])$");
        return he == "Văn bằng 2" && vb.Success && vb.Groups[2].Value == khoa && chuyenNganh is "Điều tra" or "ANM & PCTPCNC";
    }

    /// <summary>
    /// Sinh họ tên tiếng Việt ổn định hoán vị không đổi qua các lần chạy (Deterministic pseudo-random).
    /// </summary>
    public static string TaoHoTen(int so, string gioiTinh)
    {
        string[] ho = ["Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý", "Đinh", "Đoàn", "Trịnh", "Mai"];
        string[] dem = gioiTinh == "Nữ" 
            ? ["Thị", "Ngọc", "Thu", "Khánh", "Minh", "Bảo", "Thanh", "Phương", "Thùy", "Hồng", "Diệu", "Kim"] 
            : ["Văn", "Đức", "Quốc", "Minh", "Hoàng", "Anh", "Tuấn", "Gia", "Hữu", "Thanh", "Đình", "Nhật"];
        string[] ten = gioiTinh == "Nữ" 
            ? ["Linh", "Hà", "Anh", "Châu", "Ngọc", "Trang", "Hương", "Lan", "Mai", "Vy", "Yến", "Huyền", "Thảo", "Phương", "Trâm", "Ngân", "Quỳnh", "Nhung", "Hoa", "Chi", "Oanh", "Hiền", "Dung", "Tâm"] 
            : ["Huy", "Bảo", "Quân", "Phúc", "Tuấn", "Khang", "Dũng", "Sơn", "Nam", "Long", "Minh", "Khôi", "Đạt", "Hiếu", "Kiên", "Lâm", "Phong", "Hùng", "Vinh", "Hải", "Thành", "Trung", "Cường", "Bình"];
        var n = (so * 137) % (ho.Length * dem.Length * ten.Length);
        return $"{ho[n % ho.Length]} {dem[n / ho.Length % dem.Length]} {ten[n / (ho.Length * dem.Length)]}";
    }
}
