using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Khoa đào tạo.
/// </summary>
public interface IKhoaDichVu
{
    /// <summary>
    /// Lấy danh sách toàn bộ các khoa có hỗ trợ tìm kiếm và lọc.
    /// </summary>
    /// <param name="tuKhoa">Từ khóa tìm kiếm theo mã hoặc tên khoa.</param>
    /// <param name="coHoatDong">Lọc theo trạng thái hoạt động.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Danh sách các khoa.</returns>
    Task<List<Khoa>> LayDanhSachAsync(
        string? tuKhoa = null,
        bool? coHoatDong = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin chi tiết một khoa theo khóa chính Id.
    /// </summary>
    /// <param name="id">Mã định danh khoa.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Khoa hoặc null nếu không tìm thấy.</returns>
    Task<Khoa?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin khoa theo mã nghiệp vụ duy nhất.
    /// </summary>
    /// <param name="maKhoa">Mã khoa nghiệp vụ.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Khoa hoặc null nếu không tìm thấy.</returns>
    Task<Khoa?> LayTheoMaAsync(string maKhoa, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một khoa vào hệ thống sau khi kiểm tra hợp lệ.
    /// </summary>
    /// <param name="khoa">Đối tượng Khoa cần thêm mới.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Khoa đã được thêm thành công.</returns>
    Task<Khoa> ThemKhoaAsync(Khoa khoa, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin một khoa hiện có trong hệ thống.
    /// </summary>
    /// <param name="khoa">Đối tượng Khoa chứa dữ liệu cập nhật.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Khoa sau khi cập nhật.</returns>
    Task<Khoa> CapNhatKhoaAsync(Khoa khoa, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa một khoa khỏi hệ thống nếu không còn dữ liệu phụ thuộc liên quan.
    /// </summary>
    /// <param name="id">Mã định danh khoa cần xóa.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu xóa thành công.</returns>
    Task<bool> XoaKhoaAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem mã khoa đã tồn tại trong cơ sở dữ liệu hay chưa.
    /// </summary>
    /// <param name="maKhoa">Mã khoa cần kiểm tra.</param>
    /// <param name="boQuaId">Id của khoa cần bỏ qua (dùng khi cập nhật).</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu mã khoa đã tồn tại.</returns>
    Task<bool> KiemTraTonTaiMaAsync(string maKhoa, int? boQuaId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem khoa có thể xóa được hay không (không có bộ môn, lớp học, nhân sự trực thuộc).
    /// </summary>
    /// <param name="id">Mã định danh khoa.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu có thể xóa an toàn.</returns>
    Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default);
}
