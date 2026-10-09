using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Lớp học đào tạo.
/// </summary>
public interface ILopHocDichVu
{
    /// <summary>
    /// Lấy danh sách toàn bộ lớp học có hỗ trợ lọc theo khoa, niên khóa, trạng thái và tìm kiếm.
    /// </summary>
    /// <param name="khoaId">Lọc theo khoa quản lý.</param>
    /// <param name="nienKhoa">Lọc theo niên khóa đào tạo.</param>
    /// <param name="tuKhoa">Từ khóa tìm kiếm theo mã hoặc tên lớp.</param>
    /// <param name="coHoatDong">Lọc theo trạng thái hoạt động.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Danh sách lớp học.</returns>
    Task<List<LopHoc>> LayDanhSachAsync(
        int? khoaId = null,
        string? nienKhoa = null,
        string? tuKhoa = null,
        bool? coHoatDong = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin chi tiết một lớp học theo ID.
    /// </summary>
    /// <param name="id">Mã định danh lớp học.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Lớp học hoặc null nếu không tìm thấy.</returns>
    Task<LopHoc?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin lớp học theo mã nghiệp vụ duy nhất.
    /// </summary>
    /// <param name="maLop">Mã lớp nghiệp vụ.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Lớp học hoặc null nếu không tìm thấy.</returns>
    Task<LopHoc?> LayTheoMaAsync(string maLop, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một lớp học vào hệ thống sau khi kiểm tra hợp lệ.
    /// </summary>
    /// <param name="lopHoc">Đối tượng Lớp học cần thêm mới.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Lớp học đã được thêm thành công.</returns>
    Task<LopHoc> ThemLopHocAsync(LopHoc lopHoc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin một lớp học hiện có.
    /// </summary>
    /// <param name="lopHoc">Đối tượng Lớp học chứa dữ liệu cập nhật.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Đối tượng Lớp học sau khi cập nhật.</returns>
    Task<LopHoc> CapNhatLopHocAsync(LopHoc lopHoc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa một lớp học khỏi hệ thống nếu không còn học viên hoặc phân công giảng dạy.
    /// </summary>
    /// <param name="id">Mã định danh lớp học cần xóa.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu xóa thành công.</returns>
    Task<bool> XoaLopHocAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem mã lớp đã tồn tại trong cơ sở dữ liệu hay chưa.
    /// </summary>
    /// <param name="maLop">Mã lớp cần kiểm tra.</param>
    /// <param name="boQuaId">Id của lớp cần bỏ qua (khi cập nhật).</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu mã lớp đã tồn tại.</returns>
    Task<bool> KiemTraTonTaiMaAsync(string maLop, int? boQuaId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem lớp học có thể xóa được hay không (không có học viên hoặc phân công giảng dạy).
    /// </summary>
    /// <param name="id">Mã định danh lớp học.</param>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>True nếu có thể xóa an toàn.</returns>
    Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các niên khóa hiện có trong hệ thống để phục vụ bộ lọc.
    /// </summary>
    /// <param name="cancellationToken">Mã token hủy tác vụ.</param>
    /// <returns>Danh sách chuỗi các niên khóa phân biệt.</returns>
    Task<List<string>> LayDanhSachNienKhoaAsync(CancellationToken cancellationToken = default);
}
