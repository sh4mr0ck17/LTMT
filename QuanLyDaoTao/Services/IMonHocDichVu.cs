using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Môn học đào tạo.
/// </summary>
public interface IMonHocDichVu
{
    /// <summary>
    /// Lấy danh sách toàn bộ môn học có lọc theo bộ môn và tìm kiếm.
    /// </summary>
    Task<List<MonHoc>> LayDanhSachAsync(int? boMonId = null, string? tuKhoa = null, bool? coHoatDong = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin môn học theo ID.
    /// </summary>
    Task<MonHoc?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin môn học theo mã duy nhất.
    /// </summary>
    Task<MonHoc?> LayTheoMaAsync(string maMonHoc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới môn học vào hệ thống.
    /// </summary>
    Task<MonHoc> ThemMonHocAsync(MonHoc monHoc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin môn học.
    /// </summary>
    Task<MonHoc> CapNhatMonHocAsync(MonHoc monHoc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa môn học nếu không còn phân công giảng dạy.
    /// </summary>
    Task<bool> XoaMonHocAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra tồn tại mã môn học.
    /// </summary>
    Task<bool> KiemTraTonTaiMaAsync(string maMonHoc, int? boQuaId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem môn học có thể xóa hay không.
    /// </summary>
    Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default);
}
