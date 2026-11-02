using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Tài khoản người dùng hệ thống (kế thừa IdentityUser).
/// </summary>
public class TaiKhoan : IdentityUser
{
    /// <summary>
    /// Họ và tên hiển thị của chủ tài khoản.
    /// </summary>
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
    public string HoVaTen { get; set; } = string.Empty;

    /// <summary>
    /// Mã nhân sự liên kết (nếu tài khoản thuộc về một cán bộ/nhân sự).
    /// </summary>
    [StringLength(50, ErrorMessage = "Mã nhân sự liên kết không được vượt quá 50 ký tự.")]
    public string? MaNhanSuLienKet { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của tài khoản.
    /// </summary>
    public bool CoHoatDong { get; set; } = true;

    /// <summary>
    /// Ngày khởi tạo tài khoản.
    /// </summary>
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Đường dẫn ảnh đại diện (Avatar mặt người) của chủ tài khoản.
    /// </summary>
    [NotMapped]
    public string? AnhDaiDien { get; set; }
}
