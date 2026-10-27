using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Phân công giảng dạy giữa Nhân sự, Môn học và Lớp học (Đã gộp tinh gọn).
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
    /// Học kỳ và Năm học giảng dạy (Gộp từ Học kỳ, Năm học, Ngày bắt đầu, Ngày kết thúc - VD: HK1 (2026-2027)).
    /// </summary>
    [Required(ErrorMessage = "Học kỳ và năm học là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Học kỳ năm học không được vượt quá 50 ký tự.")]
    public string HocKyNamHoc { get; set; } = "HK1 (2026-2027)";

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
    /// Ghi chú phân công giảng dạy (Phòng học, ca dạy, lưu ý).
    /// </summary>
    [StringLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự.")]
    public string? GhiChu { get; set; }

    /// <summary>
    /// Ngày tạo bản ghi phân công.
    /// </summary>
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    // =========================================================================
    // CÁC THUỘC TÍNH ÁNH XẠ CHUYỂN TIẾP (TƯƠNG THÍCH HOÀN TOÀN)
    // =========================================================================

    /// <summary>
    /// Thuộc tính tương thích Học kỳ.
    /// </summary>
    [NotMapped]
    public string HocKy
    {
        get => HocKyNamHoc.Contains('(') ? HocKyNamHoc.Split('(')[0].Trim() : HocKyNamHoc;
        set => HocKyNamHoc = string.IsNullOrWhiteSpace(value) ? HocKyNamHoc : $"{value} ({NamHoc})";
    }

    /// <summary>
    /// Thuộc tính tương thích Năm học.
    /// </summary>
    [NotMapped]
    public string NamHoc
    {
        get
        {
            if (HocKyNamHoc.Contains('(') && HocKyNamHoc.Contains(')'))
            {
                int start = HocKyNamHoc.IndexOf('(') + 1;
                int end = HocKyNamHoc.IndexOf(')');
                return HocKyNamHoc.Substring(start, end - start).Trim();
            }
            return "2026-2027";
        }
        set => HocKyNamHoc = string.IsNullOrWhiteSpace(value) ? HocKyNamHoc : $"{HocKy} ({value})";
    }

    /// <summary>
    /// Thuộc tính tương thích Ngày bắt đầu.
    /// </summary>
    [NotMapped]
    public DateTime? NgayBatDau { get; set; }

    /// <summary>
    /// Thuộc tính tương thích Ngày kết thúc.
    /// </summary>
    [NotMapped]
    public DateTime? NgayKetThuc { get; set; }
}
