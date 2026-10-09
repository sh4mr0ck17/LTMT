using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Phân công giảng dạy giữa Nhân sự, Môn học và Lớp học.
/// </summary>
public class PhanCongGiangDay
{
    /// <summary>
    /// Khóa chính định danh nội bộ.
    /// </summary>
    public int PhanCongId { get; set; }

    /// <summary>
    /// Mã phân công nghiệp vụ duy nhất.
    /// </summary>
    [Required(ErrorMessage = "Mã phân công là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã phân công không được vượt quá 50 ký tự.")]
    public string MaPhanCong { get; set; } = string.Empty;

    /// <summary>
    /// Khóa ngoại liên kết tới Nhân sự / Giảng viên.
    /// </summary>
    [Required(ErrorMessage = "Nhân sự giảng dạy là bắt buộc.")]
    public int NhanSuId { get; set; }

    /// <summary>
    /// Đối tượng Nhân sự / Giảng viên được phân công.
    /// </summary>
    public virtual NhanSu? NhanSu { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Môn học đào tạo.
    /// </summary>
    [Required(ErrorMessage = "Môn học là bắt buộc.")]
    public int MonHocId { get; set; }

    /// <summary>
    /// Đối tượng Môn học được phân công.
    /// </summary>
    public virtual MonHoc? MonHoc { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Lớp học đào tạo.
    /// </summary>
    [Required(ErrorMessage = "Lớp học là bắt buộc.")]
    public int LopHocId { get; set; }

    /// <summary>
    /// Đối tượng Lớp học được phân công.
    /// </summary>
    public virtual LopHoc? LopHoc { get; set; }

    /// <summary>
    /// Học kỳ giảng dạy (ví dụ: Học kỳ 1, Học kỳ 2, Học kỳ hè).
    /// </summary>
    [Required(ErrorMessage = "Học kỳ là bắt buộc.")]
    [StringLength(20, ErrorMessage = "Học kỳ không được vượt quá 20 ký tự.")]
    public string HocKy { get; set; } = "Học kỳ 1";

    /// <summary>
    /// Năm học giảng dạy (ví dụ: 2026-2027).
    /// </summary>
    [Required(ErrorMessage = "Năm học là bắt buộc.")]
    [StringLength(20, ErrorMessage = "Năm học không được vượt quá 20 ký tự.")]
    public string NamHoc { get; set; } = "2026-2027";

    /// <summary>
    /// Vai trò giảng dạy (Giảng viên chính, Trợ giảng, Hướng dẫn thực hành).
    /// </summary>
    [StringLength(50, ErrorMessage = "Vai trò giảng dạy không được vượt quá 50 ký tự.")]
    public string VaiTroGiangDay { get; set; } = "Giảng viên chính";

    /// <summary>
    /// Xác định có phải là giáo viên chủ nhiệm lớp hay không.
    /// </summary>
    public bool LaChuNhiem { get; set; } = false;

    /// <summary>
    /// Ngày bắt đầu phân công.
    /// </summary>
    public DateTime? NgayBatDau { get; set; }

    /// <summary>
    /// Ngày kết thúc phân công.
    /// </summary>
    public DateTime? NgayKetThuc { get; set; }

    /// <summary>
    /// Ghi chú phân công giảng dạy.
    /// </summary>
    [StringLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự.")]
    public string? GhiChu { get; set; }

    /// <summary>
    /// Ngày tạo bản ghi phân công.
    /// </summary>
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
}
