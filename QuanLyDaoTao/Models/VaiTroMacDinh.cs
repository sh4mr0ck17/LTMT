namespace QuanLyDaoTao.Models;

/// <summary>
/// Các hằng số định nghĩa vai trò người dùng trong hệ thống.
/// </summary>
public static class VaiTroMacDinh
{
    /// <summary>
    /// Vai trò Siêu quản trị viên (Superadmin) - độc quyền Live CRUD Database Studio và cấu hình hệ thống.
    /// </summary>
    public const string SuperAdmin = "Superadmin";

    /// <summary>
    /// Vai trò Quản trị viên hệ thống - toàn quyền cấu hình và quản trị.
    /// </summary>
    public const string QuanTriVien = "Admin";

    /// <summary>
    /// Vai trò Quản lý đào tạo / Dữ liệu - quyền thêm, sửa, xóa, import, export nghiệp vụ.
    /// </summary>
    public const string QuanLyDaoTao = "DataManager";

    /// <summary>
    /// Vai trò Người xem - chỉ có quyền xem danh sách và chi tiết báo cáo.
    /// </summary>
    public const string NguoiXem = "Viewer";

    /// <summary>
    /// Vai trò Người dùng thông thường - có quyền xem, sửa, nhập/xuất dữ liệu nghiệp vụ, không có quyền Live CRUD.
    /// </summary>
    public const string NguoiDung = "User";

    /// <summary>
    /// Vai trò Giáo viên / Giảng viên - quản lý lớp giảng dạy, xem lịch phân công và hồ sơ cá nhân.
    /// </summary>
    public const string GiaoVien = "GiaoVien";

    /// <summary>
    /// Vai trò Cán bộ quản lý khoa/phòng - quản lý nghiệp vụ đơn vị, nhập/xuất Excel.
    /// </summary>
    public const string CanBo = "CanBo";
}
