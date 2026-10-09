using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ quản lý nhật ký hệ thống và kiểm toán dữ liệu.
/// </summary>
public class NhatKyDichVu : INhatKyDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    /// <summary>
    /// Khởi tạo dịch vụ nhật ký với DbContext.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    public NhatKyDichVu(QuanLyDaoTaoDbContext coSoDuLieu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
    }

    /// <summary>
    /// Ghi nhận một sự kiện thao tác vào nhật ký hệ thống.
    /// </summary>
    public async Task GhiNhatKyAsync(
        string nguoiThucHien,
        string hanhDong,
        string bangDuLieu,
        string? khoaBanGhi = null,
        string? giaTriCu = null,
        string? giaTriMoi = null,
        string ketQua = "Thành công",
        string? diaChiIp = null,
        CancellationToken cancellationToken = default)
    {
        var nhatKy = new NhatKyHeThong
        {
            NguoiThucHien = string.IsNullOrWhiteSpace(nguoiThucHien) ? "Khách vãng lai" : nguoiThucHien,
            ThoiDiem = DateTime.UtcNow,
            HanhDong = hanhDong,
            BangDuLieu = bangDuLieu,
            KhoaBanGhi = khoaBanGhi,
            GiaTriCu = giaTriCu,
            GiaTriMoi = giaTriMoi,
            KetQua = ketQua,
            DiaChiIp = diaChiIp
        };

        await _coSoDuLieu.NhatKyHeThongs.AddAsync(nhatKy, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy danh sách lịch sử nhật ký hệ thống có lọc và phân trang.
    /// </summary>
    public async Task<List<NhatKyHeThong>> LayDanhSachNhatKyAsync(
        string? tuKhoa = null,
        string? bangDuLieu = null,
        int soLuong = 100,
        CancellationToken cancellationToken = default)
    {
        var truyVan = _coSoDuLieu.NhatKyHeThongs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tuKhoaChuan = tuKhoa.Trim();
            truyVan = truyVan.Where(nk =>
                nk.NguoiThucHien.Contains(tuKhoaChuan) ||
                (nk.KhoaBanGhi != null && nk.KhoaBanGhi.Contains(tuKhoaChuan)) ||
                nk.HanhDong.Contains(tuKhoaChuan));
        }

        if (!string.IsNullOrWhiteSpace(bangDuLieu))
        {
            truyVan = truyVan.Where(nk => nk.BangDuLieu == bangDuLieu.Trim());
        }

        return await truyVan
            .OrderByDescending(nk => nk.ThoiDiem)
            .Take(soLuong > 0 ? soLuong : 100)
            .ToListAsync(cancellationToken);
    }
}
