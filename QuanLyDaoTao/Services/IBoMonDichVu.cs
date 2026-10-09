using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Bộ môn trực thuộc Khoa.
/// </summary>
public interface IBoMonDichVu
{
    /// <summary>
    /// Lấy danh sách bộ môn có hỗ trợ lọc theo khoa và tìm kiếm.
    /// </summary>
    /// <param name="khoaId">Lọc theo khoa chủ quản.</param>
    /// <param name="tuKhoa">Từ khóa tìm kiếm theo mã hoặc tên bộ môn.</param>
    /// <param name="coHoatDong">Lọc theo trạng thái hoạt động.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Danh sách bộ môn.</returns>
    Task<List<BoMon>> LayDanhSachAsync(
        int? khoaId = null,
        string? tuKhoa = null,
        bool? coHoatDong = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin chi tiết một bộ môn theo ID.
    /// </summary>
    /// <param name="id">Mã định danh bộ môn.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Bộ môn hoặc null nếu không tìm thấy.</returns>
    Task<BoMon?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin bộ môn theo mã nghiệp vụ duy nhất.
    /// </summary>
    /// <param name="maBoMon">Mã bộ môn nghiệp vụ.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Bộ môn hoặc null nếu không tìm thấy.</returns>
    Task<BoMon?> LayTheoMaAsync(string maBoMon, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một bộ môn vào hệ thống sau khi kiểm tra hợp lệ.
    /// </summary>
    /// <param name="boMon">Đối tượng Bộ môn cần thêm mới.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Bộ môn đã được thêm thành công.</returns>
    Task<BoMon> ThemBoMonAsync(BoMon boMon, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin một bộ môn hiện có.
    /// </summary>
    /// <param name="boMon">Đối tượng Bộ môn chứa dữ liệu cập nhật.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Bộ môn sau khi cập nhật.</returns>
    Task<BoMon> CapNhatBoMonAsync(BoMon boMon, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa một bộ môn khỏi hệ thống nếu không còn nhân sự hoặc môn học trực thuộc.
    /// </summary>
    /// <param name="id">Mã định danh bộ môn cần xóa.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu xóa thành công.</returns>
    Task<bool> XoaBoMonAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem mã bộ môn đã tồn tại trong cơ sở dữ liệu hay chưa.
    /// </summary>
    /// <param name="maBoMon">Mã bộ môn cần kiểm tra.</param>
    /// <param name="boQuaId">Id của bộ môn cần bỏ qua (khi cập nhật).</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu mã đã tồn tại.</returns>
    Task<bool> KiemTraTonTaiMaAsync(string maBoMon, int? boQuaId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem bộ môn có thể xóa được hay không (không có nhân sự, môn học trực thuộc).
    /// </summary>
    /// <param name="id">Mã định danh bộ môn.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu có thể xóa an toàn.</returns>
    Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default);
}
