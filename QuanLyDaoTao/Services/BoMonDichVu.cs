using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ nghiệp vụ quản lý Bộ môn trực thuộc Khoa.
/// </summary>
public class BoMonDichVu : IBoMonDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    /// <summary>
    /// Khởi tạo dịch vụ bộ môn với DbContext.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    public BoMonDichVu(QuanLyDaoTaoDbContext coSoDuLieu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
    }

    /// <summary>
    /// Lấy danh sách bộ môn có hỗ trợ lọc theo khoa và tìm kiếm.
    /// </summary>
    public async Task<List<BoMon>> LayDanhSachAsync(
        int? khoaId = null,
        string? tuKhoa = null,
        bool? coHoatDong = null,
        CancellationToken cancellationToken = default)
    {
        var truyVan = _coSoDuLieu.BoMons
            .Include(bm => bm.Khoa)
            .Include(bm => bm.NhanSus)
            .Include(bm => bm.MonHocs)
            .AsNoTracking()
            .AsQueryable();

        if (khoaId.HasValue)
        {
            truyVan = truyVan.Where(bm => bm.KhoaId == khoaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tuKhoaChuan = tuKhoa.Trim();
            truyVan = truyVan.Where(bm =>
                bm.MaBoMon.Contains(tuKhoaChuan) ||
                bm.TenBoMon.Contains(tuKhoaChuan));
        }

        if (coHoatDong.HasValue)
        {
            truyVan = truyVan.Where(bm => bm.CoHoatDong == coHoatDong.Value);
        }

        return await truyVan.OrderBy(bm => bm.MaBoMon).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một bộ môn theo ID.
    /// </summary>
    public async Task<BoMon?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _coSoDuLieu.BoMons
            .Include(bm => bm.Khoa)
            .Include(bm => bm.NhanSus)
            .Include(bm => bm.MonHocs)
            .FirstOrDefaultAsync(bm => bm.BoMonId == id, cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin bộ môn theo mã nghiệp vụ duy nhất.
    /// </summary>
    public async Task<BoMon?> LayTheoMaAsync(string maBoMon, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maBoMon))
        {
            return null;
        }

        return await _coSoDuLieu.BoMons
            .Include(bm => bm.Khoa)
            .Include(bm => bm.NhanSus)
            .Include(bm => bm.MonHocs)
            .FirstOrDefaultAsync(bm => bm.MaBoMon == maBoMon.Trim(), cancellationToken);
    }

    /// <summary>
    /// Thêm mới một bộ môn vào hệ thống sau khi kiểm tra hợp lệ.
    /// </summary>
    public async Task<BoMon> ThemBoMonAsync(BoMon boMon, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(boMon);

        // Kiểm tra khoa tồn tại
        var khoaTonTai = await _coSoDuLieu.Khoas.AnyAsync(k => k.KhoaId == boMon.KhoaId, cancellationToken);
        if (!khoaTonTai)
        {
            throw new InvalidOperationException($"Khoa chủ quản có ID {boMon.KhoaId} không tồn tại.");
        }

        if (await KiemTraTonTaiMaAsync(boMon.MaBoMon, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã bộ môn '{boMon.MaBoMon}' đã tồn tại trong hệ thống.");
        }

        boMon.NgayTao = DateTime.UtcNow;
        await _coSoDuLieu.BoMons.AddAsync(boMon, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return boMon;
    }

    /// <summary>
    /// Cập nhật thông tin một bộ môn hiện có.
    /// </summary>
    public async Task<BoMon> CapNhatBoMonAsync(BoMon boMon, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(boMon);

        var boMonTonTai = await _coSoDuLieu.BoMons.FirstOrDefaultAsync(bm => bm.BoMonId == boMon.BoMonId, cancellationToken);
        if (boMonTonTai == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bộ môn có ID {boMon.BoMonId}.");
        }

        // Kiểm tra khoa tồn tại
        var khoaTonTai = await _coSoDuLieu.Khoas.AnyAsync(k => k.KhoaId == boMon.KhoaId, cancellationToken);
        if (!khoaTonTai)
        {
            throw new InvalidOperationException($"Khoa chủ quản có ID {boMon.KhoaId} không tồn tại.");
        }

        if (await KiemTraTonTaiMaAsync(boMon.MaBoMon, boMon.BoMonId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã bộ môn '{boMon.MaBoMon}' đã tồn tại ở bản ghi khác.");
        }

        boMonTonTai.MaBoMon = boMon.MaBoMon;
        boMonTonTai.TenBoMon = boMon.TenBoMon;
        boMonTonTai.MoTa = boMon.MoTa;
        boMonTonTai.KhoaId = boMon.KhoaId;
        boMonTonTai.CoHoatDong = boMon.CoHoatDong;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return boMonTonTai;
    }

    /// <summary>
    /// Xóa một bộ môn khỏi hệ thống nếu không còn nhân sự hoặc môn học trực thuộc.
    /// </summary>
    public async Task<bool> XoaBoMonAsync(int id, CancellationToken cancellationToken = default)
    {
        var boMon = await _coSoDuLieu.BoMons.FirstOrDefaultAsync(bm => bm.BoMonId == id, cancellationToken);
        if (boMon == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bộ môn có ID {id}.");
        }

        if (!await CoTheXoaAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Không thể xóa bộ môn vì còn nhân sự hoặc môn học trực thuộc.");
        }

        _coSoDuLieu.BoMons.Remove(boMon);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Kiểm tra xem mã bộ môn đã tồn tại trong cơ sở dữ liệu hay chưa.
    /// </summary>
    public async Task<bool> KiemTraTonTaiMaAsync(string maBoMon, int? boQuaId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maBoMon))
        {
            return false;
        }

        var maChuan = maBoMon.Trim();
        var truyVan = _coSoDuLieu.BoMons.Where(bm => bm.MaBoMon == maChuan);

        if (boQuaId.HasValue)
        {
            truyVan = truyVan.Where(bm => bm.BoMonId != boQuaId.Value);
        }

        return await truyVan.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Kiểm tra xem bộ môn có thể xóa được hay không (không có nhân sự, môn học trực thuộc).
    /// </summary>
    public async Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default)
    {
        var coNhanSu = await _coSoDuLieu.NhanSus.AnyAsync(ns => ns.BoMonId == id, cancellationToken);
        if (coNhanSu)
        {
            return false;
        }

        var coMonHoc = await _coSoDuLieu.MonHocs.AnyAsync(mh => mh.BoMonId == id, cancellationToken);
        if (coMonHoc)
        {
            return false;
        }

        return true;
    }
}
