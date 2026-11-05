using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Thông tin quyền của người dùng hiện tại, chụp tại thời điểm gọi.
/// </summary>
/// <param name="DaXacThuc">Đã đăng nhập hay chưa.</param>
/// <param name="TenDangNhap">Tên đăng nhập (email).</param>
/// <param name="VaiTro">Vai trò cao nhất của tài khoản.</param>
/// <param name="VungQuanLy">Vùng quản lý của Manager.</param>
/// <param name="KhoaPhuTrachId">Khoa phụ trách của Manager (null = mọi khoa trong vùng).</param>
public sealed record NguoiDungHienTai(
    bool DaXacThuc,
    string TenDangNhap,
    string VaiTro,
    string? VungQuanLy,
    int? KhoaPhuTrachId)
{
    /// <summary>
    /// Người dùng ẩn danh.
    /// </summary>
    public static readonly NguoiDungHienTai AnDanh = new(false, "Ẩn danh", string.Empty, null, null);

    /// <summary>
    /// Ngữ cảnh hệ thống (khởi tạo dữ liệu, kiểm thử) có toàn quyền.
    /// </summary>
    public static readonly NguoiDungHienTai HeThong = new(true, "Hệ thống", VaiTroMacDinh.SuperAdmin, null, null);

    /// <summary>
    /// Là Superadmin hoặc Admin.
    /// </summary>
    public bool LaQuanTri => VaiTro is VaiTroMacDinh.SuperAdmin or VaiTroMacDinh.QuanTriVien;

    /// <summary>
    /// Là Superadmin.
    /// </summary>
    public bool LaSuperAdmin => VaiTro == VaiTroMacDinh.SuperAdmin;

    /// <summary>
    /// Có quyền ghi trong vùng và khoa chỉ định.
    /// </summary>
    /// <param name="vung">Vùng dữ liệu cần ghi.</param>
    /// <param name="khoaId">Khoa của bản ghi; null nếu bản ghi không gắn khoa.</param>
    public bool CoQuyenGhi(string vung, int? khoaId)
    {
        if (!DaXacThuc)
        {
            return false;
        }

        if (LaQuanTri)
        {
            return true;
        }

        if (VaiTro != VaiTroMacDinh.QuanLy || VungQuanLy != vung)
        {
            return false;
        }

        return KhoaPhuTrachId is null || KhoaPhuTrachId == khoaId;
    }

    /// <summary>
    /// Được xem thông tin cá nhân (CCCD, SĐT, email, địa chỉ) của bản ghi trong vùng chỉ định.
    /// Quy tắc giống quyền ghi: chỉ người quản lý đúng vùng/khoa mới thấy dữ liệu nhạy cảm.
    /// </summary>
    public bool CoTheXemThongTinCaNhan(string vung, int? khoaId) => CoQuyenGhi(vung, khoaId);
}

/// <summary>
/// Dịch vụ phân quyền trung tâm: mọi thao tác ghi nghiệp vụ phải đi qua đây (không chỉ ẩn nút trên giao diện).
/// </summary>
public interface IPhanQuyenDichVu
{
    /// <summary>
    /// Lấy thông tin quyền của người dùng đang thao tác.
    /// </summary>
    Task<NguoiDungHienTai> LayNguoiDungAsync();

    /// <summary>
    /// Ném <see cref="UnauthorizedAccessException"/> nếu người dùng không có quyền ghi vùng/khoa.
    /// </summary>
    /// <returns>Thông tin người dùng để ghi nhật ký.</returns>
    Task<NguoiDungHienTai> DamBaoQuyenGhiAsync(string vung, int? khoaId);

    /// <summary>
    /// Ném <see cref="UnauthorizedAccessException"/> nếu người dùng không phải Superadmin/Admin.
    /// </summary>
    Task<NguoiDungHienTai> DamBaoQuanTriAsync();
}
