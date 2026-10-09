using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ nghiệp vụ quản lý Lớp học đào tạo.
/// </summary>
public class LopHocDichVu : ILopHocDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    /// <summary>
    /// Khởi tạo dịch vụ lớp học với DbContext.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    public LopHocDichVu(QuanLyDaoTaoDbContext coSoDuLieu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ lớp học có hỗ trợ lọc theo khoa, niên khóa, trạng thái và tìm kiếm.
    /// </summary>
    public async Task<List<LopHoc>> LayDanhSachAsync(
        int? khoaId = null,
        string? nienKhoa = null,
        string? tuKhoa = null,
        bool? coHoatDong = null,
        CancellationToken cancellationToken = default)
    {
        var truyVan = _coSoDuLieu.LopHocs
            .Include(l => l.Khoa)
            .Include(l => l.HocViens)
            .Include(l => l.PhanCongGiangDays)
            .AsNoTracking()
            .AsQueryable();

        if (khoaId.HasValue)
        {
            truyVan = truyVan.Where(l => l.KhoaId == khoaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(nienKhoa))
        {
            var nienKhoaChuan = nienKhoa.Trim();
            truyVan = truyVan.Where(l => l.NienKhoa == nienKhoaChuan);
        }

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tuKhoaChuan = tuKhoa.Trim();
            truyVan = truyVan.Where(l =>
                l.MaLop.Contains(tuKhoaChuan) ||
                l.TenLop.Contains(tuKhoaChuan) ||
                l.NienKhoa.Contains(tuKhoaChuan));
        }

        if (coHoatDong.HasValue)
        {
            truyVan = truyVan.Where(l => l.CoHoatDong == coHoatDong.Value);
        }

        return await truyVan.OrderBy(l => l.MaLop).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một lớp học theo ID.
    /// </summary>
    public async Task<LopHoc?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _coSoDuLieu.LopHocs
            .Include(l => l.Khoa)
            .Include(l => l.HocViens)
            .Include(l => l.PhanCongGiangDays)
            .FirstOrDefaultAsync(l => l.LopHocId == id, cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin lớp học theo mã nghiệp vụ duy nhất.
    /// </summary>
    public async Task<LopHoc?> LayTheoMaAsync(string maLop, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maLop))
        {
            return null;
        }

        return await _coSoDuLieu.LopHocs
            .Include(l => l.Khoa)
            .Include(l => l.HocViens)
            .Include(l => l.PhanCongGiangDays)
            .FirstOrDefaultAsync(l => l.MaLop == maLop.Trim(), cancellationToken);
    }

    /// <summary>
    /// Thêm mới một lớp học vào hệ thống sau khi kiểm tra hợp lệ.
    /// </summary>
    public async Task<LopHoc> ThemLopHocAsync(LopHoc lopHoc, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lopHoc);

        var khoaTonTai = await _coSoDuLieu.Khoas.AnyAsync(k => k.KhoaId == lopHoc.KhoaId, cancellationToken);
        if (!khoaTonTai)
        {
            throw new InvalidOperationException($"Khoa quản lý có ID {lopHoc.KhoaId} không tồn tại.");
        }

        if (await KiemTraTonTaiMaAsync(lopHoc.MaLop, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã lớp '{lopHoc.MaLop}' đã tồn tại trong hệ thống.");
        }

        lopHoc.NgayTao = DateTime.UtcNow;
        await _coSoDuLieu.LopHocs.AddAsync(lopHoc, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return lopHoc;
    }

    /// <summary>
    /// Cập nhật thông tin một lớp học hiện có.
    /// </summary>
    public async Task<LopHoc> CapNhatLopHocAsync(LopHoc lopHoc, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lopHoc);

        var lopTonTai = await _coSoDuLieu.LopHocs.FirstOrDefaultAsync(l => l.LopHocId == lopHoc.LopHocId, cancellationToken);
        if (lopTonTai == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học có ID {lopHoc.LopHocId}.");
        }

        var khoaTonTai = await _coSoDuLieu.Khoas.AnyAsync(k => k.KhoaId == lopHoc.KhoaId, cancellationToken);
        if (!khoaTonTai)
        {
            throw new InvalidOperationException($"Khoa quản lý có ID {lopHoc.KhoaId} không tồn tại.");
        }

        if (await KiemTraTonTaiMaAsync(lopHoc.MaLop, lopHoc.LopHocId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã lớp '{lopHoc.MaLop}' đã tồn tại ở bản ghi khác.");
        }

        lopTonTai.MaLop = lopHoc.MaLop;
        lopTonTai.TenLop = lopHoc.TenLop;
        lopTonTai.NienKhoa = lopHoc.NienKhoa;
        lopTonTai.KhoaId = lopHoc.KhoaId;
        lopTonTai.SoLuongToiDa = lopHoc.SoLuongToiDa;
        lopTonTai.CoHoatDong = lopHoc.CoHoatDong;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return lopTonTai;
    }

    /// <summary>
    /// Xóa một lớp học khỏi hệ thống nếu không còn học viên hoặc phân công giảng dạy.
    /// </summary>
    public async Task<bool> XoaLopHocAsync(int id, CancellationToken cancellationToken = default)
    {
        var lopHoc = await _coSoDuLieu.LopHocs.FirstOrDefaultAsync(l => l.LopHocId == id, cancellationToken);
        if (lopHoc == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lớp học có ID {id}.");
        }

        if (!await CoTheXoaAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Không thể xóa lớp học vì còn học viên hoặc phân công giảng dạy liên kết.");
        }

        _coSoDuLieu.LopHocs.Remove(lopHoc);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Kiểm tra xem mã lớp đã tồn tại trong cơ sở dữ liệu hay chưa.
    /// </summary>
    public async Task<bool> KiemTraTonTaiMaAsync(string maLop, int? boQuaId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maLop))
        {
            return false;
        }

        var maChuan = maLop.Trim();
        var truyVan = _coSoDuLieu.LopHocs.Where(l => l.MaLop == maChuan);

        if (boQuaId.HasValue)
        {
            truyVan = truyVan.Where(l => l.LopHocId != boQuaId.Value);
        }

        return await truyVan.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Kiểm tra xem lớp học có thể xóa được hay không (không có học viên hoặc phân công giảng dạy).
    /// </summary>
    public async Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default)
    {
        var coHocVien = await _coSoDuLieu.HocViens.AnyAsync(hv => hv.LopHocId == id, cancellationToken);
        if (coHocVien)
        {
            return false;
        }

        var coPhanCong = await _coSoDuLieu.PhanCongGiangDays.AnyAsync(pc => pc.LopHocId == id, cancellationToken);
        if (coPhanCong)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Lấy danh sách các niên khóa hiện có trong hệ thống để phục vụ bộ lọc.
    /// </summary>
    public async Task<List<string>> LayDanhSachNienKhoaAsync(CancellationToken cancellationToken = default)
    {
        return await _coSoDuLieu.LopHocs
            .AsNoTracking()
            .Select(l => l.NienKhoa)
            .Distinct()
            .OrderByDescending(n => n)
            .ToListAsync(cancellationToken);
    }
}
