using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.ViewModels.TaiKhoan;

/// <summary>
/// ViewModel phục vụ chức năng đăng ký tài khoản người dùng mới tại Cổng truy cập.
/// </summary>
public class DangKyViewModel
{
    /// <summary>
    /// Họ và tên đầy đủ của người đăng ký.
    /// </summary>
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string HoVaTen { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ thư điện tử dùng làm tài khoản đăng nhập.
    /// </summary>
    [Required(ErrorMessage = "Email đăng ký là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại liên hệ tùy chọn.
    /// </summary>
    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    [Display(Name = "Số điện thoại")]
    public string? SoDienThoai { get; set; }

    /// <summary>
    /// Mật khẩu đăng nhập mới.
    /// </summary>
    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự, bao gồm chữ cái và số.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = string.Empty;

    /// <summary>
    /// Nhập lại mật khẩu để xác thực.
    /// </summary>
    [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc.")]
    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    [Compare("MatKhau", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
    public string XacNhanMatKhau { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn chuyển hướng sau khi đăng ký thành công.
    /// </summary>
    public string? DuongDanQuayLai { get; set; }
}
