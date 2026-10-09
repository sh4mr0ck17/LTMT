using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ nghiệp vụ quản lý Khoa đào tạo.
/// </summary>
public class KhoaDichVu : IKhoaDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    /// <summary>
    /// Khởi tạo dịch vụ khoa với DbContext.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    public KhoaDichVu(QuanLyDaoTaoDbContext coSoDuLieu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ các khoa có hỗ trợ tìm kiếm và lọc.
    /// </summary>
    public async Task<List<Khoa>> LayDanhSachAsync(
        string? tuKhoa = null,
        bool? coHoatDong = null,
        CancellationToken cancellationToken = default)
    {
        var truyVan = _coSoDuLieu.Khoas
            .Include(k => k.BoMons)
            .Include(k => k.LopHocs)
            .Include(k => k.NhanSus)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tuKhoaChuan = tuKhoa.Trim();
            truyVan = truyVan.Where(k =>
                k.MaKhoa.Contains(tuKhoaChuan) ||
                k.TenKhoa.Contains(tuKhoaChuan));
        }

        if (coHoatDong.HasValue)
        {
            truyVan = truyVan.Where(k => k.CoHoatDong == coHoatDong.Value);
        }

        return await truyVan.OrderBy(k => k.MaKhoa).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một khoa theo khóa chính Id.
    /// </summary>
    public async Task<Khoa?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _coSoDuLieu.Khoas
            .Include(k => k.BoMons)
            .Include(k => k.LopHocs)
            .Include(k => k.NhanSus)
            .FirstOrDefaultAsync(k => k.KhoaId == id, cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin khoa theo mã nghiệp vụ duy nhất.
    /// </summary>
    public async Task<Khoa?> LayTheoMaAsync(string maKhoa, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maKhoa))
        {
            return null;
        }

        return await _coSoDuLieu.Khoas
            .Include(k => k.BoMons)
            .Include(k => k.LopHocs)
            .Include(k => k.NhanSus)
            .FirstOrDefaultAsync(k => k.MaKhoa == maKhoa.Trim(), cancellationToken);
    }

    /// <summary>
    /// Thêm mới một khoa vào hệ thống sau khi kiểm tra hợp lệ.
    /// </summary>
    public async Task<Khoa> ThemKhoaAsync(Khoa khoa, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(khoa);

        if (await KiemTraTonTaiMaAsync(khoa.MaKhoa, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã khoa '{khoa.MaKhoa}' đã tồn tại trong hệ thống.");
        }

        khoa.NgayTao = DateTime.UtcNow;
        await _coSoDuLieu.Khoas.AddAsync(khoa, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return khoa;
    }

    /// <summary>
    /// Cập nhật thông tin một khoa hiện có trong hệ thống.
    /// </summary>
    public async Task<Khoa> CapNhatKhoaAsync(Khoa khoa, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(khoa);

        var khoaTonTai = await _coSoDuLieu.Khoas.FirstOrDefaultAsync(k => k.KhoaId == khoa.KhoaId, cancellationToken);
        if (khoaTonTai == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy khoa có ID {khoa.KhoaId}.");
        }

        if (await KiemTraTonTaiMaAsync(khoa.MaKhoa, khoa.KhoaId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã khoa '{khoa.MaKhoa}' đã tồn tại ở bản ghi khác.");
        }

        khoaTonTai.MaKhoa = khoa.MaKhoa;
        khoaTonTai.TenKhoa = khoa.TenKhoa;
        khoaTonTai.MoTa = khoa.MoTa;
        khoaTonTai.SoDienThoai = khoa.SoDienThoai;
        khoaTonTai.Email = khoa.Email;
        khoaTonTai.DiaDiem = khoa.DiaDiem;
        khoaTonTai.CoHoatDong = khoa.CoHoatDong;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return khoaTonTai;
    }

    /// <summary>
    /// Xóa một khoa khỏi hệ thống nếu không còn dữ liệu phụ thuộc liên quan.
    /// </summary>
    public async Task<bool> XoaKhoaAsync(int id, CancellationToken cancellationToken = default)
    {
        var khoa = await _coSoDuLieu.Khoas.FirstOrDefaultAsync(k => k.KhoaId == id, cancellationToken);
        if (khoa == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy khoa có ID {id}.");
        }

        if (!await CoTheXoaAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Không thể xóa khoa vì còn bộ môn, lớp học hoặc nhân sự trực thuộc.");
        }

        _coSoDuLieu.Khoas.Remove(khoa);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Kiểm tra xem mã khoa đã tồn tại trong cơ sở dữ liệu hay chưa.
    /// </summary>
    public async Task<bool> KiemTraTonTaiMaAsync(string maKhoa, int? boQuaId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maKhoa))
        {
            return false;
        }

        var maChuan = maKhoa.Trim();
        var truyVan = _coSoDuLieu.Khoas.Where(k => k.MaKhoa == maChuan);

        if (boQuaId.HasValue)
        {
            truyVan = truyVan.Where(k => k.KhoaId != boQuaId.Value);
        }

        return await truyVan.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Kiểm tra xem khoa có thể xóa được hay không (không có bộ môn, lớp học, nhân sự trực thuộc).
    /// </summary>
    public async Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default)
    {
        var coBoMon = await _coSoDuLieu.BoMons.AnyAsync(bm => bm.KhoaId == id, cancellationToken);
        if (coBoMon)
        {
            return false;
        }

        var coLop = await _coSoDuLieu.LopHocs.AnyAsync(l => l.KhoaId == id, cancellationToken);
        if (coLop)
        {
            return false;
        }

        var coNhanSu = await _coSoDuLieu.NhanSus.AnyAsync(ns => ns.KhoaId == id, cancellationToken);
        if (coNhanSu)
        {
            return false;
        }

        return true;
    }
}
