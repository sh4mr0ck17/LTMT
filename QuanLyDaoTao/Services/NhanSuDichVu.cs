using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Dịch vụ nghiệp vụ Hồ sơ Cán bộ / Giảng viên (vùng Tổ chức - Cán bộ):
/// phân quyền theo khoa, ràng buộc định danh, cơ cấu lãnh đạo khoa/bộ môn và che thông tin cá nhân.
/// </summary>
public class NhanSuDichVu : DichVuNghiepVuCoSo, INhanSuDichVu
{
    private const string TenBang = "NhanSu";

    /// <summary>
    /// Chỉ tiêu chức vụ lãnh đạo theo mã chức vụ: phạm vi tính (Khoa hoặc BoMon) và số người tối đa.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, (string PhamVi, int ToiDa)> ChiTieuLanhDao =
        new Dictionary<string, (string, int)>(StringComparer.OrdinalIgnoreCase)
        {
            ["CV_TK"] = ("Khoa", 1),
            ["CV_PK"] = ("Khoa", 2),
            ["CV_TBM"] = ("BoMon", 1),
            ["CV_PBM"] = ("BoMon", 1)
        };

    /// <summary>
    /// Khởi tạo dịch vụ nhân sự.
    /// </summary>
    public NhanSuDichVu(
        QuanLyDaoTaoDbContext coSoDuLieu,
        INhatKyDichVu nhatKyDichVu,
        IPhanQuyenDichVu? phanQuyen = null,
        IBusThayDoiDuLieu? bus = null)
        : base(coSoDuLieu, phanQuyen, nhatKyDichVu ?? throw new ArgumentNullException(nameof(nhatKyDichVu)), bus)
    {
    }

