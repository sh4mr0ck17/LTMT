using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ nghiệp vụ quản lý Trình độ chuyên môn.
/// </summary>
public class TrinhDoChuyenMonDichVu : ITrinhDoChuyenMonDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    /// <summary>
    /// Khởi tạo dịch vụ trình độ chuyên môn với DbContext.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    public TrinhDoChuyenMonDichVu(QuanLyDaoTaoDbContext coSoDuLieu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ trình độ chuyên môn.
    /// </summary>
    public async Task<List<TrinhDoChuyenMon>> LayDanhSachAsync(string? tuKhoa = null, bool? coHoatDong = null, CancellationToken cancellationToken = default)
    {
        var truyVan = _coSoDuLieu.TrinhDoChuyenMons
            .Include(td => td.NhanSus)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tuKhoaChuan = tuKhoa.Trim();
            truyVan = truyVan.Where(td => td.MaTrinhDo.Contains(tuKhoaChuan) || td.TenTrinhDo.Contains(tuKhoaChuan));
        }

        if (coHoatDong.HasValue)
        {
            truyVan = truyVan.Where(td => td.CoHoatDong == coHoatDong.Value);
        }

        return await truyVan.OrderBy(td => td.MaTrinhDo).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin trình độ theo ID.
    /// </summary>
    public async Task<TrinhDoChuyenMon?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _coSoDuLieu.TrinhDoChuyenMons
            .Include(td => td.NhanSus)
            .FirstOrDefaultAsync(td => td.TrinhDoChuyenMonId == id, cancellationToken);
    }

    /// <summary>
    /// Thêm mới trình độ chuyên môn.
    /// </summary>
    public async Task<TrinhDoChuyenMon> ThemTrinhDoAsync(TrinhDoChuyenMon trinhDo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trinhDo);

        if (await KiemTraTonTaiMaAsync(trinhDo.MaTrinhDo, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã trình độ '{trinhDo.MaTrinhDo}' đã tồn tại trong hệ thống.");
        }

        await _coSoDuLieu.TrinhDoChuyenMons.AddAsync(trinhDo, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return trinhDo;
    }

    /// <summary>
    /// Cập nhật thông tin trình độ chuyên môn.
    /// </summary>
    public async Task<TrinhDoChuyenMon> CapNhatTrinhDoAsync(TrinhDoChuyenMon trinhDo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(trinhDo);

        var tdTonTai = await _coSoDuLieu.TrinhDoChuyenMons.FirstOrDefaultAsync(td => td.TrinhDoChuyenMonId == trinhDo.TrinhDoChuyenMonId, cancellationToken);
        if (tdTonTai == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy trình độ có ID {trinhDo.TrinhDoChuyenMonId}.");
        }

        if (await KiemTraTonTaiMaAsync(trinhDo.MaTrinhDo, trinhDo.TrinhDoChuyenMonId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã trình độ '{trinhDo.MaTrinhDo}' đã tồn tại ở bản ghi khác.");
        }

        tdTonTai.MaTrinhDo = trinhDo.MaTrinhDo;
        tdTonTai.TenTrinhDo = trinhDo.TenTrinhDo;
        tdTonTai.MoTa = trinhDo.MoTa;
        tdTonTai.CoHoatDong = trinhDo.CoHoatDong;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return tdTonTai;
    }

    /// <summary>
    /// Xóa trình độ nếu không còn nhân sự liên kết.
    /// </summary>
    public async Task<bool> XoaTrinhDoAsync(int id, CancellationToken cancellationToken = default)
    {
        var td = await _coSoDuLieu.TrinhDoChuyenMons.FirstOrDefaultAsync(t => t.TrinhDoChuyenMonId == id, cancellationToken);
        if (td == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy trình độ có ID {id}.");
        }

        if (!await CoTheXoaAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Không thể xóa trình độ vì đang có nhân sự liên kết.");
        }

        _coSoDuLieu.TrinhDoChuyenMons.Remove(td);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Kiểm tra tồn tại mã trình độ.
    /// </summary>
    public async Task<bool> KiemTraTonTaiMaAsync(string maTrinhDo, int? boQuaId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maTrinhDo))
        {
            return false;
        }

        var maChuan = maTrinhDo.Trim();
        var truyVan = _coSoDuLieu.TrinhDoChuyenMons.Where(td => td.MaTrinhDo == maChuan);

        if (boQuaId.HasValue)
        {
            truyVan = truyVan.Where(td => td.TrinhDoChuyenMonId != boQuaId.Value);
        }

        return await truyVan.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Kiểm tra xem trình độ có thể xóa hay không.
    /// </summary>
    public async Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default)
    {
        return !await _coSoDuLieu.NhanSus.AnyAsync(ns => ns.TrinhDoChuyenMonId == id, cancellationToken);
    }
}
