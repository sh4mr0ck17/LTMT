using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.ViewModels.TaiKhoan;

/// <summary>
/// ViewModel phục vụ chức năng đổi mật khẩu của tài khoản.
/// </summary>
public class DoiMatKhauViewModel
{
    /// <summary>
    /// Mật khẩu hiện tại của người dùng.
    /// </summary>
    [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu hiện tại")]
    public string MatKhauHienTai { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu mới cần đổi.
    /// </summary>
    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Mật khẩu phải từ {2} ký tự trở lên.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string MatKhauMoi { get; set; } = string.Empty;

    /// <summary>
    /// Xác nhận lại mật khẩu mới.
    /// </summary>
    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu mới")]
    [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string XacNhanMatKhauMoi { get; set; } = string.Empty;
}
