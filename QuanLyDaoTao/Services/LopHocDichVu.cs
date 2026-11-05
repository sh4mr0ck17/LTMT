using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ nghiệp vụ quản lý Lớp học đào tạo.
/// </summary>
public class LopHocDichVu : DichVuNghiepVuCoSo, ILopHocDichVu
{
    private const string TenBang = "LopHoc";

    /// <summary>
    /// Khởi tạo dịch vụ lớp học.
    /// </summary>
    public LopHocDichVu(
        QuanLyDaoTaoDbContext coSoDuLieu,
        IPhanQuyenDichVu? phanQuyen = null,
        INhatKyDichVu? nhatKy = null,
        IBusThayDoiDuLieu? bus = null)
        : base(coSoDuLieu, phanQuyen, nhatKy, bus)
    {
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
                l.NienKhoa.Contains(tuKhoaChuan) ||
                l.HeDaoTao.Contains(tuKhoaChuan) ||
                (l.KhoaHoc != null && l.KhoaHoc.Contains(tuKhoaChuan)) ||
                (l.ChuyenNganh != null && l.ChuyenNganh.Contains(tuKhoaChuan)));
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
        ChuanHoaVaKiemTra(lopHoc);

        var khoaTonTai = await _coSoDuLieu.Khoas.AnyAsync(k => k.KhoaId == lopHoc.KhoaId, cancellationToken);
        if (!khoaTonTai)
        {
            throw new InvalidOperationException($"Khoa quản lý có ID {lopHoc.KhoaId} không tồn tại.");
        }

        var nguoiDung = await DamBaoQuyenGhiAsync(VungQuanLy.DaoTao, lopHoc.KhoaId);
        await DongBoNienKhoaTheoKhoaHocAsync(lopHoc, cancellationToken);

