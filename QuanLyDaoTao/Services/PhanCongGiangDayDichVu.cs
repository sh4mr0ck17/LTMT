using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ nghiệp vụ quản lý Phân công giảng dạy.
/// </summary>
public class PhanCongGiangDayDichVu : IPhanCongGiangDayDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    /// <summary>
    /// Khởi tạo dịch vụ phân công giảng dạy với DbContext.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    public PhanCongGiangDayDichVu(QuanLyDaoTaoDbContext coSoDuLieu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
    }

    /// <summary>
    /// Lấy danh sách phân công giảng dạy có lọc theo nhân sự, môn học, lớp học, học kỳ, năm học.
    /// </summary>
    public async Task<List<PhanCongGiangDay>> LayDanhSachAsync(
        int? nhanSuId = null,
        int? monHocId = null,
        int? lopHocId = null,
        string? hocKy = null,
        string? namHoc = null,
        string? tuKhoa = null,
        CancellationToken cancellationToken = default)
    {
        var truyVan = _coSoDuLieu.PhanCongGiangDays
            .Include(pc => pc.NhanSu)
            .Include(pc => pc.MonHoc)
            .Include(pc => pc.LopHoc)
            .AsNoTracking()
            .AsQueryable();

        if (nhanSuId.HasValue)
        {
            truyVan = truyVan.Where(pc => pc.NhanSuId == nhanSuId.Value);
        }

        if (monHocId.HasValue)
        {
            truyVan = truyVan.Where(pc => pc.MonHocId == monHocId.Value);
        }

        if (lopHocId.HasValue)
        {
            truyVan = truyVan.Where(pc => pc.LopHocId == lopHocId.Value);
        }

        if (!string.IsNullOrWhiteSpace(hocKy))
        {
            var hocKyChuan = hocKy.Trim();
            truyVan = truyVan.Where(pc => pc.HocKy == hocKyChuan);
        }

        if (!string.IsNullOrWhiteSpace(namHoc))
        {
            var namHocChuan = namHoc.Trim();
            truyVan = truyVan.Where(pc => pc.NamHoc == namHocChuan);
        }

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tuKhoaChuan = tuKhoa.Trim();
            truyVan = truyVan.Where(pc =>
                pc.MaPhanCong.Contains(tuKhoaChuan) ||
                (pc.NhanSu != null && (pc.NhanSu.HoVaTen.Contains(tuKhoaChuan) || pc.NhanSu.MaNhanSu.Contains(tuKhoaChuan))) ||
                (pc.MonHoc != null && (pc.MonHoc.TenMonHoc.Contains(tuKhoaChuan) || pc.MonHoc.MaMonHoc.Contains(tuKhoaChuan))) ||
                (pc.LopHoc != null && (pc.LopHoc.TenLop.Contains(tuKhoaChuan) || pc.LopHoc.MaLop.Contains(tuKhoaChuan))));
        }

        return await truyVan.OrderByDescending(pc => pc.NgayTao).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một phân công theo ID.
    /// </summary>
    public async Task<PhanCongGiangDay?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _coSoDuLieu.PhanCongGiangDays
            .Include(pc => pc.NhanSu)
            .Include(pc => pc.MonHoc)
            .Include(pc => pc.LopHoc)
            .FirstOrDefaultAsync(pc => pc.PhanCongId == id, cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin phân công theo mã phân công duy nhất.
    /// </summary>
    public async Task<PhanCongGiangDay?> LayTheoMaAsync(string maPhanCong, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maPhanCong))
        {
            return null;
        }

        return await _coSoDuLieu.PhanCongGiangDays
            .Include(pc => pc.NhanSu)
            .Include(pc => pc.MonHoc)
            .Include(pc => pc.LopHoc)
            .FirstOrDefaultAsync(pc => pc.MaPhanCong == maPhanCong.Trim(), cancellationToken);
    }

    /// <summary>
    /// Thêm mới một phân công giảng dạy.
    /// </summary>
    public async Task<PhanCongGiangDay> ThemPhanCongAsync(PhanCongGiangDay phanCong, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(phanCong);

        if (!await _coSoDuLieu.NhanSus.AnyAsync(ns => ns.NhanSuId == phanCong.NhanSuId, cancellationToken))
        {
            throw new InvalidOperationException($"Nhân sự có ID {phanCong.NhanSuId} không tồn tại.");
        }

        if (!await _coSoDuLieu.MonHocs.AnyAsync(mh => mh.MonHocId == phanCong.MonHocId, cancellationToken))
        {
            throw new InvalidOperationException($"Môn học có ID {phanCong.MonHocId} không tồn tại.");
        }

        if (!await _coSoDuLieu.LopHocs.AnyAsync(l => l.LopHocId == phanCong.LopHocId, cancellationToken))
        {
            throw new InvalidOperationException($"Lớp học có ID {phanCong.LopHocId} không tồn tại.");
        }

        if (await KiemTraTonTaiMaAsync(phanCong.MaPhanCong, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã phân công '{phanCong.MaPhanCong}' đã tồn tại trong hệ thống.");
        }

        phanCong.NgayTao = DateTime.UtcNow;
        await _coSoDuLieu.PhanCongGiangDays.AddAsync(phanCong, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return phanCong;
    }

    /// <summary>
    /// Cập nhật thông tin phân công giảng dạy.
    /// </summary>
    public async Task<PhanCongGiangDay> CapNhatPhanCongAsync(PhanCongGiangDay phanCong, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(phanCong);

        var pcTonTai = await _coSoDuLieu.PhanCongGiangDays.FirstOrDefaultAsync(pc => pc.PhanCongId == phanCong.PhanCongId, cancellationToken);
        if (pcTonTai == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy phân công có ID {phanCong.PhanCongId}.");
        }

        if (!await _coSoDuLieu.NhanSus.AnyAsync(ns => ns.NhanSuId == phanCong.NhanSuId, cancellationToken))
        {
            throw new InvalidOperationException($"Nhân sự có ID {phanCong.NhanSuId} không tồn tại.");
        }

        if (!await _coSoDuLieu.MonHocs.AnyAsync(mh => mh.MonHocId == phanCong.MonHocId, cancellationToken))
        {
            throw new InvalidOperationException($"Môn học có ID {phanCong.MonHocId} không tồn tại.");
        }

        if (!await _coSoDuLieu.LopHocs.AnyAsync(l => l.LopHocId == phanCong.LopHocId, cancellationToken))
        {
            throw new InvalidOperationException($"Lớp học có ID {phanCong.LopHocId} không tồn tại.");
        }

        if (await KiemTraTonTaiMaAsync(phanCong.MaPhanCong, phanCong.PhanCongId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã phân công '{phanCong.MaPhanCong}' đã tồn tại ở bản ghi khác.");
        }

        pcTonTai.MaPhanCong = phanCong.MaPhanCong;
        pcTonTai.NhanSuId = phanCong.NhanSuId;
        pcTonTai.MonHocId = phanCong.MonHocId;
        pcTonTai.LopHocId = phanCong.LopHocId;
        pcTonTai.HocKy = phanCong.HocKy;
        pcTonTai.NamHoc = phanCong.NamHoc;
        pcTonTai.VaiTroGiangDay = phanCong.VaiTroGiangDay;
        pcTonTai.LaChuNhiem = phanCong.LaChuNhiem;
        pcTonTai.NgayBatDau = phanCong.NgayBatDau;
        pcTonTai.NgayKetThuc = phanCong.NgayKetThuc;
        pcTonTai.GhiChu = phanCong.GhiChu;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return pcTonTai;
    }

    /// <summary>
    /// Xóa một phân công giảng dạy khỏi hệ thống.
    /// </summary>
    public async Task<bool> XoaPhanCongAsync(int id, CancellationToken cancellationToken = default)
    {
        var pc = await _coSoDuLieu.PhanCongGiangDays.FirstOrDefaultAsync(p => p.PhanCongId == id, cancellationToken);
        if (pc == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy phân công có ID {id}.");
        }

        _coSoDuLieu.PhanCongGiangDays.Remove(pc);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Kiểm tra tồn tại mã phân công giảng dạy.
    /// </summary>
    public async Task<bool> KiemTraTonTaiMaAsync(string maPhanCong, int? boQuaId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maPhanCong))
        {
            return false;
        }

        var maChuan = maPhanCong.Trim();
        var truyVan = _coSoDuLieu.PhanCongGiangDays.Where(p => p.MaPhanCong == maChuan);

        if (boQuaId.HasValue)
        {
            truyVan = truyVan.Where(p => p.PhanCongId != boQuaId.Value);
        }

        return await truyVan.AnyAsync(cancellationToken);
    }
}
