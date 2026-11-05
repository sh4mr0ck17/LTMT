using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Một lựa chọn phạm vi nhận thông báo.
/// </summary>
/// <param name="Ma">Mã phạm vi lưu trữ ("*", "khoa:1", "he:Chính quy", "khoahoc:D57", "lop:5").</param>
/// <param name="Ten">Nhãn hiển thị.</param>
/// <param name="Nhom">Nhóm hiển thị trong hộp chọn.</param>
public sealed record PhamViThongBao(string Ma, string Ten, string Nhom);

/// <summary>
/// Thông báo kèm trạng thái đọc của người dùng hiện tại.
/// </summary>
/// <param name="ThongBao">Thông báo (không kèm ảnh để giảm dung lượng danh sách).</param>
/// <param name="DaDoc">Người dùng hiện tại đã đọc.</param>
/// <param name="SoNguoiDoc">Tổng số người đã đọc.</param>
/// <param name="CoAnh">Có ảnh đính kèm.</param>
/// <param name="SoTep">Số tệp đính kèm.</param>
public sealed record DongThongBao(ThongBao ThongBao, bool DaDoc, int SoNguoiDoc, bool CoAnh, int SoTep = 0);

/// <summary>
/// Tệp mới tải lên kèm thông báo.
/// </summary>
/// <param name="TenTep">Tên tệp gốc.</param>
/// <param name="DuLieu">Nội dung tệp.</param>
public sealed record TepTaiLen(string TenTep, byte[] DuLieu);

/// <summary>
/// Thông tin tệp đính kèm (không kèm nội dung) để hiển thị danh sách tải về.
/// </summary>
/// <param name="Id">Khóa tệp.</param>
/// <param name="TenTep">Tên tệp.</param>
/// <param name="KieuNoiDung">MIME.</param>
/// <param name="KichThuoc">Dung lượng (byte).</param>
public sealed record ThongTinTep(int Id, string TenTep, string KieuNoiDung, long KichThuoc);

/// <summary>
/// Điều kiện lọc danh sách thông báo.
/// </summary>
public sealed class LocThongBao
{
    /// <summary>Từ khóa trong tiêu đề, nội dung, người đăng.</summary>
    public string? TuKhoa { get; set; }

    /// <summary>Chỉ thông báo gửi tới phạm vi này (kể cả thông báo toàn hệ thống).</summary>
    public string? PhamVi { get; set; }

    /// <summary>Mức độ.</summary>
    public string? MucDo { get; set; }

    /// <summary>Chỉ chưa đọc.</summary>
    public bool ChiChuaDoc { get; set; }

    /// <summary>Chỉ thông báo do tôi đăng.</summary>
    public bool ChiCuaToi { get; set; }

    /// <summary>Hiện cả thông báo đã hết hạn.</summary>
    public bool HienHetHan { get; set; }

    /// <summary>Loại văn bản (chuyên mục).</summary>
    public string? LoaiVanBan { get; set; }

    /// <summary>Năm đăng.</summary>
    public int? Nam { get; set; }

    /// <summary>Số thông báo tối đa.</summary>
    public int SoLuong { get; set; } = 200;
}

