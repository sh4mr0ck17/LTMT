using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Trình độ chuyên môn / Trình độ đào tạo của nhân sự.
/// </summary>
public class TrinhDoChuyenMon
{
    /// <summary>
    /// Khóa chính định danh nội bộ.
    /// </summary>
    public int TrinhDoChuyenMonId { get; set; }

    /// <summary>
    /// Mã trình độ nghiệp vụ duy nhất.
    /// </summary>
    [Required(ErrorMessage = "Mã trình độ là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã trình độ không được vượt quá 50 ký tự.")]
    public string MaTrinhDo { get; set; } = string.Empty;

    /// <summary>
    /// Tên trình độ chuyên môn (ví dụ: Tiến sĩ, Thạc sĩ, Cử nhân, Kỹ sư).
    /// </summary>
    [Required(ErrorMessage = "Tên trình độ là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Tên trình độ không được vượt quá 100 ký tự.")]
    public string TenTrinhDo { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết về trình độ chuyên môn.
    /// </summary>
    [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
    public string? MoTa { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của trình độ.
    /// </summary>
    public bool CoHoatDong { get; set; } = true;

    /// <summary>
    /// Danh sách nhân sự đạt trình độ này.
    /// </summary>
    public virtual ICollection<NhanSu> NhanSus { get; set; } = new List<NhanSu>();
}
