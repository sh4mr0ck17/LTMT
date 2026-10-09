using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể ghi nhận Nhật ký kiểm toán và lịch sử thao tác hệ thống.
/// </summary>
public class NhatKyHeThong
{
    /// <summary>
    /// Khóa chính định danh nội bộ.
    /// </summary>
    public int NhatKyId { get; set; }

    /// <summary>
    /// Tên tài khoản hoặc người thực hiện thao tác.
    /// </summary>
    [Required(ErrorMessage = "Người thực hiện là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Người thực hiện không được vượt quá 100 ký tự.")]
    public string NguoiThucHien { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm thực hiện thao tác.
    /// </summary>
    public DateTime ThoiDiem { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Loại hành động (Thêm mới, Chỉnh sửa, Xóa, Nhập Excel, Xuất Excel, Đăng nhập).
    /// </summary>
    [Required(ErrorMessage = "Hành động là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Hành động không được vượt quá 50 ký tự.")]
    public string HanhDong { get; set; } = string.Empty;

    /// <summary>
    /// Bảng dữ liệu / Thực thể chịu tác động (Khoa, LopHoc, NhanSu, ...).
    /// </summary>
    [Required(ErrorMessage = "Bảng dữ liệu là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Bảng dữ liệu không được vượt quá 50 ký tự.")]
    public string BangDuLieu { get; set; } = string.Empty;

    /// <summary>
    /// Khóa bản ghi bị tác động (Mã hoặc ID bản ghi).
    /// </summary>
    [StringLength(100, ErrorMessage = "Khóa bản ghi không được vượt quá 100 ký tự.")]
    public string? KhoaBanGhi { get; set; }

    /// <summary>
    /// Tóm tắt dữ liệu trước khi thay đổi (JSON hoặc văn bản tóm tắt).
    /// </summary>
    [StringLength(2000, ErrorMessage = "Giá trị cũ không được vượt quá 2000 ký tự.")]
    public string? GiaTriCu { get; set; }

    /// <summary>
    /// Tóm tắt dữ liệu sau khi thay đổi (JSON hoặc văn bản tóm tắt).
    /// </summary>
    [StringLength(2000, ErrorMessage = "Giá trị mới không được vượt quá 2000 ký tự.")]
    public string? GiaTriMoi { get; set; }

    /// <summary>
    /// Kết quả thực hiện thao tác (Thành công, Thất bại).
    /// </summary>
    [StringLength(50, ErrorMessage = "Kết quả không được vượt quá 50 ký tự.")]
    public string KetQua { get; set; } = "Thành công";

    /// <summary>
    /// Địa chỉ IP của máy khách gửi yêu cầu.
    /// </summary>
    [StringLength(50, ErrorMessage = "Địa chỉ IP không được vượt quá 50 ký tự.")]
    public string? DiaChiIp { get; set; }
}
