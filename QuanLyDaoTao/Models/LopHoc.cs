using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Lớp học đào tạo thuộc Khoa.
/// </summary>
public class LopHoc
{
    /// <summary>
    /// Khóa chính định danh nội bộ.
    /// </summary>
    public int LopHocId { get; set; }

    /// <summary>
    /// Mã lớp nghiệp vụ duy nhất.
    /// </summary>
    [Required(ErrorMessage = "Mã lớp là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã lớp không được vượt quá 50 ký tự.")]
    public string MaLop { get; set; } = string.Empty;

    /// <summary>
    /// Tên lớp đầy đủ.
    /// </summary>
    [Required(ErrorMessage = "Tên lớp là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Tên lớp không được vượt quá 200 ký tự.")]
    public string TenLop { get; set; } = string.Empty;

    /// <summary>
    /// Niên khóa của lớp (ví dụ: 2023-2027).
    /// </summary>
    [Required(ErrorMessage = "Niên khóa là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Niên khóa không được vượt quá 50 ký tự.")]
    public string NienKhoa { get; set; } = string.Empty;

    /// <summary>
    /// Khóa ngoại liên kết tới Khoa quản lý lớp.
    /// </summary>
    [Required(ErrorMessage = "Khoa quản lý là bắt buộc.")]
    public int KhoaId { get; set; }

    /// <summary>
    /// Đối tượng Khoa quản lý.
    /// </summary>
    public virtual Khoa? Khoa { get; set; }

    /// <summary>
    /// Số lượng học viên tối đa của lớp.
    /// </summary>
    [Range(1, 500, ErrorMessage = "Số lượng tối đa phải từ 1 đến 500.")]
    public int SoLuongToiDa { get; set; } = 50;

    /// <summary>
    /// Trạng thái hoạt động của lớp.
    /// </summary>
    public bool CoHoatDong { get; set; } = true;

    /// <summary>
    /// Ngày tạo bản ghi.
    /// </summary>
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Danh sách học viên thuộc lớp.
    /// </summary>
    public virtual ICollection<HocVien> HocViens { get; set; } = new List<HocVien>();

    /// <summary>
    /// Danh sách phân công giảng dạy cho lớp.
    /// </summary>
    public virtual ICollection<PhanCongGiangDay> PhanCongGiangDays { get; set; } = new List<PhanCongGiangDay>();
}
