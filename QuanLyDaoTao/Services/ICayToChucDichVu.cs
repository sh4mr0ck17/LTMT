using QuanLyDaoTao.Models.ToChuc;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Hai sơ đồ độc lập phục vụ hai vùng quản lý:
/// Tổ chức (Học viện → Khoa → Bộ môn → Cán bộ) và Đào tạo (Học viện → Hệ → Khóa → Lớp → Học viên).
/// Cán bộ / học viên là lá dữ liệu, không vẽ thành thẻ; canvas chỉ thể hiện cấu trúc.
/// </summary>
public interface ICayToChucDichVu
{
    /// <summary>Cây tổ chức: Khoa → Bộ môn → Cán bộ (kể cả cán bộ chưa phân đơn vị).</summary>
    Task<NutToChuc> LayCayAsync(CancellationToken cancellationToken = default);

    /// <summary>Cây đào tạo: Hệ → Khóa → Lớp → Học viên (lớp ghi rõ khoa quản lý).</summary>
    Task<NutToChuc> LayCayDaoTaoAsync(CancellationToken cancellationToken = default);
}
