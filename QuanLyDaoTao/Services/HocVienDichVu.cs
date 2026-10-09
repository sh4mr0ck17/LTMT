using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ nghiệp vụ quản lý Học viên đào tạo.
/// </summary>
public class HocVienDichVu : IHocVienDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    /// <summary>
    /// Khởi tạo dịch vụ học viên với DbContext.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    public HocVienDichVu(QuanLyDaoTaoDbContext coSoDuLieu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
    }

    /// <summary>
    /// Lấy danh sách học viên có lọc theo lớp học, trạng thái và tìm kiếm.
    /// </summary>
    public async Task<List<HocVien>> LayDanhSachAsync(
        int? lopHocId = null,
        string? tuKhoa = null,
        bool? coHoatDong = null,
        CancellationToken cancellationToken = default)
    {
        var truyVan = _coSoDuLieu.HocViens
            .Include(hv => hv.LopHoc)
                .ThenInclude(l => l!.Khoa)
            .AsNoTracking()
            .AsQueryable();

        if (lopHocId.HasValue)
        {
            truyVan = truyVan.Where(hv => hv.LopHocId == lopHocId.Value);
        }

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tuKhoaChuan = tuKhoa.Trim();
            truyVan = truyVan.Where(hv =>
                hv.MaHocVien.Contains(tuKhoaChuan) ||
                hv.HoVaTen.Contains(tuKhoaChuan) ||
                (hv.Email != null && hv.Email.Contains(tuKhoaChuan)) ||
                (hv.SoDienThoai != null && hv.SoDienThoai.Contains(tuKhoaChuan)));
        }

        if (coHoatDong.HasValue)
        {
            truyVan = truyVan.Where(hv => hv.CoHoatDong == coHoatDong.Value);
        }

        return await truyVan.OrderBy(hv => hv.MaHocVien).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin chi tiết một học viên theo ID.
    /// </summary>
    public async Task<HocVien?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _coSoDuLieu.HocViens
            .Include(hv => hv.LopHoc)
                .ThenInclude(l => l!.Khoa)
            .FirstOrDefaultAsync(hv => hv.HocVienId == id, cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin học viên theo mã học viên duy nhất.
    /// </summary>
    public async Task<HocVien?> LayTheoMaAsync(string maHocVien, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maHocVien))
        {
            return null;
        }

        return await _coSoDuLieu.HocViens
            .Include(hv => hv.LopHoc)
                .ThenInclude(l => l!.Khoa)
            .FirstOrDefaultAsync(hv => hv.MaHocVien == maHocVien.Trim(), cancellationToken);
    }

    /// <summary>
    /// Thêm mới một học viên vào hệ thống.
    /// </summary>
    public async Task<HocVien> ThemHocVienAsync(HocVien hocVien, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(hocVien);

        var lopTonTai = await _coSoDuLieu.LopHocs.AnyAsync(l => l.LopHocId == hocVien.LopHocId, cancellationToken);
        if (!lopTonTai)
        {
            throw new InvalidOperationException($"Lớp học có ID {hocVien.LopHocId} không tồn tại.");
        }

        if (await KiemTraTonTaiMaAsync(hocVien.MaHocVien, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã học viên '{hocVien.MaHocVien}' đã tồn tại trong hệ thống.");
        }

        hocVien.NgayTao = DateTime.UtcNow;
        await _coSoDuLieu.HocViens.AddAsync(hocVien, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return hocVien;
    }

    /// <summary>
    /// Cập nhật thông tin một học viên hiện có.
    /// </summary>
    public async Task<HocVien> CapNhatHocVienAsync(HocVien hocVien, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(hocVien);

        var hocVienTonTai = await _coSoDuLieu.HocViens.FirstOrDefaultAsync(hv => hv.HocVienId == hocVien.HocVienId, cancellationToken);
        if (hocVienTonTai == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy học viên có ID {hocVien.HocVienId}.");
        }

        var lopTonTai = await _coSoDuLieu.LopHocs.AnyAsync(l => l.LopHocId == hocVien.LopHocId, cancellationToken);
        if (!lopTonTai)
        {
            throw new InvalidOperationException($"Lớp học có ID {hocVien.LopHocId} không tồn tại.");
        }

        if (await KiemTraTonTaiMaAsync(hocVien.MaHocVien, hocVien.HocVienId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã học viên '{hocVien.MaHocVien}' đã tồn tại ở bản ghi khác.");
        }

        hocVienTonTai.MaHocVien = hocVien.MaHocVien;
        hocVienTonTai.HoVaTen = hocVien.HoVaTen;
        hocVienTonTai.NgaySinh = hocVien.NgaySinh;
        hocVienTonTai.GioiTinh = hocVien.GioiTinh;
        hocVienTonTai.Email = hocVien.Email;
        hocVienTonTai.SoDienThoai = hocVien.SoDienThoai;
        hocVienTonTai.DiaChi = hocVien.DiaChi;
        hocVienTonTai.LopHocId = hocVien.LopHocId;
        hocVienTonTai.CoHoatDong = hocVien.CoHoatDong;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return hocVienTonTai;
    }

    /// <summary>
    /// Xóa một học viên khỏi hệ thống.
    /// </summary>
    public async Task<bool> XoaHocVienAsync(int id, CancellationToken cancellationToken = default)
    {
        var hocVien = await _coSoDuLieu.HocViens.FirstOrDefaultAsync(hv => hv.HocVienId == id, cancellationToken);
        if (hocVien == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy học viên có ID {id}.");
        }

        _coSoDuLieu.HocViens.Remove(hocVien);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Kiểm tra xem mã học viên đã tồn tại trong hệ thống hay chưa.
    /// </summary>
    public async Task<bool> KiemTraTonTaiMaAsync(string maHocVien, int? boQuaId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maHocVien))
        {
            return false;
        }

        var maChuan = maHocVien.Trim();
        var truyVan = _coSoDuLieu.HocViens.Where(hv => hv.MaHocVien == maChuan);

        if (boQuaId.HasValue)
        {
            truyVan = truyVan.Where(hv => hv.HocVienId != boQuaId.Value);
        }

        return await truyVan.AnyAsync(cancellationToken);
    }
}