        if (await KiemTraTonTaiMaAsync(lopHoc.MaLop, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã lớp '{lopHoc.MaLop}' đã tồn tại trong hệ thống.");
        }

        lopHoc.NgayTao = DateTime.UtcNow;
        lopHoc.NgayCapNhat = null;
        await _coSoDuLieu.LopHocs.AddAsync(lopHoc, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await HoanTatThaoTacAsync(nguoiDung, "Thêm mới", TenBang, lopHoc.MaLop, null, MoTa(lopHoc), cancellationToken);
        return lopHoc;
    }

    /// <summary>
    /// Cập nhật thông tin một lớp học hiện có.
    /// </summary>
    public async Task<LopHoc> CapNhatLopHocAsync(LopHoc lopHoc, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(lopHoc);
        ChuanHoaVaKiemTra(lopHoc);

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

        var nguoiDung = await DamBaoQuyenGhiAsync(VungQuanLy.DaoTao, lopTonTai.KhoaId);
        if (lopHoc.KhoaId != lopTonTai.KhoaId)
        {
            await DamBaoQuyenGhiAsync(VungQuanLy.DaoTao, lopHoc.KhoaId);
        }

        if (await KiemTraTonTaiMaAsync(lopHoc.MaLop, lopHoc.LopHocId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã lớp '{lopHoc.MaLop}' đã tồn tại ở bản ghi khác.");
        }

        await DongBoNienKhoaTheoKhoaHocAsync(lopHoc, cancellationToken);

        var siSo = await _coSoDuLieu.HocViens.CountAsync(hv => hv.LopHocId == lopHoc.LopHocId, cancellationToken);
        if (lopHoc.SoLuongToiDa < siSo)
        {
            throw new InvalidOperationException($"Sĩ số tối đa không được nhỏ hơn sĩ số hiện tại ({siSo} học viên).");
        }

        var giaTriCu = MoTa(lopTonTai);
        lopTonTai.MaLop = lopHoc.MaLop;
        lopTonTai.TenLop = lopHoc.TenLop;
        lopTonTai.NienKhoa = lopHoc.NienKhoa;
        lopTonTai.KhoaId = lopHoc.KhoaId;
        lopTonTai.HeDaoTao = lopHoc.HeDaoTao;
        lopTonTai.KhoaHoc = lopHoc.KhoaHoc;
        lopTonTai.ChuyenNganh = lopHoc.ChuyenNganh;
        lopTonTai.SoLuongToiDa = lopHoc.SoLuongToiDa;
        lopTonTai.CoHoatDong = lopHoc.CoHoatDong;
        lopTonTai.NgayCapNhat = DateTime.UtcNow;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await HoanTatThaoTacAsync(nguoiDung, "Cập nhật", TenBang, lopTonTai.MaLop, giaTriCu, MoTa(lopTonTai), cancellationToken);
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

        var nguoiDung = await DamBaoQuyenGhiAsync(VungQuanLy.DaoTao, lopHoc.KhoaId);

        if (!await CoTheXoaAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Không thể xóa lớp học vì còn học viên hoặc phân công giảng dạy liên kết.");
        }

        _coSoDuLieu.LopHocs.Remove(lopHoc);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await HoanTatThaoTacAsync(nguoiDung, "Xóa", TenBang, lopHoc.MaLop, MoTa(lopHoc), null, cancellationToken);
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

    private static void ChuanHoaVaKiemTra(LopHoc lopHoc)
    {
        lopHoc.MaLop = QuyTacNghiepVu.ChuanHoaMa(lopHoc.MaLop);
        lopHoc.TenLop = QuyTacNghiepVu.ChuanHoaChuoi(lopHoc.TenLop);
        lopHoc.HeDaoTao = string.IsNullOrWhiteSpace(lopHoc.HeDaoTao) ? "Chính quy" : QuyTacNghiepVu.ChuanHoaChuoi(lopHoc.HeDaoTao);
        if (!QuyTacNghiepVu.DanhSachHeDaoTao.Contains(lopHoc.HeDaoTao))
        {
            throw new InvalidOperationException($"Hệ đào tạo phải là một trong: {string.Join(", ", QuyTacNghiepVu.DanhSachHeDaoTao)}.");
        }

        if (lopHoc.SoLuongToiDa is < 1 or > 200)
        {
            throw new InvalidOperationException("Sĩ số tối đa phải từ 1 đến 200.");
        }
        lopHoc.KhoaHoc = string.IsNullOrWhiteSpace(lopHoc.KhoaHoc)
            ? (lopHoc.HeDaoTao == "Chính quy" ? QuyTacNghiepVu.SuyRaKhoaHoc(lopHoc.MaLop) : null)
            : QuyTacNghiepVu.ChuanHoaMa(lopHoc.KhoaHoc);
        lopHoc.ChuyenNganh = string.IsNullOrWhiteSpace(lopHoc.ChuyenNganh) ? null : QuyTacNghiepVu.ChuanHoaChuoi(lopHoc.ChuyenNganh);

        var loi = QuyTacNghiepVu.KiemTraMaLop(lopHoc.MaLop, lopHoc.HeDaoTao, lopHoc.KhoaHoc);
        if (loi is not null)
        {
            throw new InvalidOperationException(loi);
        }

        if (string.IsNullOrWhiteSpace(lopHoc.TenLop))
        {
            lopHoc.TenLop = $"Lớp {lopHoc.MaLop}" + (lopHoc.KhoaHoc is null ? string.Empty : $" - Khóa {lopHoc.KhoaHoc}");
        }
    }

    /// <summary>
    /// Một khóa học chỉ có MỘT niên khóa: lớp cùng khóa dùng chung niên khóa (để trống thì tự lấy theo khóa),
    /// khai báo khác đi bị chặn. Niên khóa luôn được chuẩn hóa về dạng YYYY-YYYY.
    /// </summary>
    private async Task DongBoNienKhoaTheoKhoaHocAsync(LopHoc lopHoc, CancellationToken cancellationToken)
    {
        string? nienKhoaCuaKhoa = null;
        if (lopHoc.KhoaHoc is not null)
        {
            nienKhoaCuaKhoa = await _coSoDuLieu.LopHocs.AsNoTracking()
                .Where(l => l.KhoaHoc == lopHoc.KhoaHoc && l.LopHocId != lopHoc.LopHocId)
                .Select(l => l.NienKhoa)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(lopHoc.NienKhoa))
        {
            lopHoc.NienKhoa = nienKhoaCuaKhoa
                ?? throw new InvalidOperationException("Vui lòng nhập niên khóa (dạng 2025-2029).");
            return;
        }

        var chuan = QuyTacNghiepVu.ChuanHoaNienKhoa(lopHoc.NienKhoa)
            ?? throw new InvalidOperationException($"Niên khóa phải có dạng YYYY-YYYY, dài 1–{QuyTacNghiepVu.SoNamNienKhoaToiDa} năm (ví dụ 2025-2029).");
        if (nienKhoaCuaKhoa is not null && QuyTacNghiepVu.ChuanHoaNienKhoa(nienKhoaCuaKhoa) is { } chuanCuaKhoa && chuanCuaKhoa != chuan)
        {
            throw new InvalidOperationException($"Khóa {lopHoc.KhoaHoc} có niên khóa {chuanCuaKhoa}; các lớp cùng khóa phải cùng niên khóa.");
        }

        lopHoc.NienKhoa = chuan;
    }

    private static string MoTa(LopHoc lop) =>
        $"{lop.MaLop} | {lop.HeDaoTao} {lop.KhoaHoc} | {lop.ChuyenNganh ?? "-"} | tối đa {lop.SoLuongToiDa}";
}
