using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Khoa trong hệ thống đào tạo.
/// </summary>
public class Khoa
{
    /// <summary>
    /// Khóa chính định danh nội bộ.
    /// </summary>
    public int KhoaId { get; set; }

    /// <summary>
    /// Mã khoa nghiệp vụ duy nhất.
    /// </summary>
    [Required(ErrorMessage = "Mã khoa là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã khoa không được vượt quá 50 ký tự.")]
    public string MaKhoa { get; set; } = string.Empty;

    /// <summary>
    /// Tên khoa đầy đủ.
    /// </summary>
    [Required(ErrorMessage = "Tên khoa là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Tên khoa không được vượt quá 200 ký tự.")]
    public string TenKhoa { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết về khoa.
    /// </summary>
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? MoTa { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ của khoa.
    /// </summary>
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    public string? SoDienThoai { get; set; }

    /// <summary>
    /// Địa chỉ email liên hệ của khoa.
    /// </summary>
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
    public string? Email { get; set; }

    /// <summary>
    /// Địa điểm văn phòng khoa.
    /// </summary>
    [StringLength(200, ErrorMessage = "Địa điểm không được vượt quá 200 ký tự.")]
    public string? DiaDiem { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của khoa.
    /// </summary>
    public bool CoHoatDong { get; set; } = true;

    /// <summary>
    /// Ngày tạo bản ghi.
    /// </summary>
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Danh sách bộ môn trực thuộc khoa.
    /// </summary>
    public virtual ICollection<BoMon> BoMons { get; set; } = new List<BoMon>();

    /// <summary>
    /// Danh sách lớp học thuộc khoa.
    /// </summary>
    public virtual ICollection<LopHoc> LopHocs { get; set; } = new List<LopHoc>();

    /// <summary>
    /// Danh sách nhân sự thuộc khoa.
    /// </summary>
    public virtual ICollection<NhanSu> NhanSus { get; set; } = new List<NhanSu>();
}
