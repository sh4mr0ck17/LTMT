namespace QuanLyDaoTao.Models.Excel;

/// <summary>
/// Đại diện cho một lỗi chi tiết khi kiểm tra tệp Excel nhập vào.
/// </summary>
public class LoiChiTietExcel
{
    /// <summary>
    /// Số dòng Excel thực tế chứa lỗi (tính từ 1).
    /// </summary>
    public int SoDong { get; set; }

    /// <summary>
    /// Tên cột hoặc tên trường nghiệp vụ xảy ra lỗi.
    /// </summary>
    public string TenTruong { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ ô Excel (ví dụ: A2, B5, C10) nếu xác định được.
    /// </summary>
    public string? DiaChiO { get; set; }

    /// <summary>
    /// Giá trị đầu vào do người dùng nhập trong ô.
    /// </summary>
    public string? GiaTriDauVao { get; set; }

    /// <summary>
    /// Mã lỗi chuẩn hóa cố định (ví dụ: ERR_REQUIRED, ERR_FORMAT_EMAIL, ERR_FK_NOT_FOUND).
    /// </summary>
    public string MaLoi { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết nội dung lỗi bằng Tiếng Việt.
    /// </summary>
    public string MoTaLoi { get; set; } = string.Empty;

    /// <summary>
    /// Hướng dẫn cho người dùng cách khắc phục lỗi.
    /// </summary>
    public string HuongXuLy { get; set; } = string.Empty;

    /// <summary>
    /// Mức độ nghiêm trọng của lỗi (Lỗi nghiêm trọng, Cảnh báo).
    /// </summary>
    public string MucDoLoi { get; set; } = "Lỗi nghiêm trọng";
}
