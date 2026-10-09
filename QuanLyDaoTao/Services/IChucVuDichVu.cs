using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Chức vụ công tác.
/// </summary>
public interface IChucVuDichVu
{
    /// <summary>
    /// Lấy danh sách toàn bộ chức vụ.
    /// </summary>
    Task<List<ChucVu>> LayDanhSachAsync(string? tuKhoa = null, bool? coHoatDong = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin chức vụ theo ID.
    /// </summary>
    Task<ChucVu?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới chức vụ vào hệ thống.
    /// </summary>
    Task<ChucVu> ThemChucVuAsync(ChucVu chucVu, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin chức vụ.
    /// </summary>
    Task<ChucVu> CapNhatChucVuAsync(ChucVu chucVu, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa chức vụ nếu không còn nhân sự đảm nhiệm.
    /// </summary>
    Task<bool> XoaChucVuAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra tồn tại mã chức vụ.
    /// </summary>
    Task<bool> KiemTraTonTaiMaAsync(string maChucVu, int? boQuaId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem chức vụ có thể xóa hay không.
    /// </summary>
    Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default);
}
