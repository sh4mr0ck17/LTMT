using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể lưu trữ giá trị của một trường tùy chỉnh ứng với từng bản ghi cụ thể trong cơ sở dữ liệu.
/// </summary>
public class GiaTriTruongTuyChinh
{
    /// <summary>
    /// Khóa chính định danh nội bộ của giá trị.
    /// </summary>
    public int GiaTriId { get; set; }

    /// <summary>
    /// Khóa ngoại tham chiếu đến định nghĩa trường tùy chỉnh.
    /// </summary>
    public int TruongTuyChinhId { get; set; }

    /// <summary>
    /// Đối tượng định nghĩa trường tùy chỉnh.
    /// </summary>
    public virtual TruongTuyChinh? TruongTuyChinh { get; set; }

    /// <summary>
    /// Tên bảng dữ liệu chứa bản ghi (Khoa, BoMon, LopHoc, HocVien, NhanSu, MonHoc, PhanCongGiangDay, ChucVu, TrinhDoChuyenMon, TaiKhoan, NhatKyHeThong).
    /// </summary>
    [Required]
    [StringLength(50)]
    public string TenBang { get; set; } = string.Empty;

    /// <summary>
    /// Khóa chính của bản ghi trong bảng tương ứng (dạng chuỗi để hỗ trợ cả int ID và GUID của TaiKhoan).
    /// </summary>
    [Required]
    [StringLength(100)]
    public string KhoaBanGhi { get; set; } = string.Empty;

    /// <summary>
    /// Giá trị lưu trữ của trường tùy chỉnh cho bản ghi đó.
    /// </summary>
    [StringLength(2000)]
    public string? GiaTri { get; set; }

    /// <summary>
    /// Thời điểm cập nhật giá trị gần nhất.
    /// </summary>
    public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;
}