    /// <summary>
    /// Lấy danh sách nhân sự có lọc; thông tin cá nhân bị che nếu người dùng không quản lý khoa tương ứng.
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
            truyVan = truyVan.Where(ns => ns.TrangThai == trangThaiChuan);
        }

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tuKhoaChuan = tuKhoa.Trim();
            truyVan = truyVan.Where(ns =>
                ns.MaNhanSu.Contains(tuKhoaChuan) ||
                ns.HoVaTen.Contains(tuKhoaChuan) ||
                ns.Email.Contains(tuKhoaChuan) ||
                (ns.SoDienThoai != null && ns.SoDienThoai.Contains(tuKhoaChuan)) ||
                (ns.CapBacHam != null && ns.CapBacHam.Contains(tuKhoaChuan)) ||
                (ns.ChuyenMonDaoTao != null && ns.ChuyenMonDaoTao.Contains(tuKhoaChuan)) ||
                (ns.ChucVu != null && ns.ChucVu.TenChucVu.Contains(tuKhoaChuan)));
        }

        if (coHoatDong.HasValue)
        {
            truyVan = truyVan.Where(ns => coHoatDong.Value
                ? (ns.TrangThai != "Đã khóa" && ns.TrangThai != "Đã nghỉ việc")
                : (ns.TrangThai == "Đã khóa" || ns.TrangThai == "Đã nghỉ việc"));
        }

        var danhSach = await truyVan.OrderBy(ns => ns.MaNhanSu).ToListAsync(cancellationToken);
        var nguoiDung = await LayNguoiDungAsync();
        foreach (var nhanSu in danhSach)
        {
            if (!nguoiDung.CoTheXemThongTinCaNhan(VungQuanLy.ToChuc, nhanSu.KhoaId))
            {
                CheThongTinCaNhan(nhanSu);
            }
        }

        return danhSach;
    }

    /// <summary>
    /// Lấy chi tiết nhân sự theo ID kèm phân công giảng dạy.
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
    /// Lấy chi tiết nhân sự theo mã.
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
    /// Thêm mới hồ sơ nhân sự.
    /// </summary>
    public async Task<NhanSu> ThemNhanSuAsync(NhanSu nhanSu, string? nguoiThucHien = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nhanSu);
        ChuanHoa(nhanSu);
        await DongBoKhoaTheoBoMonAsync(nhanSu, cancellationToken);
        var nguoiDung = await DamBaoQuyenGhiAsync(VungQuanLy.ToChuc, nhanSu.KhoaId);

        if (await KiemTraTonTaiMaAsync(nhanSu.MaNhanSu, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã nhân sự '{nhanSu.MaNhanSu}' đã tồn tại trong hệ thống.");
        }

        await KiemTraRangBuocAsync(nhanSu, null, cancellationToken);

        nhanSu.NgayTao = DateTime.UtcNow;
        nhanSu.NgayCapNhat = null;
        var tenNguoiTao = TenNguoiThucHien(nguoiDung, nguoiThucHien);
        nhanSu.NguoiTao = tenNguoiTao;

        await _coSoDuLieu.NhanSus.AddAsync(nhanSu, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await HoanTatThaoTacAsync(nguoiDung with { TenDangNhap = tenNguoiTao }, "Thêm mới", TenBang, nhanSu.MaNhanSu,
            null, $"Họ tên: {nhanSu.HoVaTen}, Email: {nhanSu.Email}", cancellationToken);

        return nhanSu;
    }

    /// <summary>
    /// Cập nhật hồ sơ nhân sự (kể cả điều chuyển khoa/bộ môn). Cần quyền với cả khoa cũ và khoa mới.
    /// </summary>
    public async Task<NhanSu> CapNhatNhanSuAsync(NhanSu nhanSu, string? nguoiThucHien = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nhanSu);
        ChuanHoa(nhanSu);

        var nsTonTai = await _coSoDuLieu.NhanSus.FirstOrDefaultAsync(ns => ns.NhanSuId == nhanSu.NhanSuId, cancellationToken);
        if (nsTonTai == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy hồ sơ nhân sự có ID {nhanSu.NhanSuId}.");
        }

        await DongBoKhoaTheoBoMonAsync(nhanSu, cancellationToken);
        var nguoiDung = await DamBaoQuyenGhiAsync(VungQuanLy.ToChuc, nsTonTai.KhoaId);
        if (nhanSu.KhoaId != nsTonTai.KhoaId)
        {
            await DamBaoQuyenGhiAsync(VungQuanLy.ToChuc, nhanSu.KhoaId);
        }

        if (await KiemTraTonTaiMaAsync(nhanSu.MaNhanSu, nhanSu.NhanSuId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã nhân sự '{nhanSu.MaNhanSu}' đã tồn tại ở bản ghi khác.");
        }

        await KiemTraRangBuocAsync(nhanSu, nsTonTai.NhanSuId, cancellationToken);

        var giaTriCu = MoTa(nsTonTai);
        nsTonTai.MaNhanSu = nhanSu.MaNhanSu;
        nsTonTai.HoVaTen = nhanSu.HoVaTen;
        nsTonTai.NgaySinh = nhanSu.NgaySinh;
        nsTonTai.GioiTinh = nhanSu.GioiTinh;
        nsTonTai.SoCCCD = nhanSu.SoCCCD;
        nsTonTai.SoDienThoai = nhanSu.SoDienThoai;
        nsTonTai.Email = nhanSu.Email;
        nsTonTai.DiaChi = nhanSu.DiaChi;
        nsTonTai.QueQuan = nhanSu.QueQuan;
        nsTonTai.KhoaId = nhanSu.KhoaId;
        nsTonTai.BoMonId = nhanSu.BoMonId;
        nsTonTai.ChucVuId = nhanSu.ChucVuId;
        nsTonTai.TrinhDoChuyenMonId = nhanSu.TrinhDoChuyenMonId;
        nsTonTai.CapBacHam = nhanSu.CapBacHam;
        nsTonTai.HocHam = nhanSu.HocHam;
        nsTonTai.ChuyenMonDaoTao = nhanSu.ChuyenMonDaoTao;
        nsTonTai.NgayVaoLam = nhanSu.NgayVaoLam;
        nsTonTai.TrangThai = nhanSu.TrangThai;
        nsTonTai.GhiChu = nhanSu.GhiChu;
        if (!string.IsNullOrEmpty(nhanSu.HinhAnh))
        {
            nsTonTai.HinhAnh = nhanSu.HinhAnh;
        }

        nsTonTai.NgayCapNhat = DateTime.UtcNow;
        var tenNguoiCapNhat = TenNguoiThucHien(nguoiDung, nguoiThucHien);
        nsTonTai.NguoiCapNhat = tenNguoiCapNhat;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await HoanTatThaoTacAsync(nguoiDung with { TenDangNhap = tenNguoiCapNhat }, "Cập nhật", TenBang,
            nsTonTai.MaNhanSu, giaTriCu, MoTa(nsTonTai), cancellationToken);

        return nsTonTai;
    }

    /// <summary>
    /// Xóa hồ sơ nhân sự (bị chặn nếu còn phân công giảng dạy).
    /// </summary>
    public async Task<bool> XoaNhanSuAsync(int id, string? nguoiThucHien = null, CancellationToken cancellationToken = default)
    {
        var nhanSu = await _coSoDuLieu.NhanSus.FirstOrDefaultAsync(ns => ns.NhanSuId == id, cancellationToken);
        if (nhanSu == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy hồ sơ nhân sự có ID {id}.");
        }

        var nguoiDung = await DamBaoQuyenGhiAsync(VungQuanLy.ToChuc, nhanSu.KhoaId);

        if (!await CoTheXoaAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Không thể xóa nhân sự vì đang có phân công giảng dạy liên kết.");
        }

        var giaTriCu = $"Nhân sự: {nhanSu.HoVaTen} ({nhanSu.MaNhanSu})";
        _coSoDuLieu.NhanSus.Remove(nhanSu);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await HoanTatThaoTacAsync(nguoiDung with { TenDangNhap = TenNguoiThucHien(nguoiDung, nguoiThucHien) }, "Xóa",
            TenBang, nhanSu.MaNhanSu, giaTriCu, null, cancellationToken);

        return true;
    }

    /// <summary>
    /// Kiểm tra trùng mã nhân sự.
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
    /// Kiểm tra nhân sự có thể xóa (không còn phân công giảng dạy).
    /// </summary>
    public async Task<bool> CoTheXoaAsync(int id, CancellationToken cancellationToken = default)
    {
        var coPhanCong = await _coSoDuLieu.PhanCongGiangDays.AnyAsync(pc => pc.NhanSuId == id, cancellationToken);
        return !coPhanCong;
    }

    /// <summary>
    /// Danh sách trạng thái công tác hiện có cộng với các trạng thái chuẩn.
    /// </summary>
    public async Task<List<string>> LayDanhSachTrangThaiCongTacAsync(CancellationToken cancellationToken = default)
    {
        // Danh sách cố định: không gom giá trị tự do từ CSDL để tránh mỗi nơi một cách viết.
        await Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();
        return QuyTacNghiepVu.DanhSachTrangThaiCanBo.ToList();
    }

    /// <summary>
    /// Che thông tin cá nhân trên bản sao không theo dõi.
    /// </summary>
    public static void CheThongTinCaNhan(NhanSu nhanSu)
    {
        ArgumentNullException.ThrowIfNull(nhanSu);
        nhanSu.SoCCCD = QuyTacNghiepVu.CheThongTin(nhanSu.SoCCCD);
        nhanSu.SoDienThoai = QuyTacNghiepVu.CheThongTin(nhanSu.SoDienThoai);
        nhanSu.DiaChi = null;
        nhanSu.NgaySinh = new DateTime(nhanSu.NgaySinh.Year, 1, 1);
    }

    private static void ChuanHoa(NhanSu nhanSu)
    {
        nhanSu.MaNhanSu = (nhanSu.MaNhanSu ?? string.Empty).Trim();
        nhanSu.HoVaTen = QuyTacNghiepVu.ChuanHoaChuoi(nhanSu.HoVaTen);
        nhanSu.Email = (nhanSu.Email ?? string.Empty).Trim();
        nhanSu.SoCCCD = string.IsNullOrWhiteSpace(nhanSu.SoCCCD) ? null : nhanSu.SoCCCD.Trim();
        nhanSu.SoDienThoai = string.IsNullOrWhiteSpace(nhanSu.SoDienThoai) ? null : nhanSu.SoDienThoai.Trim();
        nhanSu.CapBacHam = string.IsNullOrWhiteSpace(nhanSu.CapBacHam) ? null : nhanSu.CapBacHam.Trim();
        nhanSu.HocHam = string.IsNullOrWhiteSpace(nhanSu.HocHam) ? null : nhanSu.HocHam.Trim();
        nhanSu.QueQuan = string.IsNullOrWhiteSpace(nhanSu.QueQuan) ? null : nhanSu.QueQuan.Trim();
    }

    private async Task DongBoKhoaTheoBoMonAsync(NhanSu nhanSu, CancellationToken cancellationToken)
    {
        if (!nhanSu.BoMonId.HasValue)
        {
            return;
        }

        var khoaCuaBoMon = await _coSoDuLieu.BoMons.AsNoTracking()
            .Where(bm => bm.BoMonId == nhanSu.BoMonId.Value)
            .Select(bm => (int?)bm.KhoaId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Bộ môn có ID {nhanSu.BoMonId.Value} không tồn tại.");

        if (nhanSu.KhoaId.HasValue && nhanSu.KhoaId != khoaCuaBoMon)
        {
            throw new InvalidOperationException("Bộ môn được chọn không thuộc khoa đã chọn.");
        }

        nhanSu.KhoaId = khoaCuaBoMon;
    }

    private async Task KiemTraRangBuocAsync(NhanSu nhanSu, int? boQuaId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(nhanSu.HoVaTen))
        {
            throw new InvalidOperationException("Họ và tên không được để trống.");
        }

        if (!QuyTacNghiepVu.LaNgaySinhHopLe(nhanSu.NgaySinh))
        {
            throw new InvalidOperationException($"Ngày sinh phải từ năm {QuyTacNghiepVu.NamSinhToiThieu} đến hôm nay.");
        }

        if (!QuyTacNghiepVu.DanhSachGioiTinh.Contains(nhanSu.GioiTinh))
        {
            throw new InvalidOperationException("Giới tính chỉ nhận Nam hoặc Nữ.");
        }

        if (!QuyTacNghiepVu.DanhSachTrangThaiCanBo.Contains(nhanSu.TrangThai))
        {
            throw new InvalidOperationException($"Trạng thái công tác phải là một trong: {string.Join(", ", QuyTacNghiepVu.DanhSachTrangThaiCanBo)}.");
        }

        if (string.IsNullOrWhiteSpace(nhanSu.Email))
        {
            throw new InvalidOperationException("Email công việc là bắt buộc.");
        }

        if (nhanSu.SoCCCD is not null && !QuyTacNghiepVu.LaCccdHopLe(nhanSu.SoCCCD))
        {
            throw new InvalidOperationException("Số CCCD phải gồm đúng 12 chữ số.");
        }

        if (nhanSu.SoDienThoai is not null && !QuyTacNghiepVu.LaSoDienThoaiHopLe(nhanSu.SoDienThoai))
        {
            throw new InvalidOperationException("Số điện thoại phải có 10 chữ số và đầu số di động hợp lệ (03, 05, 07, 08, 09).");
        }

        var emailTonTai = await _coSoDuLieu.NhanSus.AnyAsync(ns => ns.Email == nhanSu.Email && ns.NhanSuId != boQuaId, cancellationToken);
        if (emailTonTai)
        {
            throw new InvalidOperationException($"Email công việc '{nhanSu.Email}' đã được sử dụng bởi nhân sự khác.");
        }

        var loiTrung = await KiemTraTrungHoSo.TimLoiTrungAsync(_coSoDuLieu, nhanSu.SoCCCD, nhanSu.SoDienThoai,
            null, boQuaId, cancellationToken);
        if (loiTrung is not null)
        {
            throw new InvalidOperationException(loiTrung);
        }

        if (nhanSu.KhoaId.HasValue && !await _coSoDuLieu.Khoas.AnyAsync(k => k.KhoaId == nhanSu.KhoaId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Khoa có ID {nhanSu.KhoaId.Value} không tồn tại.");
        }

        if (nhanSu.TrinhDoChuyenMonId.HasValue &&
            !await _coSoDuLieu.TrinhDoChuyenMons.AnyAsync(td => td.TrinhDoChuyenMonId == nhanSu.TrinhDoChuyenMonId.Value, cancellationToken))
        {
            throw new InvalidOperationException($"Trình độ chuyên môn có ID {nhanSu.TrinhDoChuyenMonId.Value} không tồn tại.");
        }

        if (nhanSu.ChucVuId.HasValue)
        {
            var chucVu = await _coSoDuLieu.ChucVus.AsNoTracking()
                .FirstOrDefaultAsync(cv => cv.ChucVuId == nhanSu.ChucVuId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Chức vụ có ID {nhanSu.ChucVuId.Value} không tồn tại.");
            await KiemTraChiTieuLanhDaoAsync(nhanSu, chucVu, boQuaId, cancellationToken);
        }
    }

    private async Task KiemTraChiTieuLanhDaoAsync(NhanSu nhanSu, ChucVu chucVu, int? boQuaId, CancellationToken cancellationToken)
    {
        if (!ChiTieuLanhDao.TryGetValue(chucVu.MaChucVu, out var chiTieu))
        {
            return;
        }

        var dangLamViec = nhanSu.TrangThai is not ("Đã nghỉ việc" or "Đã khóa");
        if (!dangLamViec)
        {
            return;
        }

        if (chiTieu.PhamVi == "Khoa" && !nhanSu.KhoaId.HasValue)
        {
            throw new InvalidOperationException($"Chức vụ {chucVu.TenChucVu} cần chọn khoa.");
        }

        if (chiTieu.PhamVi == "BoMon" && !nhanSu.BoMonId.HasValue)
        {
            throw new InvalidOperationException($"Chức vụ {chucVu.TenChucVu} cần chọn bộ môn.");
        }

        var truyVan = _coSoDuLieu.NhanSus.Where(ns =>
            ns.ChucVuId == chucVu.ChucVuId && ns.NhanSuId != boQuaId &&
            ns.TrangThai != "Đã nghỉ việc" && ns.TrangThai != "Đã khóa");
        truyVan = chiTieu.PhamVi == "Khoa"
            ? truyVan.Where(ns => ns.KhoaId == nhanSu.KhoaId)
            : truyVan.Where(ns => ns.BoMonId == nhanSu.BoMonId);

        var soNguoi = await truyVan.CountAsync(cancellationToken);
        if (soNguoi >= chiTieu.ToiDa)
        {
            var donVi = chiTieu.PhamVi == "Khoa" ? "Khoa" : "Bộ môn";
            throw new InvalidOperationException(
                $"{donVi} đã đủ {chiTieu.ToiDa} {chucVu.TenChucVu}. Hãy cập nhật hồ sơ đang giữ vị trí này trước.");
        }
    }

    private static string TenNguoiThucHien(NguoiDungHienTai nguoiDung, string? nguoiThucHien) =>
        nguoiDung.DaXacThuc && nguoiDung != NguoiDungHienTai.HeThong
            ? nguoiDung.TenDangNhap
            : nguoiThucHien ?? "Hệ thống";

    private static string MoTa(NhanSu ns) =>
        $"{ns.HoVaTen} | khoa {ns.KhoaId} | bộ môn {ns.BoMonId} | chức vụ {ns.ChucVuId} | {ns.TrangThai}";
}
