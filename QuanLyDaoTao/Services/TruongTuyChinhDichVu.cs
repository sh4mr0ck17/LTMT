using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Đọc / ghi giá trị trường tùy chỉnh cho các trang nghiệp vụ (có kiểm tra quyền theo vùng).
/// Live CRUD Studio của Superadmin vẫn dùng <see cref="IQuanTriDuLieuDichVu"/>.
/// </summary>
public interface ITruongTuyChinhDichVu
{
    /// <summary>
    /// Danh sách định nghĩa trường tùy chỉnh đang hoạt động của bảng.
    /// </summary>
    Task<List<TruongTuyChinh>> LayDinhNghiaAsync(string tenBang, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bảng giá trị: khóa bản ghi → (mã trường → giá trị).
    /// </summary>
    Task<Dictionary<string, Dictionary<string, string>>> LayGiaTriAsync(string tenBang, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ghi giá trị một ô tùy chỉnh. Người dùng phải có quyền ghi vùng/khoa của bản ghi.
    /// </summary>
    Task LuuGiaTriAsync(string tenBang, string khoaBanGhi, string maTruong, string? giaTri,
        string vung, int? khoaId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Cài đặt <see cref="ITruongTuyChinhDichVu"/>.
/// </summary>
public class TruongTuyChinhDichVu : DichVuNghiepVuCoSo, ITruongTuyChinhDichVu
{
    /// <summary>
    /// Khởi tạo dịch vụ.
    /// </summary>
    public TruongTuyChinhDichVu(
        QuanLyDaoTaoDbContext coSoDuLieu,
        IPhanQuyenDichVu? phanQuyen = null,
        INhatKyDichVu? nhatKy = null,
        IBusThayDoiDuLieu? bus = null)
        : base(coSoDuLieu, phanQuyen, nhatKy, bus)
    {
    }

    /// <inheritdoc />
    public Task<List<TruongTuyChinh>> LayDinhNghiaAsync(string tenBang, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenBang);
        return _coSoDuLieu.TruongTuyChinhs.AsNoTracking()
            .Where(t => t.TenBang == tenBang && t.CoHoatDong)
            .OrderBy(t => t.ThuTuHienThi)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, Dictionary<string, string>>> LayGiaTriAsync(string tenBang, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenBang);
        var danhSach = await _coSoDuLieu.GiaTriTruongTuyChinhs.AsNoTracking()
            .Where(g => g.TenBang == tenBang)
            .Select(g => new { g.KhoaBanGhi, g.TruongTuyChinh!.MaTruong, g.GiaTri })
            .ToListAsync(cancellationToken);

        return danhSach
            .GroupBy(g => g.KhoaBanGhi)
            .ToDictionary(
                nhom => nhom.Key,
                nhom => nhom.GroupBy(g => g.MaTruong).ToDictionary(g => g.Key, g => g.First().GiaTri ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public async Task LuuGiaTriAsync(string tenBang, string khoaBanGhi, string maTruong, string? giaTri,
        string vung, int? khoaId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenBang);
        ArgumentException.ThrowIfNullOrWhiteSpace(khoaBanGhi);
        ArgumentException.ThrowIfNullOrWhiteSpace(maTruong);
        var nguoiDung = await DamBaoQuyenGhiAsync(vung, khoaId);

        var truong = await _coSoDuLieu.TruongTuyChinhs
            .FirstOrDefaultAsync(t => t.TenBang == tenBang && t.MaTruong == maTruong, cancellationToken)
            ?? throw new KeyNotFoundException($"Không tìm thấy trường tùy chỉnh '{maTruong}'.");
        var giaTriChuan = string.IsNullOrWhiteSpace(giaTri) ? null : giaTri.Trim();
        if (truong.LaBatBuoc && giaTriChuan is null)
        {
            throw new InvalidOperationException($"Trường \"{truong.TenHienThi}\" là bắt buộc.");
        }

        var banGhi = await _coSoDuLieu.GiaTriTruongTuyChinhs
            .FirstOrDefaultAsync(g => g.TruongTuyChinhId == truong.TruongTuyChinhId && g.KhoaBanGhi == khoaBanGhi, cancellationToken);
        var giaTriCu = banGhi?.GiaTri;
        if (banGhi is null)
        {
            banGhi = new GiaTriTruongTuyChinh
            {
                TruongTuyChinhId = truong.TruongTuyChinhId,
                TenBang = tenBang,
                KhoaBanGhi = khoaBanGhi
            };
            _coSoDuLieu.GiaTriTruongTuyChinhs.Add(banGhi);
        }

        banGhi.GiaTri = giaTriChuan;
        banGhi.NgayCapNhat = DateTime.UtcNow;
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await HoanTatThaoTacAsync(nguoiDung, "Sửa nhanh trường tùy chỉnh", tenBang, $"{khoaBanGhi}/{maTruong}",
            giaTriCu, giaTriChuan, cancellationToken);
    }
}
