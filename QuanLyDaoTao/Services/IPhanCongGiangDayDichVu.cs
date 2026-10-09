using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Giao diện dịch vụ nghiệp vụ quản lý Phân công giảng dạy.
/// </summary>
public interface IPhanCongGiangDayDichVu
{
    /// <summary>
    /// Lấy danh sách phân công giảng dạy có lọc theo nhân sự, môn học, lớp học, học kỳ, năm học.
    /// </summary>
    Task<List<PhanCongGiangDay>> LayDanhSachAsync(
        int? nhanSuId = null,
        int? monHocId = null,
        int? lopHocId = null,
        string? hocKy = null,
        string? namHoc = null,
        string? tuKhoa = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin chi tiết một phân công theo ID.
    /// </summary>
    Task<PhanCongGiangDay?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin phân công theo mã phân công duy nhất.
    /// </summary>
    Task<PhanCongGiangDay?> LayTheoMaAsync(string maPhanCong, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một phân công giảng dạy.
    /// </summary>
    Task<PhanCongGiangDay> ThemPhanCongAsync(PhanCongGiangDay phanCong, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thông tin phân công giảng dạy.
    /// </summary>
    Task<PhanCongGiangDay> CapNhatPhanCongAsync(PhanCongGiangDay phanCong, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa một phân công giảng dạy khỏi hệ thống.
    /// </summary>
    Task<bool> XoaPhanCongAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra tồn tại mã phân công giảng dạy.
    /// </summary>
    Task<bool> KiemTraTonTaiMaAsync(string maPhanCong, int? boQuaId = null, CancellationToken cancellationToken = default);
}
