using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Lớp triển khai dịch vụ nghiệp vụ quản lý Hồ sơ Nhân sự / Giảng viên.
/// </summary>
public class NhanSuDichVu : INhanSuDichVu
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo dịch vụ nhân sự với DbContext và Dịch vụ Nhật ký.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    /// <param name="nhatKyDichVu">Dịch vụ ghi nhật ký kiểm toán.</param>
    public NhanSuDichVu(QuanLyDaoTaoDbContext coSoDuLieu, INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _coSoDuLieu = coSoDuLieu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Lấy danh sách nhân sự có hỗ trợ lọc theo khoa, bộ môn, chức vụ, trạng thái công tác và từ khóa tìm kiếm.
    /// </summary>
    public async Task<List<NhanSu>> LayDanhSachAsync(
        int? khoaId = null,
        int? boMonId = null,
        int? chucVuId = null,
        string? trangThai = null,
        string? tuKhoa = null,
        bool? coHoatDong = null,
        CancellationToken cancellationToken = default)
    {
        var truyVan = _coSoDuLieu.NhanSus
            .Include(ns => ns.Khoa)
            .Include(ns => ns.BoMon)
            .Include(ns => ns.ChucVu)
            .Include(ns => ns.TrinhDoChuyenMon)
            .Include(ns => ns.PhanCongGiangDays)
            .AsNoTracking()
            .AsQueryable();

        if (khoaId.HasValue)
        {
            truyVan = truyVan.Where(ns => ns.KhoaId == khoaId.Value);
        }

        if (boMonId.HasValue)
        {
            truyVan = truyVan.Where(ns => ns.BoMonId == boMonId.Value);
        }

        if (chucVuId.HasValue)
        {
            truyVan = truyVan.Where(ns => ns.ChucVuId == chucVuId.Value);
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            var trangThaiChuan = trangThai.Trim();
            truyVan = truyVan.Where(ns => ns.TrangThaiCongTac == trangThaiChuan);
        }

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tuKhoaChuan = tuKhoa.Trim();
            truyVan = truyVan.Where(ns =>
                ns.MaNhanSu.Contains(tuKhoaChuan) ||
                ns.HoVaTen.Contains(tuKhoaChuan) ||
                ns.EmailCongViec.Contains(tuKhoaChuan) ||
                (ns.SoDienThoai != null && ns.SoDienThoai.Contains(tuKhoaChuan)) ||
                (ns.ChucDanhNgheNghiep != null && ns.ChucDanhNgheNghiep.Contains(tuKhoaChuan)));
        }

        if (coHoatDong.HasValue)
        {
            truyVan = truyVan.Where(ns => ns.CoHoatDong == coHoatDong.Value);
        }

        return await truyVan.OrderBy(ns => ns.MaNhanSu).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin chi tiết đầy đủ của một nhân sự theo ID.
    /// </summary>
    public async Task<NhanSu?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _coSoDuLieu.NhanSus
            .Include(ns => ns.Khoa)
            .Include(ns => ns.BoMon)
            .Include(ns => ns.ChucVu)
            .Include(ns => ns.TrinhDoChuyenMon)
            .Include(ns => ns.PhanCongGiangDays)
                .ThenInclude(pc => pc.MonHoc)
            .Include(ns => ns.PhanCongGiangDays)
                .ThenInclude(pc => pc.LopHoc)
            .FirstOrDefaultAsync(ns => ns.NhanSuId == id, cancellationToken);
    }

    /// <summary>
    /// Lấy thông tin nhân sự theo mã nhân sự duy nhất.
    /// </summary>
    public async Task<NhanSu?> LayTheoMaAsync(string maNhanSu, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maNhanSu))
        {
            return null;
        }

        return await _coSoDuLieu.NhanSus
            .Include(ns => ns.Khoa)
            .Include(ns => ns.BoMon)
            .Include(ns => ns.ChucVu)
            .Include(ns => ns.TrinhDoChuyenMon)
            .Include(ns => ns.PhanCongGiangDays)
            .FirstOrDefaultAsync(ns => ns.MaNhanSu == maNhanSu.Trim(), cancellationToken);
    }

    /// <summary>
    /// Thêm mới một hồ sơ nhân sự vào hệ thống.
    /// </summary>
    public async Task<NhanSu> ThemNhanSuAsync(NhanSu nhanSu, string? nguoiThucHien = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nhanSu);

        // Kiểm tra mã nhân sự trùng
        if (await KiemTraTonTaiMaAsync(nhanSu.MaNhanSu, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã nhân sự '{nhanSu.MaNhanSu}' đã tồn tại trong hệ thống.");
        }

        // Kiểm tra email công việc trùng
        var emailTonTai = await _coSoDuLieu.NhanSus.AnyAsync(ns => ns.EmailCongViec == nhanSu.EmailCongViec.Trim(), cancellationToken);
        if (emailTonTai)
        {
            throw new InvalidOperationException($"Email công việc '{nhanSu.EmailCongViec}' đã được sử dụng bởi nhân sự khác.");
        }

        // Kiểm tra khóa ngoại hợp lệ nếu có chỉ định
        if (nhanSu.KhoaId.HasValue && !await _coSoDuLieu.Khoas.AnyAsync(k => k.KhoaId == nhanSu.KhoaId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Khoa có ID {nhanSu.KhoaId.Value} không tồn tại.");
        }

        if (nhanSu.BoMonId.HasValue && !await _coSoDuLieu.BoMons.AnyAsync(bm => bm.BoMonId == nhanSu.BoMonId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Bộ môn có ID {nhanSu.BoMonId.Value} không tồn tại.");
        }

        if (nhanSu.ChucVuId.HasValue && !await _coSoDuLieu.ChucVus.AnyAsync(cv => cv.ChucVuId == nhanSu.ChucVuId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Chức vụ có ID {nhanSu.ChucVuId.Value} không tồn tại.");
        }

        if (nhanSu.TrinhDoChuyenMonId.HasValue && !await _coSoDuLieu.TrinhDoChuyenMons.AnyAsync(td => td.TrinhDoChuyenMonId == nhanSu.TrinhDoChuyenMonId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Trình độ chuyên môn có ID {nhanSu.TrinhDoChuyenMonId.Value} không tồn tại.");
        }

        nhanSu.NgayTao = DateTime.UtcNow;
        nhanSu.NguoiTao = nguoiThucHien;

        await _coSoDuLieu.NhanSus.AddAsync(nhanSu, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await _nhatKyDichVu.GhiNhatKyAsync(
            nguoiThucHien ?? "Hệ thống",
            "Thêm mới",
            "NhanSu",
            nhanSu.MaNhanSu,
            null,
            $"Họ tên: {nhanSu.HoVaTen}, Email: {nhanSu.EmailCongViec}",
            "Thành công",
            null,
            cancellationToken);

        return nhanSu;
    }

    /// <summary>
    /// Cập nhật thông tin hồ sơ nhân sự hiện có.
    /// </summary>
    public async Task<NhanSu> CapNhatNhanSuAsync(NhanSu nhanSu, string? nguoiThucHien = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nhanSu);

        var nsTonTai = await _coSoDuLieu.NhanSus.FirstOrDefaultAsync(ns => ns.NhanSuId == nhanSu.NhanSuId, cancellationToken);
        if (nsTonTai == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy hồ sơ nhân sự có ID {nhanSu.NhanSuId}.");
        }

        if (await KiemTraTonTaiMaAsync(nhanSu.MaNhanSu, nhanSu.NhanSuId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã nhân sự '{nhanSu.MaNhanSu}' đã tồn tại ở bản ghi khác.");
        }

        var emailTonTai = await _coSoDuLieu.NhanSus.AnyAsync(ns => ns.EmailCongViec == nhanSu.EmailCongViec.Trim() && ns.NhanSuId != nhanSu.NhanSuId, cancellationToken);
        if (emailTonTai)
        {
            throw new InvalidOperationException($"Email công việc '{nhanSu.EmailCongViec}' đã được sử dụng bởi nhân sự khác.");
        }

        if (nhanSu.KhoaId.HasValue && !await _coSoDuLieu.Khoas.AnyAsync(k => k.KhoaId == nhanSu.KhoaId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Khoa có ID {nhanSu.KhoaId.Value} không tồn tại.");
        }

        if (nhanSu.BoMonId.HasValue && !await _coSoDuLieu.BoMons.AnyAsync(bm => bm.BoMonId == nhanSu.BoMonId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Bộ môn có ID {nhanSu.BoMonId.Value} không tồn tại.");
        }

        if (nhanSu.ChucVuId.HasValue && !await _coSoDuLieu.ChucVus.AnyAsync(cv => cv.ChucVuId == nhanSu.ChucVuId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Chức vụ có ID {nhanSu.ChucVuId.Value} không tồn tại.");
        }

        if (nhanSu.TrinhDoChuyenMonId.HasValue && !await _coSoDuLieu.TrinhDoChuyenMons.AnyAsync(td => td.TrinhDoChuyenMonId == nhanSu.TrinhDoChuyenMonId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Trình độ chuyên môn có ID {nhanSu.TrinhDoChuyenMonId.Value} không tồn tại.");
        }

        // Cập nhật Nhóm A
        nsTonTai.MaNhanSu = nhanSu.MaNhanSu;
        nsTonTai.TrangThaiHoSo = nhanSu.TrangThaiHoSo;
        if (!string.IsNullOrEmpty(nhanSu.HinhAnh))
        {
            nsTonTai.HinhAnh = nhanSu.HinhAnh;
        }

        // Cập nhật Nhóm B
        nsTonTai.HoVaTen = nhanSu.HoVaTen;
        nsTonTai.NgaySinh = nhanSu.NgaySinh;
        nsTonTai.GioiTinh = nhanSu.GioiTinh;
        nsTonTai.QuocTich = nhanSu.QuocTich;
        nsTonTai.DanToc = nhanSu.DanToc;
        nsTonTai.TinhTrangHonNhan = nhanSu.TinhTrangHonNhan;

        // Cập nhật Nhóm C
        nsTonTai.LoaiGiayTo = nhanSu.LoaiGiayTo;
        nsTonTai.SoGiayTo = nhanSu.SoGiayTo;
        nsTonTai.NgayCap = nhanSu.NgayCap;
        nsTonTai.NoiCap = nhanSu.NoiCap;
        nsTonTai.SoDienThoai = nhanSu.SoDienThoai;
        nsTonTai.EmailCongViec = nhanSu.EmailCongViec;
        nsTonTai.EmailCaNhan = nhanSu.EmailCaNhan;
        nsTonTai.DiaChiThuongTru = nhanSu.DiaChiThuongTru;
        nsTonTai.DiaChiLienHe = nhanSu.DiaChiLienHe;
        nsTonTai.NguoiLienHeKhanCap = nhanSu.NguoiLienHeKhanCap;
        nsTonTai.SoDienThoaiKhanCap = nhanSu.SoDienThoaiKhanCap;

        // Cập nhật Nhóm D
        nsTonTai.KhoaId = nhanSu.KhoaId;
        nsTonTai.BoMonId = nhanSu.BoMonId;
        nsTonTai.ChucVuId = nhanSu.ChucVuId;
        nsTonTai.ChucDanhNgheNghiep = nhanSu.ChucDanhNgheNghiep;
        nsTonTai.NguoiQuanLyTrucTiep = nhanSu.NguoiQuanLyTrucTiep;
        nsTonTai.NgayVaoLam = nhanSu.NgayVaoLam;
        nsTonTai.LoaiHopDong = nhanSu.LoaiHopDong;
        nsTonTai.NgayBatDauHopDong = nhanSu.NgayBatDauHopDong;
        nsTonTai.NgayKetThucHopDong = nhanSu.NgayKetThucHopDong;
        nsTonTai.TrangThaiCongTac = nhanSu.TrangThaiCongTac;

        // Cập nhật Nhóm E
        nsTonTai.TrinhDoChuyenMonId = nhanSu.TrinhDoChuyenMonId;
        nsTonTai.TrinhDoHocVan = nhanSu.TrinhDoHocVan;
        nsTonTai.ChuyenNganh = nhanSu.ChuyenNganh;
        nsTonTai.TruongDaoTao = nhanSu.TruongDaoTao;
        nsTonTai.HocVi = nhanSu.HocVi;
        nsTonTai.HocHam = nhanSu.HocHam;
        nsTonTai.LinhVucChuyenMon = nhanSu.LinhVucChuyenMon;
        nsTonTai.KinhNghiemCongTac = nhanSu.KinhNghiemCongTac;
        nsTonTai.GhiChuChuyenMon = nhanSu.GhiChuChuyenMon;

        // Cập nhật Quản trị
        nsTonTai.CoHoatDong = nhanSu.CoHoatDong;
        nsTonTai.NgayCapNhat = DateTime.UtcNow;
        nsTonTai.NguoiCapNhat = nguoiThucHien;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await _nhatKyDichVu.GhiNhatKyAsync(
            nguoiThucHien ?? "Hệ thống",
            "Cập nhật",
            "NhanSu",
            nsTonTai.MaNhanSu,
            null,
            $"Cập nhật hồ sơ nhân sự {nsTonTai.HoVaTen}",
            "Thành công",
            null,
            cancellationToken);

        return nsTonTai;
    }

    /// <summary>
    /// Xóa một hồ sơ nhân sự nếu không còn phân công giảng dạy.
    /// </summary>
    public async Task<bool> XoaNhanSuAsync(int id, string? nguoiThucHien = null, CancellationToken cancellationToken = default)
    {
        var nhanSu = await _coSoDuLieu.NhanSus.FirstOrDefaultAsync(ns => ns.NhanSuId == id, cancellationToken);
        if (nhanSu == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy hồ sơ nhân sự có ID {id}.");
        }

        if (!await CoTheXoaAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Không thể xóa nhân sự vì đang có phân công giảng dạy liên kết.");
        }

        var maNhanSu = nhanSu.MaNhanSu;
        var hoVaTen = nhanSu.HoVaTen;

        _coSoDuLieu.NhanSus.Remove(nhanSu);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await _nhatKyDichVu.GhiNhatKyAsync(
            nguoiThucHien ?? "Hệ thống",
            "Xóa",
            "NhanSu",
            maNhanSu,
            $"Nhân sự: {hoVaTen} ({maNhanSu})",
            null,
            "Thành công",
            null,
            cancellationToken);

        return true;
    }

    /// <summary>
    /// Kiểm tra xem mã nhân sự đã tồn tại trong hệ thống hay chưa.
    /// </summary>
    public async Task<bool> KiemTraTonTaiMaAsync(string maNhanSu, int? boQuaId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maNhanSu))
        {
            return false;
        }

        var maChuan = maNhanSu.Trim();
        var truyVan = _coSoDuLieu.NhanSus.Where(ns => ns.MaNhanSu == maChuan);

        if (boQuaId.HasValue)
        {
            truyVan = truyVan.Where(ns => ns.NhanSuId != boQuaId.Value);
        }

        return await truyVan.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Kiểm tra xem nhân sự có thể xóa được hay không (không có phân công giảng dạy).
    /// </summary>
    public async Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default)
    {
        var coPhanCong = await _coSoDuLieu.PhanCongGiangDays.AnyAsync(pc => pc.NhanSuId == id, cancellationToken);
        return !coPhanCong;
    }

    /// <summary>
    /// Lấy danh sách các trạng thái công tác khả dụng để phục vụ bộ lọc.
    /// </summary>
    public async Task<List<string>> LayDanhSachTrangThaiCongTacAsync(CancellationToken cancellationToken = default)
    {
        var danhSach = await _coSoDuLieu.NhanSus
            .AsNoTracking()
            .Select(ns => ns.TrangThaiCongTac)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!danhSach.Contains("Đang làm việc"))
        {
            danhSach.Add("Đang làm việc");
        }
        if (!danhSach.Contains("Tạm nghỉ"))
        {
            danhSach.Add("Tạm nghỉ");
        }
        if (!danhSach.Contains("Đã nghỉ việc"))
        {
            danhSach.Add("Đã nghỉ việc");
        }

        return danhSach.OrderBy(t => t).ToList();
    }
}
