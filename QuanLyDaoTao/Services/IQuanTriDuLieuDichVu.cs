using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.QuanTri;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Giao diện dịch vụ Quản trị dữ liệu trực tuyến (Live Manual CRUD &amp; Custom Schema) cho toàn bộ các nhóm trong hệ thống.
/// </summary>
public interface IQuanTriDuLieuDichVu
{
    /// <summary>
    /// Lấy danh sách toàn bộ các bảng / nhóm quản lý trong hệ thống kèm số lượng bản ghi và số trường tùy chỉnh.
    /// </summary>
    Task<List<ThongTinBangHeThong>> LayDanhSachBangHeThongAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy cấu trúc cột (cột gốc + cột tùy chỉnh) và toàn bộ dữ liệu dòng của một bảng để hiển thị/chỉnh sửa trực tiếp.
    /// </summary>
    Task<DuLieuBangTrucTuyen> LayDuLieuBangTrucTuyenAsync(
        string tenBang,
        string? tuKhoa = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật trực tiếp các trường gốc và trường tùy chỉnh của một bản ghi vào cơ sở dữ liệu ngay lập tức.
    /// </summary>
    Task<bool> CapNhatNhanhBanGhiAsync(
        string tenBang,
        string khoaBanGhi,
        Dictionary<string, string?> duLieuCapNhat,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới nhanh một bản ghi trực tiếp vào bảng bất kỳ kèm các giá trị trường tùy chỉnh.
    /// </summary>
    Task<string> ThemNhanhBanGhiAsync(
        string tenBang,
        Dictionary<string, string?> duLieuMoi,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa trực tiếp một bản ghi khỏi bảng (có kiểm tra ràng buộc toàn vẹn dữ liệu).
    /// </summary>
    Task<bool> XoaNhanhBanGhiAsync(
        string tenBang,
        string khoaBanGhi,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách các trường tùy chỉnh của một bảng cụ thể.
    /// </summary>
    Task<List<TruongTuyChinh>> LayDanhSachTruongTuyChinhAsync(
        string tenBang,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một trường (cột) tùy chỉnh cho bảng được chỉ định.
    /// </summary>
    Task<TruongTuyChinh> ThemTruongTuyChinhAsync(
        TruongTuyChinh truongMoi,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật cấu hình một trường (cột) tùy chỉnh.
    /// </summary>
    Task<TruongTuyChinh> CapNhatTruongTuyChinhAsync(
        TruongTuyChinh truongCapNhat,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa một trường (cột) tùy chỉnh và các giá trị đi kèm của trường đó.
    /// </summary>
    Task<bool> XoaTruongTuyChinhAsync(
        int truongTuyChinhId,
        string? nguoiThucHien = null,
        CancellationToken cancellationToken = default);
}
