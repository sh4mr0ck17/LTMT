using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ nghiệp vụ quản lý Chức vụ công tác.
/// </summary>
public class ChucVuDichVu : IChucVuDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    /// <summary>
    /// Khởi tạo dịch vụ chức vụ với DbContext.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    public ChucVuDichVu(QuanLyDaoTaoDbContext coSoDuLieu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ chức vụ.
    /// </summary>
    public async Task<List<ChucVu>> LayDanhSachAsync(string? tuKhoa = null, bool? coHoatDong = null, CancellationToken cancellationToken = default)
    {
        var truyVan = _coSoDuLieu.ChucVus
            .Include(cv => cv.NhanSus)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tuKhoaChuan = tuKhoa.Trim();
            truyVan = truyVan.Where(cv => cv.MaChucVu.Contains(tuKhoaChuan) || cv.TenChucVu.Contains(tuKhoaChuan));
        }

        if (coHoatDong.HasValue)
        {
            truyVan = truyVan.Where(cv => cv.CoHoatDong == coHoatDong.Value);
        }

        return await truyVan.OrderBy(cv => cv.MaChucVu).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin chức vụ theo ID.
    /// </summary>
    public async Task<ChucVu?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _coSoDuLieu.ChucVus
            .Include(cv => cv.NhanSus)
            .FirstOrDefaultAsync(cv => cv.ChucVuId == id, cancellationToken);
    }

    /// <summary>
    /// Thêm mới chức vụ vào hệ thống.
    /// </summary>
    public async Task<ChucVu> ThemChucVuAsync(ChucVu chucVu, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chucVu);

        if (await KiemTraTonTaiMaAsync(chucVu.MaChucVu, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã chức vụ '{chucVu.MaChucVu}' đã tồn tại trong hệ thống.");
        }

        await _coSoDuLieu.ChucVus.AddAsync(chucVu, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return chucVu;
    }

    /// <summary>
    /// Cập nhật thông tin chức vụ.
    /// </summary>
    public async Task<ChucVu> CapNhatChucVuAsync(ChucVu chucVu, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chucVu);

        var cvTonTai = await _coSoDuLieu.ChucVus.FirstOrDefaultAsync(cv => cv.ChucVuId == chucVu.ChucVuId, cancellationToken);
        if (cvTonTai == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy chức vụ có ID {chucVu.ChucVuId}.");
        }

        if (await KiemTraTonTaiMaAsync(chucVu.MaChucVu, chucVu.ChucVuId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã chức vụ '{chucVu.MaChucVu}' đã tồn tại ở bản ghi khác.");
        }

        cvTonTai.MaChucVu = chucVu.MaChucVu;
        cvTonTai.TenChucVu = chucVu.TenChucVu;
        cvTonTai.MoTa = chucVu.MoTa;
        cvTonTai.CoHoatDong = chucVu.CoHoatDong;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return cvTonTai;
    }

    /// <summary>
    /// Xóa chức vụ nếu không còn nhân sự đảm nhiệm.
    /// </summary>
    public async Task<bool> XoaChucVuAsync(int id, CancellationToken cancellationToken = default)
    {
        var cv = await _coSoDuLieu.ChucVus.FirstOrDefaultAsync(c => c.ChucVuId == id, cancellationToken);
        if (cv == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy chức vụ có ID {id}.");
        }

        if (!await CoTheXoaAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Không thể xóa chức vụ vì đang có nhân sự giữ chức vụ này.");
        }

        _coSoDuLieu.ChucVus.Remove(cv);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Kiểm tra tồn tại mã chức vụ.
    /// </summary>
    public async Task<bool> KiemTraTonTaiMaAsync(string maChucVu, int? boQuaId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maChucVu))
        {
            return false;
        }

        var maChuan = maChucVu.Trim();
        var truyVan = _coSoDuLieu.ChucVus.Where(c => c.MaChucVu == maChuan);

        if (boQuaId.HasValue)
        {
            truyVan = truyVan.Where(c => c.ChucVuId != boQuaId.Value);
        }

        return await truyVan.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Kiểm tra xem chức vụ có thể xóa hay không.
    /// </summary>
    public async Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default)
    {
        return !await _coSoDuLieu.NhanSus.AnyAsync(ns => ns.ChucVuId == id, cancellationToken);
    }
}
