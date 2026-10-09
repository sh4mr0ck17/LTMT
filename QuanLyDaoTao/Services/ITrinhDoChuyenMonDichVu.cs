using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Trình độ chuyên môn.
/// </summary>
public interface ITrinhDoChuyenMonDichVu
{
    /// <summary>
    /// Lấy danh sách toàn bộ trình độ chuyên môn.
    /// </summary>
    Task<List<TrinhDoChuyenMon>> LayDanhSachAsync(string? tuKhoa = null, bool? coHoatDong = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin trình độ theo ID.
    /// </summary>
    Task<TrinhDoChuyenMon?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới trình độ chuyên môn.
    /// </summary>
    Task<TrinhDoChuyenMon> ThemTrinhDoAsync(TrinhDoChuyenMon trinhDo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin trình độ chuyên môn.
    /// </summary>
    Task<TrinhDoChuyenMon> CapNhatTrinhDoAsync(TrinhDoChuyenMon trinhDo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa trình độ nếu không còn nhân sự liên kết.
    /// </summary>
    Task<bool> XoaTrinhDoAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra tồn tại mã trình độ.
    /// </summary>
    Task<bool> KiemTraTonTaiMaAsync(string maTrinhDo, int? boQuaId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra xem trình độ có thể xóa hay không.
    /// </summary>
    Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default);
}