/// <summary>
/// Dịch vụ thông báo nội bộ: đăng theo phạm vi, mức độ, hạn hiệu lực, ảnh đính kèm, theo dõi đã đọc.
/// </summary>
public interface IThongBaoDichVu
{
    /// <summary>
    /// Lấy danh sách thông báo theo bộ lọc (ghim → khẩn → mới nhất), kèm trạng thái đọc.
    /// </summary>
    Task<List<DongThongBao>> LayDanhSachAsync(LocThongBao loc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy chi tiết một thông báo (kèm ảnh).
    /// </summary>
    Task<ThongBao?> LayChiTietAsync(int thongBaoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Danh sách người đã đọc (chỉ người đăng hoặc quản trị viên).
    /// </summary>
    Task<List<ThongBaoDaDoc>> LayNguoiDaDocAsync(int thongBaoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Danh sách tệp đính kèm (không kèm nội dung).
    /// </summary>
    Task<List<ThongTinTep>> LayDanhSachTepAsync(int thongBaoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy nội dung một tệp để tải về (người dùng phải đăng nhập).
    /// </summary>
    Task<TepDinhKemThongBao?> LayTepAsync(int tepId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm tệp đính kèm vào thông báo (người đăng hoặc quản trị viên).
    /// </summary>
    Task ThemTepAsync(int thongBaoId, IReadOnlyList<TepTaiLen> danhSachTep, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa một tệp đính kèm (người đăng hoặc quản trị viên).
    /// </summary>
    Task XoaTepAsync(int tepId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Danh mục phạm vi có thể chọn (toàn hệ thống, khoa, hệ, khóa, lớp).
    /// </summary>
    Task<List<PhamViThongBao>> LayDanhMucPhamViAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm hoặc cập nhật thông báo. Ảnh mới (nếu có) được kiểm tra chữ ký tệp.
    /// </summary>
    Task<ThongBao> LuuAsync(ThongBao banNhap, byte[]? anhMoi = null, bool xoaAnh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa thông báo.
    /// </summary>
    Task XoaAsync(int thongBaoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu người dùng hiện tại đã đọc thông báo.
    /// </summary>
    Task DanhDauDaDocAsync(int thongBaoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đánh dấu đã đọc toàn bộ thông báo còn hiệu lực.
    /// </summary>
    Task DanhDauTatCaDaDocAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Đếm thông báo còn hiệu lực mà tài khoản chưa đọc.
    /// </summary>
    Task<int> DemChuaDocAsync(string tenDangNhap, CancellationToken cancellationToken = default);

    /// <summary>
    /// Người dùng hiện tại có được đăng thông báo hay không.
    /// </summary>
    Task<bool> CoTheDangAsync();
}

/// <summary>
/// Cài đặt <see cref="IThongBaoDichVu"/>.
/// </summary>
public class ThongBaoDichVu : DichVuNghiepVuCoSo, IThongBaoDichVu
{
    /// <summary>Tên bảng dùng cho nhật ký và kênh thay đổi.</summary>
    public const string TenBang = "ThongBao";

    /// <summary>Kênh thay đổi trạng thái đọc (để cập nhật số chưa đọc trên menu).</summary>
    public const string TenBangDaDoc = "ThongBaoDaDoc";

    /// <summary>Dung lượng ảnh đính kèm tối đa (3 MB).</summary>
    public const int GioiHanAnh = 3 * 1024 * 1024;

    /// <summary>Độ dài tiêu đề tối đa.</summary>
    public const int DoDaiTieuDeToiDa = 200;

    /// <summary>Độ dài nội dung tối đa.</summary>
    public const int DoDaiNoiDungToiDa = 20000;

    private const string PhamViToanHeThong = "*";

    /// <summary>Cơ quan ban hành mặc định.</summary>
    public const string CoQuanMacDinh = "Học viện An ninh nhân dân";

    /// <summary>Dung lượng tối đa mỗi tệp đính kèm (10 MB).</summary>
    public const int GioiHanTep = 10 * 1024 * 1024;

    /// <summary>Số tệp đính kèm tối đa mỗi thông báo.</summary>
    public const int SoTepToiDa = 5;

    /// <summary>
    /// Khởi tạo dịch vụ thông báo.
    /// </summary>
    public ThongBaoDichVu(
        QuanLyDaoTaoDbContext coSoDuLieu,
        IPhanQuyenDichVu? phanQuyen = null,
        INhatKyDichVu? nhatKy = null,
        IBusThayDoiDuLieu? bus = null)
        : base(coSoDuLieu, phanQuyen, nhatKy, bus)
    {
    }

    /// <inheritdoc />
    public async Task<List<DongThongBao>> LayDanhSachAsync(LocThongBao loc, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loc);
        var nguoiDung = await LayNguoiDungAsync();
        var bayGio = DateTime.UtcNow;

        var truyVan = _coSoDuLieu.ThongBaos.AsNoTracking().AsQueryable();
        if (!loc.HienHetHan)
        {
            truyVan = truyVan.Where(tb => tb.HetHanLuc == null || tb.HetHanLuc > bayGio);
        }

        if (!string.IsNullOrWhiteSpace(loc.MucDo))
        {
            truyVan = truyVan.Where(tb => tb.MucDo == loc.MucDo);
        }

        if (loc.ChiCuaToi)
        {
            truyVan = truyVan.Where(tb => tb.NguoiDang == nguoiDung.TenDangNhap);
        }

        if (!string.IsNullOrWhiteSpace(loc.LoaiVanBan))
        {
            truyVan = truyVan.Where(tb => tb.LoaiVanBan == loc.LoaiVanBan);
        }

        if (loc.Nam.HasValue)
        {
            var dauNam = new DateTime(loc.Nam.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var cuoiNam = dauNam.AddYears(1);
            truyVan = truyVan.Where(tb => tb.NgayDang >= dauNam && tb.NgayDang < cuoiNam);
        }

        // Không tải ảnh trong danh sách; chi tiết tải riêng.
        var danhSach = await truyVan
            .OrderByDescending(tb => tb.DaGhim)
            .ThenByDescending(tb => tb.NgayDang)
            .Take(Math.Clamp(loc.SoLuong, 1, 1000))
            .Select(tb => new
            {
                ThongBao = new ThongBao
                {
                    ThongBaoId = tb.ThongBaoId, TieuDe = tb.TieuDe, NoiDung = tb.NoiDung, PhamVi = tb.PhamVi, DaGhim = tb.DaGhim,
                    MucDo = tb.MucDo, HetHanLuc = tb.HetHanLuc, NguoiDang = tb.NguoiDang, NgayDang = tb.NgayDang, NgaySua = tb.NgaySua,
                    LoaiVanBan = tb.LoaiVanBan, SoKyHieu = tb.SoKyHieu, CoQuanBanHanh = tb.CoQuanBanHanh, NguoiKy = tb.NguoiKy,
                    NgayBanHanh = tb.NgayBanHanh, TrichYeu = tb.TrichYeu
                },
                CoAnh = tb.AnhDinhKem != null,
                SoTep = tb.TepDinhKem.Count,
                SoNguoiDoc = tb.LuotDoc.Count,
                DaDoc = tb.LuotDoc.Any(d => d.TenDangNhap == nguoiDung.TenDangNhap)
            })
            .ToListAsync(cancellationToken);

        return danhSach
            .Where(x => string.IsNullOrWhiteSpace(loc.PhamVi) || loc.PhamVi == PhamViToanHeThong ||
                        x.ThongBao.DanhSachPhamVi.Contains(PhamViToanHeThong) || x.ThongBao.DanhSachPhamVi.Contains(loc.PhamVi))
            .Where(x => !loc.ChiChuaDoc || !x.DaDoc)
            .Where(x => QuyTacNghiepVu.ChuaTuKhoa(x.ThongBao.TieuDe, loc.TuKhoa) || QuyTacNghiepVu.ChuaTuKhoa(x.ThongBao.NoiDung, loc.TuKhoa) ||
                        QuyTacNghiepVu.ChuaTuKhoa(x.ThongBao.NguoiDang, loc.TuKhoa) || QuyTacNghiepVu.ChuaTuKhoa(x.ThongBao.SoKyHieu, loc.TuKhoa) ||
                        QuyTacNghiepVu.ChuaTuKhoa(x.ThongBao.TrichYeu, loc.TuKhoa))
            // Ghim trước, rồi mức độ cao trước trong cùng nhóm ghim, cuối cùng là mới nhất.
            .OrderByDescending(x => x.ThongBao.DaGhim)
            .ThenByDescending(x => MucDoThongBao.TatCa.ToList().IndexOf(x.ThongBao.MucDo))
            .ThenByDescending(x => x.ThongBao.NgayDang)
            .Select(x => new DongThongBao(x.ThongBao, x.DaDoc, x.SoNguoiDoc, x.CoAnh, x.SoTep))
            .ToList();
    }

    /// <inheritdoc />
    public Task<ThongBao?> LayChiTietAsync(int thongBaoId, CancellationToken cancellationToken = default) =>
        _coSoDuLieu.ThongBaos.AsNoTracking().FirstOrDefaultAsync(tb => tb.ThongBaoId == thongBaoId, cancellationToken);

    /// <inheritdoc />
    public async Task<List<ThongBaoDaDoc>> LayNguoiDaDocAsync(int thongBaoId, CancellationToken cancellationToken = default)
    {
        var nguoiDung = await LayNguoiDungAsync();
        var nguoiDang = await _coSoDuLieu.ThongBaos.Where(tb => tb.ThongBaoId == thongBaoId).Select(tb => tb.NguoiDang).FirstOrDefaultAsync(cancellationToken);
        if (!nguoiDung.LaQuanTri && nguoiDang != nguoiDung.TenDangNhap)
        {
            throw new UnauthorizedAccessException("Chỉ người đăng hoặc quản trị viên được xem danh sách người đã đọc.");
        }

        return await _coSoDuLieu.ThongBaoDaDocs.AsNoTracking()
            .Where(d => d.ThongBaoId == thongBaoId)
            .OrderByDescending(d => d.DocLuc)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<PhamViThongBao>> LayDanhMucPhamViAsync(CancellationToken cancellationToken = default)
    {
        var ketQua = new List<PhamViThongBao> { new(PhamViToanHeThong, "Toàn hệ thống", "Chung") };

        var khoas = await _coSoDuLieu.Khoas.AsNoTracking().OrderBy(k => k.TenKhoa).ToListAsync(cancellationToken);
        ketQua.AddRange(khoas.Select(k => new PhamViThongBao($"khoa:{k.KhoaId}", k.TenKhoa, "Khoa")));

        var lops = await _coSoDuLieu.LopHocs.AsNoTracking().OrderBy(l => l.MaLop).ToListAsync(cancellationToken);
        ketQua.AddRange(lops.Select(l => l.HeDaoTao).Distinct().Order()
            .Select(he => new PhamViThongBao($"he:{he}", $"Hệ {he}", "Hệ đào tạo")));
        ketQua.AddRange(lops.Where(l => l.KhoaHoc != null).Select(l => l.KhoaHoc!).Distinct().OrderDescending()
            .Select(kh => new PhamViThongBao($"khoahoc:{kh}", $"Khóa {kh}", "Khóa")));
        ketQua.AddRange(lops.Select(l => new PhamViThongBao($"lop:{l.LopHocId}", $"Lớp {l.MaLop}", "Lớp")));
        return ketQua;
    }

    /// <inheritdoc />
    public async Task<bool> CoTheDangAsync()
    {
        var nguoiDung = await LayNguoiDungAsync();
        return nguoiDung.LaQuanTri || nguoiDung.VaiTro == VaiTroMacDinh.QuanLy;
    }

    /// <inheritdoc />
    public async Task<ThongBao> LuuAsync(ThongBao banNhap, byte[]? anhMoi = null, bool xoaAnh = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(banNhap);
        var nguoiDung = await LayNguoiDungAsync();
        if (!nguoiDung.LaQuanTri && nguoiDung.VaiTro != VaiTroMacDinh.QuanLy)
        {
            throw new UnauthorizedAccessException("Chỉ quản trị viên và quản lý được đăng thông báo.");
        }

        var tieuDe = QuyTacNghiepVu.ChuanHoaChuoi(banNhap.TieuDe);
        var noiDung = (banNhap.NoiDung ?? string.Empty).Trim();
        if (tieuDe.Length is 0 or > DoDaiTieuDeToiDa || noiDung.Length is 0 or > DoDaiNoiDungToiDa)
        {
            throw new InvalidOperationException($"Tiêu đề cần 1–{DoDaiTieuDeToiDa} ký tự; nội dung cần 1–{DoDaiNoiDungToiDa:N0} ký tự.");
        }

        if (!MucDoThongBao.TatCa.Contains(banNhap.MucDo))
        {
            throw new InvalidOperationException("Mức độ thông báo không hợp lệ.");
        }

        if (!LoaiVanBanThongBao.TatCa.Contains(banNhap.LoaiVanBan))
        {
            throw new InvalidOperationException("Loại văn bản không hợp lệ.");
        }

        if (banNhap.HetHanLuc.HasValue && banNhap.HetHanLuc.Value <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Hạn hiển thị phải ở tương lai.");
        }

        var danhMuc = (await LayDanhMucPhamViAsync(cancellationToken)).Select(p => p.Ma).ToHashSet();
        var phamVi = banNhap.DanhSachPhamVi.Distinct().ToList();
        if (phamVi.Count == 0)
        {
            phamVi.Add(PhamViToanHeThong);
        }

        if (phamVi.Any(p => !danhMuc.Contains(p)))
        {
            throw new InvalidOperationException("Phạm vi nhận không còn tồn tại. Vui lòng chọn lại.");
        }

        if (!nguoiDung.LaQuanTri && phamVi.Contains(PhamViToanHeThong))
        {
            throw new UnauthorizedAccessException("Chỉ quản trị viên được gửi thông báo toàn hệ thống.");
        }

        var anh = anhMoi is null ? null : TaoAnh(anhMoi);

        ThongBao banGhi;
        var laMoi = banNhap.ThongBaoId == 0;
        if (laMoi)
        {
            banGhi = new ThongBao { NguoiDang = nguoiDung.TenDangNhap, NgayDang = DateTime.UtcNow };
            _coSoDuLieu.ThongBaos.Add(banGhi);
        }
        else
        {
            banGhi = await _coSoDuLieu.ThongBaos.FirstOrDefaultAsync(tb => tb.ThongBaoId == banNhap.ThongBaoId, cancellationToken)
                ?? throw new KeyNotFoundException("Thông báo đã bị xóa. Vui lòng tải lại.");
            if (!nguoiDung.LaQuanTri && banGhi.NguoiDang != nguoiDung.TenDangNhap)
            {
                throw new UnauthorizedAccessException("Chỉ người đăng hoặc quản trị viên được sửa thông báo này.");
            }

            banGhi.NgaySua = DateTime.UtcNow;
        }

        banGhi.TieuDe = tieuDe;
        banGhi.NoiDung = noiDung;
        banGhi.PhamVi = string.Join(';', phamVi);
        banGhi.MucDo = banNhap.MucDo;
        banGhi.HetHanLuc = banNhap.HetHanLuc;
        banGhi.DaGhim = nguoiDung.LaQuanTri && banNhap.DaGhim;
        banGhi.LoaiVanBan = banNhap.LoaiVanBan;
        banGhi.SoKyHieu = string.IsNullOrWhiteSpace(banNhap.SoKyHieu) ? null : banNhap.SoKyHieu.Trim();
        banGhi.CoQuanBanHanh = string.IsNullOrWhiteSpace(banNhap.CoQuanBanHanh) ? CoQuanMacDinh : banNhap.CoQuanBanHanh.Trim();
        banGhi.NguoiKy = string.IsNullOrWhiteSpace(banNhap.NguoiKy) ? null : banNhap.NguoiKy.Trim();
        banGhi.NgayBanHanh = banNhap.NgayBanHanh ?? banGhi.NgayDang;
        banGhi.TrichYeu = string.IsNullOrWhiteSpace(banNhap.TrichYeu) ? null : QuyTacNghiepVu.ChuanHoaChuoi(banNhap.TrichYeu);
        if (xoaAnh)
        {
            banGhi.AnhDinhKem = null;
        }

        if (anh is not null)
        {
            banGhi.AnhDinhKem = anh;
        }

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);

        await HoanTatThaoTacAsync(nguoiDung, laMoi ? "Thêm mới" : "Cập nhật", TenBang,
            banGhi.ThongBaoId.ToString(System.Globalization.CultureInfo.InvariantCulture), null, $"[{banGhi.MucDo}] {tieuDe}", cancellationToken);
        return banGhi;
    }

    /// <inheritdoc />
    public async Task XoaAsync(int thongBaoId, CancellationToken cancellationToken = default)
    {
        var nguoiDung = await LayNguoiDungAsync();
        var banGhi = await _coSoDuLieu.ThongBaos.FirstOrDefaultAsync(tb => tb.ThongBaoId == thongBaoId, cancellationToken)
            ?? throw new KeyNotFoundException("Thông báo đã bị xóa.");
        if (!nguoiDung.LaQuanTri && banGhi.NguoiDang != nguoiDung.TenDangNhap)
        {
            throw new UnauthorizedAccessException("Chỉ người đăng hoặc quản trị viên được xóa thông báo này.");
        }

        _coSoDuLieu.ThongBaos.Remove(banGhi);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        await HoanTatThaoTacAsync(nguoiDung, "Xóa", TenBang,
            thongBaoId.ToString(System.Globalization.CultureInfo.InvariantCulture), banGhi.TieuDe, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DanhDauDaDocAsync(int thongBaoId, CancellationToken cancellationToken = default)
    {
        var nguoiDung = await LayNguoiDungAsync();
        if (!nguoiDung.DaXacThuc)
        {
            return;
        }

        var daCo = await _coSoDuLieu.ThongBaoDaDocs.AnyAsync(d => d.ThongBaoId == thongBaoId && d.TenDangNhap == nguoiDung.TenDangNhap, cancellationToken);
        if (daCo || !await _coSoDuLieu.ThongBaos.AnyAsync(tb => tb.ThongBaoId == thongBaoId, cancellationToken))
        {
            return;
        }

        _coSoDuLieu.ThongBaoDaDocs.Add(new ThongBaoDaDoc { ThongBaoId = thongBaoId, TenDangNhap = nguoiDung.TenDangNhap });
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        await HoanTatThaoTacAsync(nguoiDung, "Đọc", TenBangDaDoc, null, null, null, cancellationToken, ghiNhatKy: false);
    }

    /// <inheritdoc />
    public async Task DanhDauTatCaDaDocAsync(CancellationToken cancellationToken = default)
    {
        var nguoiDung = await LayNguoiDungAsync();
        if (!nguoiDung.DaXacThuc)
        {
            return;
        }

        var bayGio = DateTime.UtcNow;
        var chuaDoc = await _coSoDuLieu.ThongBaos
            .Where(tb => (tb.HetHanLuc == null || tb.HetHanLuc > bayGio) && !tb.LuotDoc.Any(d => d.TenDangNhap == nguoiDung.TenDangNhap))
            .Select(tb => tb.ThongBaoId)
            .ToListAsync(cancellationToken);
        _coSoDuLieu.ThongBaoDaDocs.AddRange(chuaDoc.Select(id => new ThongBaoDaDoc { ThongBaoId = id, TenDangNhap = nguoiDung.TenDangNhap }));
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        await HoanTatThaoTacAsync(nguoiDung, "Đọc", TenBangDaDoc, null, null, null, cancellationToken, ghiNhatKy: false);
    }

    /// <inheritdoc />
    public Task<int> DemChuaDocAsync(string tenDangNhap, CancellationToken cancellationToken = default)
    {
        var bayGio = DateTime.UtcNow;
        return _coSoDuLieu.ThongBaos.CountAsync(tb =>
            (tb.HetHanLuc == null || tb.HetHanLuc > bayGio) && tb.NguoiDang != tenDangNhap &&
            !tb.LuotDoc.Any(d => d.TenDangNhap == tenDangNhap), cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<ThongTinTep>> LayDanhSachTepAsync(int thongBaoId, CancellationToken cancellationToken = default) =>
        _coSoDuLieu.TepDinhKemThongBaos.AsNoTracking()
            .Where(t => t.ThongBaoId == thongBaoId)
            .OrderBy(t => t.TepDinhKemThongBaoId)
            .Select(t => new ThongTinTep(t.TepDinhKemThongBaoId, t.TenTep, t.KieuNoiDung, t.KichThuoc))
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<TepDinhKemThongBao?> LayTepAsync(int tepId, CancellationToken cancellationToken = default)
    {
        var nguoiDung = await LayNguoiDungAsync();
        if (!nguoiDung.DaXacThuc)
        {
            throw new UnauthorizedAccessException("Cần đăng nhập để tải tệp.");
        }

        return await _coSoDuLieu.TepDinhKemThongBaos.AsNoTracking().FirstOrDefaultAsync(t => t.TepDinhKemThongBaoId == tepId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ThemTepAsync(int thongBaoId, IReadOnlyList<TepTaiLen> danhSachTep, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(danhSachTep);
        if (danhSachTep.Count == 0)
        {
            return;
        }

        var nguoiDung = await DamBaoQuyenSuaAsync(thongBaoId, cancellationToken);
        var soTepHienCo = await _coSoDuLieu.TepDinhKemThongBaos.CountAsync(t => t.ThongBaoId == thongBaoId, cancellationToken);
        if (soTepHienCo + danhSachTep.Count > SoTepToiDa)
        {
            throw new InvalidOperationException($"Mỗi thông báo đính kèm tối đa {SoTepToiDa} tệp.");
        }

        foreach (var tep in danhSachTep)
        {
            var kieu = XacDinhKieuTep(tep.TenTep, tep.DuLieu);
            _coSoDuLieu.TepDinhKemThongBaos.Add(new TepDinhKemThongBao
            {
                ThongBaoId = thongBaoId,
                TenTep = LamSachTenTep(tep.TenTep),
                KieuNoiDung = kieu,
                KichThuoc = tep.DuLieu.LongLength,
                DuLieu = tep.DuLieu
            });
        }

        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        await HoanTatThaoTacAsync(nguoiDung, "Đính kèm tệp", TenBang, thongBaoId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            null, string.Join(", ", danhSachTep.Select(t => t.TenTep)), cancellationToken);
    }

    /// <inheritdoc />
    public async Task XoaTepAsync(int tepId, CancellationToken cancellationToken = default)
    {
        var tep = await _coSoDuLieu.TepDinhKemThongBaos.FirstOrDefaultAsync(t => t.TepDinhKemThongBaoId == tepId, cancellationToken)
            ?? throw new KeyNotFoundException("Tệp đã bị xóa.");
        var nguoiDung = await DamBaoQuyenSuaAsync(tep.ThongBaoId, cancellationToken);
        _coSoDuLieu.TepDinhKemThongBaos.Remove(tep);
        await _coSoDuLieu.SaveChangesAsync(cancellationToken);
        await HoanTatThaoTacAsync(nguoiDung, "Xóa tệp đính kèm", TenBang, tep.ThongBaoId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            tep.TenTep, null, cancellationToken);
    }

    /// <summary>
    /// Xác định MIME từ chữ ký nhị phân và phần mở rộng hợp lệ; từ chối tệp thực thi hoặc giả mạo đuôi.
    /// </summary>
    public static string XacDinhKieuTep(string tenTep, byte[] duLieu)
    {
        ArgumentNullException.ThrowIfNull(duLieu);
        if (duLieu.Length is 0 or > GioiHanTep)
        {
            throw new InvalidOperationException($"Tệp {tenTep} rỗng hoặc vượt quá 10 MB.");
        }

        var duoi = Path.GetExtension(tenTep ?? string.Empty).ToLowerInvariant();
        static bool BatDau(byte[] d, params byte[] chuKy) => d.Length >= chuKy.Length && d.AsSpan(0, chuKy.Length).SequenceEqual(chuKy);
        var laZip = BatDau(duLieu, 0x50, 0x4B, 0x03, 0x04);
        var laOle = BatDau(duLieu, 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1);

        return duoi switch
        {
            ".pdf" when BatDau(duLieu, 0x25, 0x50, 0x44, 0x46) => "application/pdf",
            ".docx" when laZip => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xlsx" when laZip => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".pptx" when laZip => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".doc" when laOle => "application/msword",
            ".xls" when laOle => "application/vnd.ms-excel",
            ".png" or ".jpg" or ".jpeg" or ".webp" => LayMimeAnh(duLieu),
            _ => throw new InvalidOperationException($"Tệp {tenTep}: chỉ chấp nhận PDF, Word, Excel, PowerPoint hoặc ảnh, và nội dung phải khớp đuôi tệp.")
        };
    }

    private static string LayMimeAnh(byte[] duLieu)
    {
        var uri = TaoAnh(duLieu);
        return uri[5..uri.IndexOf(';')];
    }

    private static string LamSachTenTep(string tenTep)
    {
        var ten = Path.GetFileName(tenTep ?? "tep");
        var khongHopLe = Path.GetInvalidFileNameChars();
        ten = new string(ten.Select(c => khongHopLe.Contains(c) ? '_' : c).ToArray()).Trim();
        const int DoDaiTenToiDa = 200;
        return ten.Length == 0 ? "tep" : ten.Length > DoDaiTenToiDa ? ten[^DoDaiTenToiDa..] : ten;
    }

    private async Task<NguoiDungHienTai> DamBaoQuyenSuaAsync(int thongBaoId, CancellationToken cancellationToken)
    {
        var nguoiDung = await LayNguoiDungAsync();
        var nguoiDang = await _coSoDuLieu.ThongBaos.Where(tb => tb.ThongBaoId == thongBaoId).Select(tb => tb.NguoiDang).FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Thông báo đã bị xóa.");
        if (!nguoiDung.LaQuanTri && nguoiDang != nguoiDung.TenDangNhap)
        {
            throw new UnauthorizedAccessException("Chỉ người đăng hoặc quản trị viên được thay đổi tệp đính kèm.");
        }

        return nguoiDung;
    }

    /// <summary>
    /// Kiểm tra chữ ký tệp ảnh (không tin phần mở rộng/MIME của trình duyệt) và trả về data URI.
    /// </summary>
    public static string TaoAnh(byte[] anh)
    {
        ArgumentNullException.ThrowIfNull(anh);
        if (anh.Length is 0 or > GioiHanAnh)
        {
            throw new InvalidOperationException("Ảnh phải có dung lượng không quá 3 MB.");
        }

        byte[] chuKyPng = [137, 80, 78, 71, 13, 10, 26, 10];
        string mime;
        if (anh.Length >= chuKyPng.Length && anh.AsSpan(0, chuKyPng.Length).SequenceEqual(chuKyPng))
        {
            mime = "image/png";
        }
        else if (anh.Length >= 3 && anh[0] == 0xFF && anh[1] == 0xD8 && anh[2] == 0xFF)
        {
            mime = "image/jpeg";
        }
        else if (anh.Length >= 12 && System.Text.Encoding.ASCII.GetString(anh, 0, 4) == "RIFF" &&
                 System.Text.Encoding.ASCII.GetString(anh, 8, 4) == "WEBP")
        {
            mime = "image/webp";
        }
        else
        {
            throw new InvalidOperationException("Chỉ chấp nhận ảnh PNG, JPEG hoặc WebP.");
        }

        return $"data:{mime};base64,{Convert.ToBase64String(anh)}";
    }
}
