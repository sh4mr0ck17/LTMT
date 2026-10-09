using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.ViewModels.TaiKhoan;

/// <summary>
/// ViewModel phục vụ chức năng đăng nhập tài khoản vào hệ thống.
/// </summary>
public class DangNhapViewModel
{
    /// <summary>
    /// Địa chỉ email đăng nhập.
    /// </summary>
    [Required(ErrorMessage = "Email đăng nhập là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu đăng nhập.
    /// </summary>
    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = string.Empty;

    /// <summary>
    /// Tùy chọn duy trì phiên đăng nhập.
    /// </summary>
    [Display(Name = "Ghi nhớ phiên đăng nhập")]
    public bool GhiNhoDangNhap { get; set; }

    /// <summary>
    /// Đường dẫn chuyển hướng sau khi đăng nhập thành công.
    /// </summary>
    public string? DuongDanQuayLai { get; set; }
}
