using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Bộ môn trực thuộc Khoa.
/// </summary>
public class BoMon
{
    /// <summary>
    /// Khóa chính định danh nội bộ.
    /// </summary>
    public int BoMonId { get; set; }

    /// <summary>
    /// Mã bộ môn nghiệp vụ duy nhất.
    /// </summary>
    [Required(ErrorMessage = "Mã bộ môn là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã bộ môn không được vượt quá 50 ký tự.")]
    public string MaBoMon { get; set; } = string.Empty;

    /// <summary>
    /// Tên bộ môn đầy đủ.
    /// </summary>
    [Required(ErrorMessage = "Tên bộ môn là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Tên bộ môn không được vượt quá 200 ký tự.")]
    public string TenBoMon { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả chi tiết về bộ môn.
    /// </summary>
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? MoTa { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Khoa chủ quản.
    /// </summary>
    [Required(ErrorMessage = "Khoa chủ quản là bắt buộc.")]
    public int KhoaId { get; set; }

    /// <summary>
    /// Đối tượng Khoa chủ quản.
    /// </summary>
    public virtual Khoa? Khoa { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của bộ môn.
    /// </summary>
    public bool CoHoatDong { get; set; } = true;

    /// <summary>
    /// Ngày tạo bản ghi.
    /// </summary>
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Danh sách nhân sự thuộc bộ môn.
    /// </summary>
    public virtual ICollection<NhanSu> NhanSus { get; set; } = new List<NhanSu>();

    /// <summary>
    /// Danh sách môn học phụ trách bởi bộ môn.
    /// </summary>
    public virtual ICollection<MonHoc> MonHocs { get; set; } = new List<MonHoc>();
}
