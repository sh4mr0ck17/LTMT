namespace QuanLyDaoTao.ViewModels.TaiKhoan;

/// <summary>
/// ViewModel hiển thị từng dòng trong danh sách quản trị tài khoản người dùng.
/// </summary>
public class ThongTinTaiKhoanItemViewModel
{
    /// <summary>
    /// Mã định danh người dùng Identity.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ email đăng nhập.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Họ và tên người dùng.
    /// </summary>
    public string HoVaTen { get; set; } = string.Empty;

    /// <summary>
    /// Mã nhân sự liên kết (nếu có).
    /// </summary>
    public string? MaNhanSuLienKet { get; set; }

    /// <summary>
    /// Danh sách các vai trò được phân quyền.
    /// </summary>
    public IList<string> DanhSachVaiTro { get; set; } = new List<string>();

    /// <summary>
    /// Trạng thái hoạt động của tài khoản.
    /// </summary>
    public bool CoHoatDong { get; set; }

    /// <summary>
    /// Thời điểm khởi tạo tài khoản.
    /// </summary>
    public DateTime NgayTao { get; set; }

    /// <summary>
    /// Vùng quản lý của Manager.
    /// </summary>
    public string? VungQuanLy { get; set; }

    /// <summary>
    /// Khoa phụ trách của Manager.
    /// </summary>
    public int? KhoaPhuTrachId { get; set; }

    /// <summary>
    /// Tên khoa phụ trách để hiển thị.
    /// </summary>
    public string? TenKhoaPhuTrach { get; set; }

    /// <summary>
    /// Lần đăng nhập gần nhất.
    /// </summary>
    public DateTime? LanDangNhapCuoi { get; set; }

    /// <summary>
    /// Tài khoản đang bị khóa tạm do đăng nhập sai nhiều lần.
    /// </summary>
    public bool DangBiKhoaTam { get; set; }
}

/// <summary>
/// ViewModel chính cho trang quản lý tài khoản người dùng.
/// </summary>
public class QuanLyTaiKhoanViewModel
{
    /// <summary>
    /// Danh sách các tài khoản người dùng.
    /// </summary>
    public List<ThongTinTaiKhoanItemViewModel> DanhSachTaiKhoan { get; set; } = new();

    /// <summary>
    /// Từ khóa tìm kiếm tài khoản.
    /// </summary>
    public string? TuKhoa { get; set; }

    /// <summary>
    /// Vai trò lọc theo.
    /// </summary>
    public string? LocTheoVaiTro { get; set; }

    /// <summary>
    /// Danh sách khoa để chọn khoa phụ trách.
    /// </summary>
    public List<QuanLyDaoTao.Models.Khoa> DanhSachKhoa { get; set; } = new();
}
