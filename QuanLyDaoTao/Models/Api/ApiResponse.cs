namespace QuanLyDaoTao.Models.Api;

/// <summary>
/// Cấu trúc phản hồi JSON chuẩn hóa cho toàn bộ hệ sinh thái RESTful API của hệ thống.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của gói tin trả về.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Trạng thái thành công hay thất bại của yêu cầu.
    /// </summary>
    public bool ThanhCong { get; set; }

    /// <summary>
    /// Mã trạng thái HTTP chuẩn (200, 201, 400, 404, 500...).
    /// </summary>
    public int MaTrangThai { get; set; }

    /// <summary>
    /// Thông điệp phản hồi bằng Tiếng Việt thân thiện với người dùng.
    /// </summary>
    public string ThongDiep { get; set; } = string.Empty;

    /// <summary>
    /// Dữ liệu kết quả thực thi (nếu có).
    /// </summary>
    public T? DuLieu { get; set; }

    /// <summary>
    /// Danh sách các lỗi chi tiết (nếu có).
    /// </summary>
    public List<string>? DanhSachLoi { get; set; }

    /// <summary>
    /// Thời điểm máy chủ xử lý xong yêu cầu (UTC).
    /// </summary>
    public DateTime ThoiGian { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Sinh phản hồi thành công tiện lợi.
    /// </summary>
    public static ApiResponse<T?> ThanhCongResponse(T? duLieu = default, string thongDiep = "Thao tác thực hiện thành công.", int maTrangThai = 200)
    {
        return new ApiResponse<T?>
        {
            ThanhCong = true,
            MaTrangThai = maTrangThai,
            ThongDiep = thongDiep,
            DuLieu = duLieu
        };
    }

    /// <summary>
    /// Sinh phản hồi thất bại tiện lợi.
    /// </summary>
    public static ApiResponse<T?> ThatBaiResponse(string thongDiep, int maTrangThai = 400, List<string>? danhSachLoi = null)
    {
        return new ApiResponse<T?>
        {
            ThanhCong = false,
            MaTrangThai = maTrangThai,
            ThongDiep = thongDiep,
            DanhSachLoi = danhSachLoi
        };
    }
}
