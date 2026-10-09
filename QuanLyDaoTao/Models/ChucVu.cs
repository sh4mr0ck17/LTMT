using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Chức vụ công tác của nhân sự.
/// </summary>
public class ChucVu
{
    /// <summary>
    /// Khóa chính định danh nội bộ.
    /// </summary>
    public int ChucVuId { get; set; }

    /// <summary>
    /// Mã chức vụ nghiệp vụ duy nhất.
    /// </summary>
    [Required(ErrorMessage = "Mã chức vụ là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã chức vụ không được vượt quá 50 ký tự.")]
    public string MaChucVu { get; set; } = string.Empty;

    /// <summary>
    /// Tên chức vụ đầy đủ.
    /// </summary>
    [Required(ErrorMessage = "Tên chức vụ là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Tên chức vụ không được vượt quá 100 ký tự.")]
    public string TenChucVu { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả quyền hạn và trách nhiệm của chức vụ.
    /// </summary>
    [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
    public string? MoTa { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của chức vụ.
    /// </summary>
    public bool CoHoatDong { get; set; } = true;

    /// <summary>
    /// Danh sách nhân sự giữ chức vụ này.
    /// </summary>
    public virtual ICollection<NhanSu> NhanSus { get; set; } = new List<NhanSu>();
}
