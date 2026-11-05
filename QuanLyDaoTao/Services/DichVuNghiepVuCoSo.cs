using QuanLyDaoTao.Data;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp cơ sở cho dịch vụ nghiệp vụ: gom kiểm tra quyền, ghi nhật ký và phát thông báo thay đổi.
/// Các phụ thuộc phụ là tùy chọn; khi vắng (kiểm thử đơn vị) dịch vụ chạy với quyền hệ thống.
/// Trong ứng dụng thật DI luôn cung cấp đủ phụ thuộc.
/// </summary>
public abstract class DichVuNghiepVuCoSo
{
    /// <summary>
    /// Ngữ cảnh CSDL.
    /// </summary>
    protected readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    private readonly IPhanQuyenDichVu? _phanQuyen;
    private readonly INhatKyDichVu? _nhatKy;
    private readonly IBusThayDoiDuLieu? _bus;

    /// <summary>
    /// Khởi tạo lớp cơ sở.
    /// </summary>
    protected DichVuNghiepVuCoSo(
        QuanLyDaoTaoDbContext coSoDuLieu,
        IPhanQuyenDichVu? phanQuyen,
        INhatKyDichVu? nhatKy,
        IBusThayDoiDuLieu? bus)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
        _phanQuyen = phanQuyen;
        _nhatKy = nhatKy;
        _bus = bus;
    }

    /// <summary>
    /// Lấy người dùng hiện tại (quyền hệ thống nếu không có dịch vụ phân quyền).
    /// </summary>
    protected Task<NguoiDungHienTai> LayNguoiDungAsync() =>
        _phanQuyen?.LayNguoiDungAsync() ?? Task.FromResult(NguoiDungHienTai.HeThong);

    /// <summary>
    /// Bảo đảm quyền ghi vùng/khoa, ném <see cref="UnauthorizedAccessException"/> nếu không đủ quyền.
    /// </summary>
    protected Task<NguoiDungHienTai> DamBaoQuyenGhiAsync(string vung, int? khoaId) =>
        _phanQuyen?.DamBaoQuyenGhiAsync(vung, khoaId) ?? Task.FromResult(NguoiDungHienTai.HeThong);

    /// <summary>
    /// Bảo đảm người dùng là quản trị viên.
    /// </summary>
    protected Task<NguoiDungHienTai> DamBaoQuanTriAsync() =>
        _phanQuyen?.DamBaoQuanTriAsync() ?? Task.FromResult(NguoiDungHienTai.HeThong);

    /// <summary>
    /// Ghi nhật ký thao tác và phát thông báo thay đổi bảng.
    /// </summary>
    protected async Task HoanTatThaoTacAsync(
        NguoiDungHienTai nguoiDung,
        string hanhDong,
        string bang,
        string? khoaBanGhi,
        string? giaTriCu,
        string? giaTriMoi,
        CancellationToken cancellationToken,
        bool ghiNhatKy = true)
    {
        ArgumentNullException.ThrowIfNull(nguoiDung);
        if (ghiNhatKy && _nhatKy is not null)
        {
            await _nhatKy.GhiNhatKyAsync(nguoiDung.TenDangNhap, hanhDong, bang, khoaBanGhi, giaTriCu, giaTriMoi,
                cancellationToken: cancellationToken);
        }

        _bus?.PhatThayDoi(bang);
    }
}
