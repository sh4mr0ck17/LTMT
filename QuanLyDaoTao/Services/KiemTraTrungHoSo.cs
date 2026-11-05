using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Kiểm tra trùng trường định danh (CCCD, số điện thoại) trên cả học viên và cán bộ,
/// tránh một người bị nhập thành hai hồ sơ ở hai phân hệ.
/// </summary>
public static class KiemTraTrungHoSo
{
    /// <summary>
    /// Trả về thông báo lỗi nếu CCCD hoặc SĐT đã thuộc hồ sơ khác; null nếu không trùng.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh CSDL.</param>
    /// <param name="soCccd">Số CCCD cần kiểm tra (có thể rỗng).</param>
    /// <param name="soDienThoai">Số điện thoại cần kiểm tra (có thể rỗng).</param>
    /// <param name="boQuaHocVienId">Bỏ qua học viên đang sửa.</param>
    /// <param name="boQuaNhanSuId">Bỏ qua cán bộ đang sửa.</param>
    /// <param name="cancellationToken">Token hủy.</param>
    public static async Task<string?> TimLoiTrungAsync(
        QuanLyDaoTaoDbContext coSoDuLieu,
        string? soCccd,
        string? soDienThoai,
        int? boQuaHocVienId,
        int? boQuaNhanSuId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        var cccd = string.IsNullOrWhiteSpace(soCccd) ? null : soCccd.Trim();
        var sdt = string.IsNullOrWhiteSpace(soDienThoai) ? null : soDienThoai.Trim();
        if (cccd is null && sdt is null)
        {
            return null;
        }

        var hocVienTrung = await coSoDuLieu.HocViens.AsNoTracking()
            .Where(hv => hv.HocVienId != boQuaHocVienId &&
                         ((cccd != null && hv.SoCCCD == cccd) || (sdt != null && hv.SoDienThoai == sdt)))
            .Select(hv => new { hv.MaHocVien, hv.HoVaTen, hv.SoCCCD, hv.SoDienThoai })
            .FirstOrDefaultAsync(cancellationToken);
        if (hocVienTrung is not null)
        {
            var truong = hocVienTrung.SoCCCD == cccd && cccd != null ? "Số CCCD" : "Số điện thoại";
            return $"{truong} đã được dùng cho học viên {hocVienTrung.HoVaTen} ({hocVienTrung.MaHocVien}).";
        }

        var canBoTrung = await coSoDuLieu.NhanSus.AsNoTracking()
            .Where(ns => ns.NhanSuId != boQuaNhanSuId &&
                         ((cccd != null && ns.SoCCCD == cccd) || (sdt != null && ns.SoDienThoai == sdt)))
            .Select(ns => new { ns.MaNhanSu, ns.HoVaTen, ns.SoCCCD })
            .FirstOrDefaultAsync(cancellationToken);
        if (canBoTrung is not null)
        {
            var truong = canBoTrung.SoCCCD == cccd && cccd != null ? "Số CCCD" : "Số điện thoại";
            return $"{truong} đã được dùng cho cán bộ {canBoTrung.HoVaTen} ({canBoTrung.MaNhanSu}).";
        }

        return null;
    }
}
