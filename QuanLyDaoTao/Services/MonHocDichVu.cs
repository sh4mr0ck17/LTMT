using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ nghiệp vụ quản lý Môn học đào tạo.
/// </summary>
public class MonHocDichVu : IMonHocDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    /// <summary>
    /// Khởi tạo dịch vụ môn học với DbContext.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    public MonHocDichVu(QuanLyDaoTaoDbContext coSoDuLieu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ môn học có lọc theo bộ môn và tìm kiếm.
    /// </summary>
    public async Task<List<MonHoc>> LayDanhSachAsync(int? boMonId = null, string? tuKhoa = null, bool? coHoatDong = null, CancellationToken cancellationToken = default)
    {
        var truyVan = _coSoDuLieu.MonHocs
            .Include(mh => mh.BoMon)
                .ThenInclude(bm => bm!.Khoa)
            .Include(mh => mh.PhanCongGiangDays)
            .AsNoTracking()
            .AsQueryable();

        if (boMonId.HasValue)
        {
            truyVan = truyVan.Where(mh => mh.BoMonId == boMonId.Value);
        }

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tuKhoaChuan = tuKhoa.Trim();
            truyVan = truyVan.Where(mh => mh.MaMonHoc.Contains(tuKhoaChuan) || mh.TenMonHoc.Contains(tuKhoaChuan));
        }

        if (coHoatDong.HasValue)
        {
            truyVan = truyVan.Where(mh => mh.CoHoatDong == coHoatDong.Value);
        }

        return await truyVan.OrderBy(mh => mh.MaMonHoc).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin môn học theo ID.
    /// </summary>
    public async Task<MonHoc?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _coSoDuLieu.MonHocs
            .Include(mh => mh.BoMon)
                .ThenInclude(bm => bm!.Khoa)
            .Include(mh => mh.PhanCongGiangDays)
            .FirstOrDefaultAsync(mh => mh.MonHocId == id, cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin môn học theo mã duy nhất.
    /// </summary>
    public async Task<MonHoc?> LayTheoMaAsync(string maMonHoc, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maMonHoc))
        {
            return null;
        }

        return await _coSoDuLieu.MonHocs
            .Include(mh => mh.BoMon)
            .Include(mh => mh.PhanCongGiangDays)
            .FirstOrDefaultAsync(mh => mh.MaMonHoc == maMonHoc.Trim(), cancellationToken);
    }

    /// <summary>
    /// Thêm mới môn học vào hệ thống.
    /// </summary>
    public async Task<MonHoc> ThemMonHocAsync(MonHoc monHoc, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(monHoc);

        if (monHoc.BoMonId.HasValue && !await _coSoDuLieu.BoMons.AnyAsync(bm => bm.BoMonId == monHoc.BoMonId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Bộ môn có ID {monHoc.BoMonId.Value} không tồn tại.");
        }

        if (await KiemTraTonTaiMaAsync(monHoc.MaMonHoc, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã môn học '{monHoc.MaMonHoc}' đã tồn tại trong hệ thống.");
        }

        await _coSoDuLieu.MonHocs.AddAsync(monHoc, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return monHoc;
    }

    /// <summary>
    /// Cập nhật thông tin môn học.
    /// </summary>
    public async Task<MonHoc> CapNhatMonHocAsync(MonHoc monHoc, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(monHoc);

        var mhTonTai = await _coSoDuLieu.MonHocs.FirstOrDefaultAsync(mh => mh.MonHocId == monHoc.MonHocId, cancellationToken);
        if (mhTonTai == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy môn học có ID {monHoc.MonHocId}.");
        }

        if (monHoc.BoMonId.HasValue && !await _coSoDuLieu.BoMons.AnyAsync(bm => bm.BoMonId == monHoc.BoMonId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Bộ môn có ID {monHoc.BoMonId.Value} không tồn tại.");
        }

        if (await KiemTraTonTaiMaAsync(monHoc.MaMonHoc, monHoc.MonHocId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã môn học '{monHoc.MaMonHoc}' đã tồn tại ở bản ghi khác.");
        }

        mhTonTai.MaMonHoc = monHoc.MaMonHoc;
        mhTonTai.TenMonHoc = monHoc.TenMonHoc;
        mhTonTai.SoTinChi = monHoc.SoTinChi;
        mhTonTai.SoTietLyThuyet = monHoc.SoTietLyThuyet;
        mhTonTai.SoTietThucHanh = monHoc.SoTietThucHanh;
        mhTonTai.BoMonId = monHoc.BoMonId;
        mhTonTai.CoHoatDong = monHoc.CoHoatDong;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return mhTonTai;
    }

    /// <summary>
    /// Xóa môn học nếu không còn phân công giảng dạy.
    /// </summary>
    public async Task<bool> XoaMonHocAsync(int id, CancellationToken cancellationToken = default)
    {
        var mh = await _coSoDuLieu.MonHocs.FirstOrDefaultAsync(m => m.MonHocId == id, cancellationToken);
        if (mh == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy môn học có ID {id}.");
        }

        if (!await CoTheXoaAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Không thể xóa môn học vì đang có phân công giảng dạy liên kết.");
        }

        _coSoDuLieu.MonHocs.Remove(mh);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Kiểm tra tồn tại mã môn học.
    /// </summary>
    public async Task<bool> KiemTraTonTaiMaAsync(string maMonHoc, int? boQuaId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maMonHoc))
        {
            return false;
        }

        var maChuan = maMonHoc.Trim();
        var truyVan = _coSoDuLieu.MonHocs.Where(m => m.MaMonHoc == maChuan);

        if (boQuaId.HasValue)
        {
            truyVan = truyVan.Where(m => m.MonHocId != boQuaId.Value);
        }

        return await truyVan.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Kiểm tra xem môn học có thể xóa hay không.
    /// </summary>
    public async Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default)
    {
        return !await _coSoDuLieu.PhanCongGiangDays.AnyAsync(pc => pc.MonHocId == id, cancellationToken);
    }
}
