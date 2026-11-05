using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Hồ sơ cán bộ / giảng viên trong hệ thống đào tạo (Đã gộp tinh gọn).
/// </summary>
public class NhanSu
{
    /// <summary>
    /// Khóa chính định danh nội bộ.
    /// </summary>
    public int NhanSuId { get; set; }

    /// <summary>
    /// Mã cán bộ / giảng viên nghiệp vụ duy nhất.
    /// </summary>
    [Required(ErrorMessage = "Mã nhân sự là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã nhân sự không được vượt quá 50 ký tự.")]
    public string MaNhanSu { get; set; } = string.Empty;

    /// <summary>
    /// Họ và tên cán bộ đầy đủ.
    /// </summary>
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
    public string HoVaTen { get; set; } = string.Empty;

    /// <summary>
    /// Ngày tháng năm sinh.
    /// </summary>
    [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
    public DateTime NgaySinh { get; set; }

    /// <summary>
    /// Giới tính (Nam, Nữ, Khác).
    /// </summary>
    [Required(ErrorMessage = "Giới tính là bắt buộc.")]
    [StringLength(20, ErrorMessage = "Giới tính không được vượt quá 20 ký tự.")]
    public string GioiTinh { get; set; } = "Nam";

    /// <summary>
    /// Số định danh cá nhân / Thẻ CCCD (Gộp từ Số giấy tờ, Loại giấy tờ, Nơi cấp, Ngày cấp).
    /// </summary>
    [StringLength(50, ErrorMessage = "Số CCCD không được vượt quá 50 ký tự.")]
    public string? SoCCCD { get; set; }

    /// <summary>
    /// Số điện thoại liên lạc chính thức.
    /// </summary>
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    public string? SoDienThoai { get; set; }

    /// <summary>
    /// Địa chỉ Email liên hệ và công vụ (Bắt buộc & Duy nhất - Gộp từ Email công việc và Email cá nhân).
    /// </summary>
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ cư trú (Gộp từ Địa chỉ thường trú và Địa chỉ liên hệ).
    /// </summary>
    [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
    public string? DiaChi { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Khoa trực thuộc.
    /// </summary>
    public int? KhoaId { get; set; }

    /// <summary>
    /// Đối tượng Khoa chủ quản.
    /// </summary>
    public virtual Khoa? Khoa { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Bộ môn trực thuộc.
    /// </summary>
    public int? BoMonId { get; set; }

    /// <summary>
    /// Đối tượng Bộ môn trực thuộc.
    /// </summary>
    public virtual BoMon? BoMon { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Chức vụ công tác.
    /// </summary>
    public int? ChucVuId { get; set; }

    /// <summary>
    /// Đối tượng Chức vụ công tác.
    /// </summary>
    public virtual ChucVu? ChucVu { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Trình độ chuyên môn chuẩn.
    /// </summary>
    public int? TrinhDoChuyenMonId { get; set; }

    /// <summary>
    /// Đối tượng Trình độ chuyên môn.
    /// </summary>
    public virtual TrinhDoChuyenMon? TrinhDoChuyenMon { get; set; }

    /// <summary>
    /// Tóm tắt chuyên môn đào tạo (Gộp từ Chuyên ngành, Trường đào tạo, Học vị, Học hàm, Lĩnh vực nghiên cứu).
    /// </summary>
    [StringLength(255, ErrorMessage = "Chuyên môn đào tạo không được vượt quá 255 ký tự.")]
    public string? ChuyenMonDaoTao { get; set; }

    /// <summary>
    /// Cấp bậc hàm (Đại tá, Thượng tá, Trung tá...).
    /// </summary>
    [StringLength(50, ErrorMessage = "Cấp bậc hàm không được vượt quá 50 ký tự.")]
    public string? CapBacHam { get; set; }

    /// <summary>
    /// Học hàm (Giáo sư, Phó giáo sư). Tách khỏi cột chuyên môn để không ghi đè lẫn nhau.
    /// </summary>
    [StringLength(50, ErrorMessage = "Học hàm không được vượt quá 50 ký tự.")]
    public string? HocHam { get; set; }

    /// <summary>
    /// Quê quán (tỉnh/thành).
    /// </summary>
    [StringLength(100, ErrorMessage = "Quê quán không được vượt quá 100 ký tự.")]
    public string? QueQuan { get; set; }

    /// <summary>
    /// Ngày bắt đầu vào làm việc chính thức tại đơn vị.
    /// </summary>
    public DateTime? NgayVaoLam { get; set; }

    /// <summary>
    /// Trạng thái hồ sơ &amp; công tác (Đang làm việc, Tạm nghỉ, Đã nghỉ việc, Đã khóa - Gộp từ 3 cờ trạng thái).
    /// </summary>
    [StringLength(50, ErrorMessage = "Trạng thái không được vượt quá 50 ký tự.")]
    public string TrangThai { get; set; } = "Đang làm việc";

    /// <summary>
    /// Ghi chú đa năng (Gộp từ Kinh nghiệm công tác, Ghi chú chuyên môn, Người liên hệ khẩn cấp).
    /// </summary>
    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
    public string? GhiChu { get; set; }

    /// <summary>
    /// Đường dẫn ảnh đại diện tùy chọn.
    /// </summary>
    [StringLength(500)]
    public string? HinhAnh { get; set; }

    /// <summary>
    /// Thời điểm khởi tạo bản ghi.
    /// </summary>
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời điểm cập nhật bản ghi gần nhất.
    /// </summary>
    public DateTime? NgayCapNhat { get; set; }

    /// <summary>
    /// Người tạo bản ghi.
    /// </summary>
    [StringLength(100)]
    public string? NguoiTao { get; set; }

    /// <summary>
    /// Người cập nhật bản ghi.
    /// </summary>
    [StringLength(100)]
    public string? NguoiCapNhat { get; set; }

    /// <summary>
    /// Danh sách phân công giảng dạy của cán bộ.
    /// </summary>
    public virtual ICollection<PhanCongGiangDay> PhanCongGiangDays { get; set; } = new List<PhanCongGiangDay>();

    // =========================================================================
    // THUỘC TÍNH SUY DIỄN (không lưu riêng, tính từ TrangThai)
    // =========================================================================

    /// <summary>
    /// Trạng thái hoạt động suy diễn từ trạng thái tổng hợp.
    /// </summary>
    [NotMapped]
    public bool CoHoatDong
    {
        get => !string.Equals(TrangThai, "Đã khóa", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(TrangThai, "Đã nghỉ việc", StringComparison.OrdinalIgnoreCase);
        set => TrangThai = value ? (TrangThai == "Đã khóa" ? "Đang làm việc" : TrangThai) : "Đã khóa";
    }
}
