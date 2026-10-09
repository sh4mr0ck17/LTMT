using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Hồ sơ nhân sự / Cán bộ / Giảng viên trong hệ thống đào tạo.
/// </summary>
public class NhanSu
{
    // ==========================================
    // NHÓM A: ĐỊNH DANH NỘI BỘ
    // ==========================================

    /// <summary>
    /// Khóa chính định danh nội bộ.
    /// </summary>
    public int NhanSuId { get; set; }

    /// <summary>
    /// Mã nhân sự nghiệp vụ duy nhất.
    /// </summary>
    [Required(ErrorMessage = "Mã nhân sự là bắt buộc.")]
    [StringLength(50, ErrorMessage = "Mã nhân sự không được vượt quá 50 ký tự.")]
    public string MaNhanSu { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái hồ sơ nhân sự (Đang xử lý, Hoàn tất, Lưu trữ).
    /// </summary>
    [StringLength(50, ErrorMessage = "Trạng thái hồ sơ không được vượt quá 50 ký tự.")]
    public string TrangThaiHoSo { get; set; } = "Hoàn tất";

    /// <summary>
    /// Đường dẫn ảnh đại diện tùy chọn.
    /// </summary>
    [StringLength(500, ErrorMessage = "Đường dẫn ảnh không được vượt quá 500 ký tự.")]
    public string? HinhAnh { get; set; }

    // ==========================================
    // NHÓM B: THÔNG TIN CÁ NHÂN
    // ==========================================

    /// <summary>
    /// Họ và tên nhân sự.
    /// </summary>
    [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
    public string HoVaTen { get; set; } = string.Empty;

    /// <summary>
    /// Ngày sinh.
    /// </summary>
    [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
    public DateTime NgaySinh { get; set; }

    /// <summary>
    /// Giới tính.
    /// </summary>
    [Required(ErrorMessage = "Giới tính là bắt buộc.")]
    [StringLength(20, ErrorMessage = "Giới tính không được vượt quá 20 ký tự.")]
    public string GioiTinh { get; set; } = "Nam";

    /// <summary>
    /// Quốc tịch.
    /// </summary>
    [StringLength(50, ErrorMessage = "Quốc tịch không được vượt quá 50 ký tự.")]
    public string QuocTich { get; set; } = "Việt Nam";

    /// <summary>
    /// Dân tộc.
    /// </summary>
    [StringLength(50, ErrorMessage = "Dân tộc không được vượt quá 50 ký tự.")]
    public string? DanToc { get; set; } = "Kinh";

    /// <summary>
    /// Tình trạng hôn nhân.
    /// </summary>
    [StringLength(50, ErrorMessage = "Tình trạng hôn nhân không được vượt quá 50 ký tự.")]
    public string? TinhTrangHonNhan { get; set; }

    // ==========================================
    // NHÓM C: ĐỊNH DANH VÀ LIÊN HỆ
    // ==========================================

    /// <summary>
    /// Loại giấy tờ định danh (CCCD, Hộ chiếu).
    /// </summary>
    [StringLength(50, ErrorMessage = "Loại giấy tờ không được vượt quá 50 ký tự.")]
    public string LoaiGiayTo { get; set; } = "CCCD";

    /// <summary>
    /// Số CCCD hoặc Hộ chiếu.
    /// </summary>
    [StringLength(50, ErrorMessage = "Số giấy tờ không được vượt quá 50 ký tự.")]
    public string? SoGiayTo { get; set; }

    /// <summary>
    /// Ngày cấp giấy tờ định danh.
    /// </summary>
    public DateTime? NgayCap { get; set; }

    /// <summary>
    /// Nơi cấp giấy tờ định danh.
    /// </summary>
    [StringLength(100, ErrorMessage = "Nơi cấp không được vượt quá 100 ký tự.")]
    public string? NoiCap { get; set; }

    /// <summary>
    /// Số điện thoại liên lạc.
    /// </summary>
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    public string? SoDienThoai { get; set; }

    /// <summary>
    /// Email cơ quan / công việc (Bắt buộc).
    /// </summary>
    [Required(ErrorMessage = "Email công việc là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email công việc không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Email công việc không được vượt quá 100 ký tự.")]
    public string EmailCongViec { get; set; } = string.Empty;

    /// <summary>
    /// Email cá nhân tùy chọn.
    /// </summary>
    [EmailAddress(ErrorMessage = "Email cá nhân không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Email cá nhân không được vượt quá 100 ký tự.")]
    public string? EmailCaNhan { get; set; }

    /// <summary>
    /// Địa chỉ thường trú theo hộ khẩu.
    /// </summary>
    [StringLength(255, ErrorMessage = "Địa chỉ thường trú không được vượt quá 255 ký tự.")]
    public string? DiaChiThuongTru { get; set; }

    /// <summary>
    /// Địa chỉ liên hệ hiện tại.
    /// </summary>
    [StringLength(255, ErrorMessage = "Địa chỉ liên hệ không được vượt quá 255 ký tự.")]
    public string? DiaChiLienHe { get; set; }

    /// <summary>
    /// Họ tên người liên hệ khẩn cấp.
    /// </summary>
    [StringLength(100, ErrorMessage = "Người liên hệ khẩn cấp không được vượt quá 100 ký tự.")]
    public string? NguoiLienHeKhanCap { get; set; }

    /// <summary>
    /// Số điện thoại liên hệ khẩn cấp.
    /// </summary>
    [StringLength(20, ErrorMessage = "Số điện thoại khẩn cấp không được vượt quá 20 ký tự.")]
    public string? SoDienThoaiKhanCap { get; set; }

    // ==========================================
    // NHÓM D: THÔNG TIN CÔNG TÁC
    // ==========================================

    /// <summary>
    /// Khóa ngoại liên kết tới Khoa.
    /// </summary>
    public int? KhoaId { get; set; }

    /// <summary>
    /// Đối tượng Khoa chủ quản.
    /// </summary>
    public virtual Khoa? Khoa { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Bộ môn.
    /// </summary>
    public int? BoMonId { get; set; }

    /// <summary>
    /// Đối tượng Bộ môn trực thuộc.
    /// </summary>
    public virtual BoMon? BoMon { get; set; }

    /// <summary>
    /// Khóa ngoại liên kết tới Chức vụ.
    /// </summary>
    public int? ChucVuId { get; set; }

    /// <summary>
    /// Đối tượng Chức vụ công tác.
    /// </summary>
    public virtual ChucVu? ChucVu { get; set; }

    /// <summary>
    /// Chức danh nghề nghiệp (Giảng viên, Giảng viên chính, Chuyên viên).
    /// </summary>
    [StringLength(100, ErrorMessage = "Chức danh nghề nghiệp không được vượt quá 100 ký tự.")]
    public string? ChucDanhNgheNghiep { get; set; }

    /// <summary>
    /// Họ tên người quản lý trực tiếp.
    /// </summary>
    [StringLength(100, ErrorMessage = "Người quản lý trực tiếp không được vượt quá 100 ký tự.")]
    public string? NguoiQuanLyTrucTiep { get; set; }

    /// <summary>
    /// Ngày bắt đầu vào làm việc.
    /// </summary>
    public DateTime? NgayVaoLam { get; set; }

    /// <summary>
    /// Loại hợp đồng làm việc (Hợp đồng không xác định thời hạn, Có xác định thời hạn, Thử việc).
    /// </summary>
    [StringLength(50, ErrorMessage = "Loại hợp đồng không được vượt quá 50 ký tự.")]
    public string? LoaiHopDong { get; set; } = "Chính thức";

    /// <summary>
    /// Ngày bắt đầu hợp đồng hiện tại.
    /// </summary>
    public DateTime? NgayBatDauHopDong { get; set; }

    /// <summary>
    /// Ngày kết thúc hợp đồng hiện tại (nếu có).
    /// </summary>
    public DateTime? NgayKetThucHopDong { get; set; }

    /// <summary>
    /// Trạng thái công tác (Đang làm việc, Tạm nghỉ, Đã nghỉ việc, Nghỉ thai sản).
    /// </summary>
    [StringLength(50, ErrorMessage = "Trạng thái công tác không được vượt quá 50 ký tự.")]
    public string TrangThaiCongTac { get; set; } = "Đang làm việc";

    // ==========================================
    // NHÓM E: TRÌNH ĐỘ VÀ CHUYÊN MÔN
    // ==========================================

    /// <summary>
    /// Khóa ngoại liên kết tới Trình độ chuyên môn.
    /// </summary>
    public int? TrinhDoChuyenMonId { get; set; }

    /// <summary>
    /// Đối tượng Trình độ chuyên môn.
    /// </summary>
    public virtual TrinhDoChuyenMon? TrinhDoChuyenMon { get; set; }

    /// <summary>
    /// Trình độ học vấn cao nhất (Đại học, Thạc sĩ, Tiến sĩ).
    /// </summary>
    [StringLength(100, ErrorMessage = "Trình độ học vấn không được vượt quá 100 ký tự.")]
    public string? TrinhDoHocVan { get; set; }

    /// <summary>
    /// Chuyên ngành đào tạo.
    /// </summary>
    [StringLength(100, ErrorMessage = "Chuyên ngành không được vượt quá 100 ký tự.")]
    public string? ChuyenNganh { get; set; }

    /// <summary>
    /// Trường tốt nghiệp / cơ sở đào tạo.
    /// </summary>
    [StringLength(150, ErrorMessage = "Trường đào tạo không được vượt quá 150 ký tự.")]
    public string? TruongDaoTao { get; set; }

    /// <summary>
    /// Học vị (Cử nhân, Kỹ sư, Thạc sĩ, Tiến sĩ).
    /// </summary>
    [StringLength(50, ErrorMessage = "Học vị không được vượt quá 50 ký tự.")]
    public string? HocVi { get; set; }

    /// <summary>
    /// Học hàm (Giáo sư, Phó giáo sư).
    /// </summary>
    [StringLength(50, ErrorMessage = "Học hàm không được vượt quá 50 ký tự.")]
    public string? HocHam { get; set; }

    /// <summary>
    /// Lĩnh vực chuyên môn nghiên cứu và giảng dạy.
    /// </summary>
    [StringLength(200, ErrorMessage = "Lĩnh vực chuyên môn không được vượt quá 200 ký tự.")]
    public string? LinhVucChuyenMon { get; set; }

    /// <summary>
    /// Kinh nghiệm công tác.
    /// </summary>
    [StringLength(500, ErrorMessage = "Kinh nghiệm công tác không được vượt quá 500 ký tự.")]
    public string? KinhNghiemCongTac { get; set; }

    /// <summary>
    /// Ghi chú chuyên môn bổ sung.
    /// </summary>
    [StringLength(500, ErrorMessage = "Ghi chú chuyên môn không được vượt quá 500 ký tự.")]
    public string? GhiChuChuyenMon { get; set; }

    // ==========================================
    // NHÓM F: QUẢN TRỊ HỒ SƠ & FLAGS
    // ==========================================

    /// <summary>
    /// Trạng thái hoạt động trong hệ thống.
    /// </summary>
    public bool CoHoatDong { get; set; } = true;

    /// <summary>
    /// Thời điểm tạo bản ghi.
    /// </summary>
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Thời điểm cập nhật bản ghi gần nhất.
    /// </summary>
    public DateTime? NgayCapNhat { get; set; }

    /// <summary>
    /// Tài khoản hoặc người tạo bản ghi.
    /// </summary>
    [StringLength(100, ErrorMessage = "Người tạo không được vượt quá 100 ký tự.")]
    public string? NguoiTao { get; set; }

    /// <summary>
    /// Tài khoản hoặc người cập nhật bản ghi gần nhất.
    /// </summary>
    [StringLength(100, ErrorMessage = "Người cập nhật không được vượt quá 100 ký tự.")]
    public string? NguoiCapNhat { get; set; }

    /// <summary>
    /// Danh sách phân công giảng dạy của nhân sự.
    /// </summary>
    public virtual ICollection<PhanCongGiangDay> PhanCongGiangDays { get; set; } = new List<PhanCongGiangDay>();
}
