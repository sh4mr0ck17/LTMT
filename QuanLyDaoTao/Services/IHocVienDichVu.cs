using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Học viên đào tạo.
/// </summary>
public interface IHocVienDichVu
{
    /// <summary>
    /// Lấy danh sách học viên có lọc theo lớp học, trạng thái và tìm kiếm.
    /// </summary>
    /// <param name="lopHocId">Lọc theo lớp học chủ quản.</param>
    /// <param name="tuKhoa">Từ khóa tìm kiếm theo mã, họ tên, email, sđt.</param>
    /// <param name="coHoatDong">Lọc theo trạng thái hoạt động.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Danh sách học viên.</returns>
    Task<List<HocVien>> LayDanhSachAsync(
        int? lopHocId = null,
        string? tuKhoa = null,
        bool? coHoatDong = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin chi tiết một học viên theo ID.
    /// </summary>
    /// <param name="id">Mã định danh học viên.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Học viên hoặc null nếu không tìm thấy.</returns>
    Task<HocVien?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin học viên theo mã học viên duy nhất.
    /// </summary>
    /// <param name="maHocVien">Mã học viên nghiệp vụ.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Học viên hoặc null nếu không tìm thấy.</returns>
    Task<HocVien?> LayTheoMaAsync(string maHocVien, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một học viên vào hệ thống.
    /// </summary>
    /// <param name="hocVien">Đối tượng Học viên cần thêm mới.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Học viên sau khi lưu.</returns>
    Task<HocVien> ThemHocVienAsync(HocVien hocVien, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin một học viên hiện có.
    /// </summary>
    /// <param name="hocVien">Đối tượng Học viên chứa dữ liệu cập nhật.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Học viên sau khi cập nhật.</returns>
    Task<HocVien> CapNhatHocVienAsync(HocVien hocVien, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa một học viên khỏi hệ thống.
    /// </summary>
    /// <param name="id">Mã định danh học viên cần xóa.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu xóa thành công.</returns>
    Task<bool> XoaHocVienAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem mã học viên đã tồn tại trong hệ thống hay chưa.
    /// </summary>
    /// <param name="maHocVien">Mã học viên cần kiểm tra.</param>
    /// <param name="boQuaId">Id học viên cần bỏ qua (khi cập nhật).</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu mã học viên đã tồn tại.</returns>
    Task<bool> KiemTraTonTaiMaAsync(string maHocVien, int? boQuaId = null, CancellationToken cancellationToken = default);
}
