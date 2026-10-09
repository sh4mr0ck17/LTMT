namespace QuanLyDaoTao.Models;

/// <summary>
/// Các hằng số định nghĩa vai trò người dùng trong hệ thống.
/// </summary>
public static class VaiTroMacDinh
{
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
}
