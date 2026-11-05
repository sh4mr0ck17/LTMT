using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Dịch vụ nghiệp vụ Học viên: kiểm tra quyền theo vùng Đào tạo, ràng buộc định danh,
/// chỉ tiêu chức vụ trong lớp, sĩ số tối đa và che thông tin cá nhân khi đọc.
/// </summary>
public class HocVienDichVu : DichVuNghiepVuCoSo, IHocVienDichVu
{
    private const string TenBang = "HocVien";

    /// <summary>
    /// Khởi tạo dịch vụ học viên.
    /// </summary>
    public HocVienDichVu(
        QuanLyDaoTaoDbContext coSoDuLieu,
        IPhanQuyenDichVu? phanQuyen = null,
        INhatKyDichVu? nhatKy = null,
        IBusThayDoiDuLieu? bus = null)
        : base(coSoDuLieu, phanQuyen, nhatKy, bus)
    {
    }

    /// <summary>
    /// Lấy danh sách học viên có lọc theo lớp học, trạng thái và tìm kiếm.
    /// Thông tin cá nhân bị che nếu người dùng không quản lý vùng Đào tạo của khoa tương ứng.
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
                (hv.ChucVuLop != null && hv.ChucVuLop.Contains(tuKhoaChuan)) ||
                (hv.Email != null && hv.Email.Contains(tuKhoaChuan)) ||
                (hv.SoDienThoai != null && hv.SoDienThoai.Contains(tuKhoaChuan)) ||
                (hv.DiaChi != null && hv.DiaChi.Contains(tuKhoaChuan)));
        }

        if (coHoatDong.HasValue)
        {
            truyVan = truyVan.Where(hv => hv.TrangThai == (coHoatDong.Value ? "Đang học" : "Thôi học"));
        }

        var danhSach = await truyVan.OrderBy(hv => hv.MaHocVien).ToListAsync(cancellationToken);
        var nguoiDung = await LayNguoiDungAsync();
        foreach (var hocVien in danhSach)
        {
            if (!nguoiDung.CoTheXemThongTinCaNhan(VungQuanLy.DaoTao, hocVien.LopHoc?.KhoaId))
            {
                CheThongTinCaNhan(hocVien);
            }
        }

        return danhSach;
    }

    /// <summary>
    /// Lấy chi tiết học viên theo ID (bản ghi được theo dõi để cập nhật, không che thông tin).
    /// </summary>
    public async Task<HocVien?> LayTheoIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _coSoDuLieu.HocViens
            .Include(hv => hv.LopHoc)
                .ThenInclude(l => l!.Khoa)
            .FirstOrDefaultAsync(hv => hv.HocVienId == id, cancellationToken);
    }

    /// <summary>
    /// Lấy chi tiết học viên theo mã.
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
    /// Thêm mới học viên sau khi kiểm tra quyền và toàn bộ ràng buộc nghiệp vụ.
    /// </summary>
    public async Task<HocVien> ThemHocVienAsync(HocVien hocVien, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(hocVien);
        ChuanHoa(hocVien);

        var lop = await LayLopBatBuocAsync(hocVien.LopHocId, cancellationToken);
        var nguoiDung = await DamBaoQuyenGhiAsync(VungQuanLy.DaoTao, lop.KhoaId);

        if (await KiemTraTonTaiMaAsync(hocVien.MaHocVien, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã học viên '{hocVien.MaHocVien}' đã tồn tại trong hệ thống.");
        }

        await KiemTraRangBuocAsync(hocVien, lop, null, cancellationToken);

        hocVien.NgayTao = DateTime.UtcNow;
        hocVien.NgayCapNhat = null;
        await _coSoDuLieu.HocViens.AddAsync(hocVien, cancellationToken);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await HoanTatThaoTacAsync(nguoiDung, "Thêm mới", TenBang, hocVien.MaHocVien, null,
            $"{hocVien.HoVaTen} - lớp {lop.MaLop}", cancellationToken);
        return hocVien;
    }

    /// <summary>
    /// Cập nhật học viên (kể cả chuyển lớp). Người dùng phải có quyền với cả lớp cũ và lớp mới.
    /// </summary>
    public async Task<HocVien> CapNhatHocVienAsync(HocVien hocVien, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(hocVien);
        ChuanHoa(hocVien);

        var hocVienTonTai = await _coSoDuLieu.HocViens
            .Include(hv => hv.LopHoc)
            .FirstOrDefaultAsync(hv => hv.HocVienId == hocVien.HocVienId, cancellationToken);
        if (hocVienTonTai == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy học viên có ID {hocVien.HocVienId}.");
        }

        var lopMoi = await LayLopBatBuocAsync(hocVien.LopHocId, cancellationToken);
        var nguoiDung = await DamBaoQuyenGhiAsync(VungQuanLy.DaoTao, hocVienTonTai.LopHoc?.KhoaId);
        if (lopMoi.LopHocId != hocVienTonTai.LopHocId)
        {
            await DamBaoQuyenGhiAsync(VungQuanLy.DaoTao, lopMoi.KhoaId);
        }

        if (await KiemTraTonTaiMaAsync(hocVien.MaHocVien, hocVien.HocVienId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã học viên '{hocVien.MaHocVien}' đã tồn tại ở bản ghi khác.");
        }

        await KiemTraRangBuocAsync(hocVien, lopMoi, hocVienTonTai, cancellationToken);

        var giaTriCu = MoTa(hocVienTonTai, hocVienTonTai.LopHoc?.MaLop);
        hocVienTonTai.MaHocVien = hocVien.MaHocVien;
        hocVienTonTai.HoVaTen = hocVien.HoVaTen;
        hocVienTonTai.NgaySinh = hocVien.NgaySinh;
        hocVienTonTai.GioiTinh = hocVien.GioiTinh;
        hocVienTonTai.Email = hocVien.Email;
        hocVienTonTai.SoDienThoai = hocVien.SoDienThoai;
        hocVienTonTai.SoCCCD = hocVien.SoCCCD;
        hocVienTonTai.ChucVuLop = hocVien.ChucVuLop;
        hocVienTonTai.DiaChi = hocVien.DiaChi;
        hocVienTonTai.LopHocId = hocVien.LopHocId;
        hocVienTonTai.TrangThai = hocVien.TrangThai;
        hocVienTonTai.NgayCapNhat = DateTime.UtcNow;

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await HoanTatThaoTacAsync(nguoiDung, "Cập nhật", TenBang, hocVienTonTai.MaHocVien, giaTriCu,
            MoTa(hocVienTonTai, lopMoi.MaLop), cancellationToken);
        return hocVienTonTai;
    }

    /// <summary>
    /// Xóa học viên.
    /// </summary>
    public async Task<bool> XoaHocVienAsync(int id, CancellationToken cancellationToken = default)
    {
        var hocVien = await _coSoDuLieu.HocViens
            .Include(hv => hv.LopHoc)
            .FirstOrDefaultAsync(hv => hv.HocVienId == id, cancellationToken);
        if (hocVien == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy học viên có ID {id}.");
        }

        var nguoiDung = await DamBaoQuyenGhiAsync(VungQuanLy.DaoTao, hocVien.LopHoc?.KhoaId);
        var giaTriCu = MoTa(hocVien, hocVien.LopHoc?.MaLop);

        _coSoDuLieu.HocViens.Remove(hocVien);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await HoanTatThaoTacAsync(nguoiDung, "Xóa", TenBang, hocVien.MaHocVien, giaTriCu, null, cancellationToken);
        return true;
    }

    /// <summary>
    /// Kiểm tra trùng mã học viên.
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

    /// <summary>
    /// Che thông tin cá nhân trên bản sao không theo dõi: CCCD, SĐT giữ 3 số cuối, ẩn email, địa chỉ chỉ còn tỉnh/thành.
    /// </summary>
    public static void CheThongTinCaNhan(HocVien hocVien)
    {
        ArgumentNullException.ThrowIfNull(hocVien);
        hocVien.SoCCCD = QuyTacNghiepVu.CheThongTin(hocVien.SoCCCD);
        hocVien.SoDienThoai = QuyTacNghiepVu.CheThongTin(hocVien.SoDienThoai);
        hocVien.Email = hocVien.Email is null ? null : "•••@•••";
        if (!string.IsNullOrEmpty(hocVien.DiaChi))
        {
            var phan = hocVien.DiaChi.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            hocVien.DiaChi = phan.Length == 0 ? null : phan[^1];
        }
    }

    private static void ChuanHoa(HocVien hocVien)
    {
        hocVien.MaHocVien = (hocVien.MaHocVien ?? string.Empty).Trim();
        hocVien.HoVaTen = QuyTacNghiepVu.ChuanHoaChuoi(hocVien.HoVaTen);
        hocVien.SoCCCD = string.IsNullOrWhiteSpace(hocVien.SoCCCD) ? null : hocVien.SoCCCD.Trim();
        hocVien.SoDienThoai = string.IsNullOrWhiteSpace(hocVien.SoDienThoai) ? null : hocVien.SoDienThoai.Trim();
        hocVien.Email = string.IsNullOrWhiteSpace(hocVien.Email) ? null : hocVien.Email.Trim();
        hocVien.DiaChi = string.IsNullOrWhiteSpace(hocVien.DiaChi) ? null : QuyTacNghiepVu.ChuanHoaChuoi(hocVien.DiaChi);
        hocVien.ChucVuLop = QuyTacNghiepVu.GhepChucVuLop(QuyTacNghiepVu.TachChucVuLop(hocVien.ChucVuLop));
    }

    private async Task<LopHoc> LayLopBatBuocAsync(int lopHocId, CancellationToken cancellationToken)
    {
        return await _coSoDuLieu.LopHocs.AsNoTracking().FirstOrDefaultAsync(l => l.LopHocId == lopHocId, cancellationToken)
            ?? throw new InvalidOperationException($"Lớp học có ID {lopHocId} không tồn tại.");
    }

    private async Task KiemTraRangBuocAsync(HocVien hocVien, LopHoc lop, HocVien? banGhiCu, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(hocVien.HoVaTen))
        {
            throw new InvalidOperationException("Họ và tên không được để trống.");
        }

        if (!QuyTacNghiepVu.LaNgaySinhHopLe(hocVien.NgaySinh))
        {
            throw new InvalidOperationException($"Ngày sinh phải từ năm {QuyTacNghiepVu.NamSinhToiThieu} đến hôm nay.");
        }

        if (!QuyTacNghiepVu.DanhSachGioiTinh.Contains(hocVien.GioiTinh))
        {
            throw new InvalidOperationException("Giới tính chỉ nhận Nam hoặc Nữ.");
        }

        if (!QuyTacNghiepVu.DanhSachTrangThaiHocVien.Contains(hocVien.TrangThai))
        {
            throw new InvalidOperationException($"Trạng thái học tập phải là một trong: {string.Join(", ", QuyTacNghiepVu.DanhSachTrangThaiHocVien)}.");
        }

        if (hocVien.SoCCCD is not null && !QuyTacNghiepVu.LaCccdHopLe(hocVien.SoCCCD))
        {
            throw new InvalidOperationException("Số CCCD phải gồm đúng 12 chữ số.");
        }

        if (hocVien.SoDienThoai is not null && !QuyTacNghiepVu.LaSoDienThoaiHopLe(hocVien.SoDienThoai))
        {
            throw new InvalidOperationException("Số điện thoại phải có 10 chữ số và đầu số di động hợp lệ (03, 05, 07, 08, 09).");
        }

        var loiTrung = await KiemTraTrungHoSo.TimLoiTrungAsync(_coSoDuLieu, hocVien.SoCCCD, hocVien.SoDienThoai,
            banGhiCu?.HocVienId, null, cancellationToken);
        if (loiTrung is not null)
        {
            throw new InvalidOperationException(loiTrung);
        }

        // Sĩ số luôn tính từ dữ liệu thực, chỉ chặn khi thêm mới hoặc chuyển vào lớp đã đầy.
        if (banGhiCu is null || banGhiCu.LopHocId != lop.LopHocId)
        {
            var siSo = await _coSoDuLieu.HocViens.CountAsync(hv => hv.LopHocId == lop.LopHocId, cancellationToken);
            if (siSo >= lop.SoLuongToiDa)
            {
                throw new InvalidOperationException($"Lớp {lop.MaLop} đã đủ sĩ số tối đa {lop.SoLuongToiDa} học viên.");
            }
        }

        await KiemTraChiTieuChucVuAsync(hocVien, lop, banGhiCu, cancellationToken);
    }

    private async Task KiemTraChiTieuChucVuAsync(HocVien hocVien, LopHoc lop, HocVien? banGhiCu, CancellationToken cancellationToken)
    {
        var chucVuMoi = QuyTacNghiepVu.TachChucVuLop(hocVien.ChucVuLop);
        if (chucVuMoi.Count == 0)
        {
            return;
        }

        // Chỉ kiểm tra chức vụ mới được gán (hoặc khi chuyển lớp) để không chặn sửa hồ sơ cũ đã vượt chỉ tiêu.
        var chucVuCu = banGhiCu is not null && banGhiCu.LopHocId == lop.LopHocId
            ? QuyTacNghiepVu.TachChucVuLop(banGhiCu.ChucVuLop)
            : [];
        var canKiemTra = chucVuMoi.Except(chucVuCu)
            .Where(cv => QuyTacNghiepVu.ChiTieuChucVuLop.TryGetValue(cv, out var chiTieu) && chiTieu.HasValue)
            .ToList();
        if (canKiemTra.Count == 0)
        {
            return;
        }

        var chucVuTrongLop = await _coSoDuLieu.HocViens.AsNoTracking()
            .Where(hv => hv.LopHocId == lop.LopHocId && hv.ChucVuLop != null && hv.HocVienId != hocVien.HocVienId)
            .Select(hv => hv.ChucVuLop)
            .ToListAsync(cancellationToken);

        foreach (var chucVu in canKiemTra)
        {
            var chiTieu = QuyTacNghiepVu.ChiTieuChucVuLop[chucVu]!.Value;
            var soNguoi = chucVuTrongLop.Count(x => QuyTacNghiepVu.TachChucVuLop(x).Contains(chucVu));
            if (soNguoi >= chiTieu)
            {
                var ten = QuyTacNghiepVu.TenDayDuChucVuLop.GetValueOrDefault(chucVu, chucVu);
                throw new InvalidOperationException($"Lớp {lop.MaLop} đã đủ {chiTieu} {ten} ({chucVu}).");
            }
        }
    }

    private static string MoTa(HocVien hocVien, string? maLop) =>
        $"{hocVien.HoVaTen} | lớp {maLop} | chức vụ {hocVien.ChucVuLop ?? "-"} | {hocVien.TrangThai}";
}
