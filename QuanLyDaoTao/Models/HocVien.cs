using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Học viên tham gia đào tạo.
/// </summary>
public class HocVien
{
    /// <summary>
    /// Khóa chính định danh nội bộ.
    /// </summary>
    public int HocVienId { get; set; }

    /// <summary>
    /// Mã học viên duy nhất.
    /// </summary>
    [Required(ErrorMessage = "Mã học viên là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã học viên không được vượt quá 50 ký tự.")]
    public string MaHocVien { get; set; } = string.Empty;

    /// <summary>
    /// Họ và tên học viên.
    /// </summary>
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
    public string HoVaTen { get; set; } = string.Empty;

    /// <summary>
    /// Ngày sinh của học viên.
    /// </summary>
    [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
    public DateTime NgaySinh { get; set; }

    /// <summary>
    /// Giới tính của học viên.
    /// </summary>
    [Required(ErrorMessage = "Giới tính là bắt buộc.")]
    [StringLength(20, ErrorMessage = "Giới tính không được vượt quá 20 ký tự.")]
    public string GioiTinh { get; set; } = "Nam";

    /// <summary>
    /// Địa chỉ email của học viên.
    /// </summary>
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
    public string? Email { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ.
    /// </summary>
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    public string? SoDienThoai { get; set; }

    /// <summary>
    /// Địa chỉ cư trú.
    /// </summary>
    [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
    public string? DiaChi { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Lớp học.
    /// </summary>
    [Required(ErrorMessage = "Lớp học là bắt buộc.")]
    public int LopHocId { get; set; }

    /// <summary>
    /// Đối tượng Lớp học của học viên.
    /// </summary>
    public virtual LopHoc? LopHoc { get; set; }

    /// <summary>
    /// Trạng thái hoạt động / theo học.
    /// </summary>
    public bool CoHoatDong { get; set; } = true;

    /// <summary>
    /// Ngày tạo bản ghi.
    /// </summary>
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
}
