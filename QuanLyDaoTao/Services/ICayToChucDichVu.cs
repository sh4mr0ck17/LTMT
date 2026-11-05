using QuanLyDaoTao.Models.ToChuc;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Hai sơ đồ độc lập phục vụ hai vùng quản lý:
/// Tổ chức (Học viện → Khoa → Bộ môn → Cán bộ) và Đào tạo (Học viện → Hệ → Khóa → Lớp → Học viên).
/// Cán bộ / học viên là lá dữ liệu, không vẽ thành thẻ; canvas chỉ thể hiện cấu trúc.
/// </summary>
public interface ICayToChucDichVu
{
    /// <summary>Cây tổ chức: Khoa → Bộ môn → Cán bộ. Hỗ trợ tham số lọc khoaId, boMonId hoặc chỉ lấy cấp đơn vị để tăng tốc độ truy xuất.</summary>
    Task<NutToChuc> LayCayAsync(int? khoaId = null, int? boMonId = null, bool chiLayDonVi = false, CancellationToken cancellationToken = default);

    /// <summary>Cây đào tạo: Hệ → Khóa → Lớp → Học viên. Hỗ trợ tham số lọc theo hệ, khóa, lớpId hoặc chỉ lấy cấp lớp để tăng tốc độ truy xuất.</summary>
    Task<NutToChuc> LayCayDaoTaoAsync(string? he = null, string? khoaHoc = null, int? lopId = null, bool chiLayLop = false, CancellationToken cancellationToken = default);
}
