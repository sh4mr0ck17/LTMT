using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Môn học trong chương trình đào tạo.
/// </summary>
public class MonHoc
{
    /// <summary>
    /// Khóa chính định danh nội bộ.
    /// </summary>
    public int MonHocId { get; set; }

    /// <summary>
    /// Mã môn học nghiệp vụ duy nhất.
    /// </summary>
    [Required(ErrorMessage = "Mã môn học là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã môn học không được vượt quá 50 ký tự.")]
    public string MaMonHoc { get; set; } = string.Empty;

    /// <summary>
    /// Tên môn học đầy đủ.
    /// </summary>
    [Required(ErrorMessage = "Tên môn học là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Tên môn học không được vượt quá 200 ký tự.")]
    public string TenMonHoc { get; set; } = string.Empty;

    /// <summary>
    /// Số tín chỉ của môn học.
    /// </summary>
    [Range(1, 20, ErrorMessage = "Số tín chỉ phải từ 1 đến 20.")]
    public int SoTinChi { get; set; } = 3;

    /// <summary>
    /// Số tiết lý thuyết.
    /// </summary>
    [Range(0, 500, ErrorMessage = "Số tiết lý thuyết phải từ 0 đến 500.")]
    public int SoTietLyThuyet { get; set; } = 30;

    /// <summary>
    /// Số tiết thực hành.
    /// </summary>
    [Range(0, 500, ErrorMessage = "Số tiết thực hành phải từ 0 đến 500.")]
    public int SoTietThucHanh { get; set; } = 15;

    /// <summary>
    /// Khóa ngoại liên kết tới Bộ môn phụ trách môn học.
    /// </summary>
    public int? BoMonId { get; set; }

    /// <summary>
    /// Đối tượng Bộ môn phụ trách môn học.
    /// </summary>
    public virtual BoMon? BoMon { get; set; }

    /// <summary>
    /// Trạng thái hoạt động của môn học.
    /// </summary>
    public bool CoHoatDong { get; set; } = true;

    /// <summary>
    /// Danh sách phân công giảng dạy của môn học.
    /// </summary>
    public virtual ICollection<PhanCongGiangDay> PhanCongGiangDays { get; set; } = new List<PhanCongGiangDay>();
}
