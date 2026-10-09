using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.ViewModels.TaiKhoan;

/// <summary>
/// ViewModel phục vụ chức năng tạo mới tài khoản người dùng bởi Quản trị viên.
/// </summary>
public class TaoTaiKhoanViewModel
{
    /// <summary>
    /// Địa chỉ email đăng nhập của tài khoản.
    /// </summary>
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Họ và tên hiển thị.
    /// </summary>
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string HoVaTen { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu ban đầu.
    /// </summary>
    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Mật khẩu phải từ {2} ký tự trở lên.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = string.Empty;

    /// <summary>
    /// Vai trò phân quyền cho tài khoản.
    /// </summary>
    [Required(ErrorMessage = "Vai trò phân quyền là bắt buộc.")]
    [Display(Name = "Vai trò")]
    public string VaiTro { get; set; } = string.Empty;

    /// <summary>
    /// Mã nhân sự liên kết nếu có.
    /// </summary>
    [StringLength(50, ErrorMessage = "Mã nhân sự không được vượt quá 50 ký tự.")]
    [Display(Name = "Mã nhân sự liên kết")]
    public string? MaNhanSuLienKet { get; set; }
}
