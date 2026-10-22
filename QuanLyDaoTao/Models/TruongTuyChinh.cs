using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể định nghĩa một trường (cột) dữ liệu tùy chỉnh do Quản trị viên cấu hình cho từng bảng nghiệp vụ.
/// </summary>
public class TruongTuyChinh
{
    /// <summary>
    /// Khóa chính định danh nội bộ của trường tùy chỉnh.
    /// </summary>
    public int TruongTuyChinhId { get; set; }

    /// <summary>
    /// Tên bảng / nhóm quản lý áp dụng (Khoa, BoMon, LopHoc, HocVien, NhanSu, MonHoc, PhanCongGiangDay, ChucVu, TrinhDoChuyenMon).
    /// </summary>
    [Required(ErrorMessage = "Tên bảng áp dụng là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Tên bảng không được vượt quá 50 ký tự.")]
    public string TenBang { get; set; } = string.Empty;

    /// <summary>
    /// Mã định danh kỹ thuật của trường (ví dụ: SoQuyetDinh, GhiChuRieng).
    /// </summary>
    [Required(ErrorMessage = "Mã trường là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã trường không được vượt quá 50 ký tự.")]
    public string MaTruong { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị trên giao diện bảng và biểu mẫu.
    /// </summary>
    [Required(ErrorMessage = "Tên hiển thị của trường là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Tên hiển thị không được vượt quá 100 ký tự.")]
    public string TenHienThi { get; set; } = string.Empty;

    /// <summary>
    /// Kiểu dữ liệu của trường (VanBan, SoNguyen, NgayThang, Logic, DanhSach).
    /// </summary>
    [Required(ErrorMessage = "Kiểu dữ liệu là bắt buộc.")]
    [StringLength(30, ErrorMessage = "Kiểu dữ liệu không được vượt quá 30 ký tự.")]
    public string KieuDuLieu { get; set; } = "VanBan";

    /// <summary>
    /// Danh sách các tùy chọn phân cách bởi dấu phẩy nếu kiểu dữ liệu là DanhSach.
    /// </summary>
    [StringLength(500, ErrorMessage = "Tùy chọn danh sách không được vượt quá 500 ký tự.")]
    public string? TuyChonDanhSach { get; set; }

    /// <summary>
    /// Giá trị mặc định khi tạo mới bản ghi.
    /// </summary>
    [StringLength(255, ErrorMessage = "Giá trị mặc định không được vượt quá 255 ký tự.")]
    public string? GiaTriMacDinh { get; set; }

    /// <summary>
    /// Xác định trường này có bắt buộc nhập hay không.
    /// </summary>
    public bool LaBatBuoc { get; set; } = false;

    /// <summary>
    /// Thứ tự hiển thị của cột trên bảng dữ liệu.
    /// </summary>
    public int ThuTuHienThi { get; set; } = 1;

    /// <summary>
    /// Trạng thái kích hoạt của trường tùy chỉnh.
    /// </summary>
    public bool CoHoatDong { get; set; } = true;

    /// <summary>
    /// Thời điểm tạo trường tùy chỉnh.
    /// </summary>
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Danh sách các giá trị dữ liệu đã lưu của trường này.
    /// </summary>
    public virtual ICollection<GiaTriTruongTuyChinh> GiaTris { get; set; } = new List<GiaTriTruongTuyChinh>();
}
