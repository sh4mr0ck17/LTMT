using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Dòng hiển thị một tài khoản trên trang quản trị.
/// </summary>
public sealed class DongTaiKhoan
{
    /// <summary>Id tài khoản.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Email đăng nhập.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Họ tên.</summary>
    public string HoVaTen { get; init; } = string.Empty;

    /// <summary>Vai trò hiện tại.</summary>
    public string VaiTro { get; init; } = VaiTroMacDinh.NguoiXem;

    /// <summary>Vùng quản lý.</summary>
    public string? VungQuanLy { get; init; }

    /// <summary>Khoa phụ trách.</summary>
    public int? KhoaPhuTrachId { get; init; }

    /// <summary>Mã cán bộ liên kết.</summary>
    public string? MaNhanSuLienKet { get; init; }

    /// <summary>Tài khoản đang mở.</summary>
    public bool CoHoatDong { get; init; }

    /// <summary>Đang bị khóa tạm do đăng nhập sai.</summary>
    public bool DangKhoaTam { get; init; }

    /// <summary>Số lần đăng nhập sai liên tiếp.</summary>
    public int SoLanSai { get; init; }

    /// <summary>Lần đăng nhập gần nhất (UTC).</summary>
    public DateTime? LanDangNhapCuoi { get; init; }

    /// <summary>Ngày tạo (UTC).</summary>
    public DateTime NgayTao { get; init; }
}

/// <summary>
/// Yêu cầu tạo / cập nhật tài khoản.
/// </summary>
public sealed class YeuCauTaiKhoan
{
    /// <summary>Id (rỗng khi tạo mới).</summary>
    public string? Id { get; set; }

    /// <summary>Email đăng nhập.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Họ tên.</summary>
    public string HoVaTen { get; set; } = string.Empty;

    /// <summary>Mật khẩu (bắt buộc khi tạo; khi sửa để trống là giữ nguyên).</summary>
    public string? MatKhau { get; set; }

    /// <summary>Vai trò.</summary>
    public string VaiTro { get; set; } = VaiTroMacDinh.NguoiXem;

    /// <summary>Vùng quản lý (với Manager).</summary>
    public string? VungQuanLy { get; set; }

    /// <summary>Khoa phụ trách (với Manager).</summary>
    public int? KhoaPhuTrachId { get; set; }

    /// <summary>Mã cán bộ liên kết.</summary>
    public string? MaNhanSuLienKet { get; set; }
}

/// <summary>
/// Quản trị tài khoản và phân quyền (chỉ Superadmin/Admin).
/// </summary>
public interface ITaiKhoanDichVu
{
    /// <summary>Danh sách tài khoản kèm vai trò, vùng, trạng thái khóa.</summary>
    Task<List<DongTaiKhoan>> LayDanhSachAsync();

    /// <summary>Tạo mới hoặc cập nhật tài khoản; đổi quyền làm phiên cũ hết hiệu lực.</summary>
    Task LuuAsync(YeuCauTaiKhoan yeuCau);

    /// <summary>Khóa hoặc mở tài khoản.</summary>
    Task DoiTrangThaiAsync(string id);

    /// <summary>Mở khóa tạm do đăng nhập sai.</summary>
    Task MoKhoaTamAsync(string id);

    /// <summary>Xóa tài khoản.</summary>
    Task XoaAsync(string id);
}

/// <summary>
/// Cài đặt <see cref="ITaiKhoanDichVu"/> trên ASP.NET Core Identity.
/// </summary>
public class TaiKhoanDichVu : DichVuNghiepVuCoSo, ITaiKhoanDichVu
{
    private const string TenBang = "TaiKhoan";
    private const int DoDaiMatKhauToiThieu = 8;
    private readonly UserManager<TaiKhoan> _quanLyNguoiDung;

    /// <summary>
    /// Khởi tạo dịch vụ tài khoản.
    /// </summary>
    public TaiKhoanDichVu(
        QuanLyDaoTaoDbContext coSoDuLieu,
        UserManager<TaiKhoan> quanLyNguoiDung,
        IPhanQuyenDichVu? phanQuyen = null,
        INhatKyDichVu? nhatKy = null,
        IBusThayDoiDuLieu? bus = null)
        : base(coSoDuLieu, phanQuyen, nhatKy, bus)
    {
        ArgumentNullException.ThrowIfNull(quanLyNguoiDung);
        _quanLyNguoiDung = quanLyNguoiDung;
    }

    /// <inheritdoc />
    public async Task<List<DongTaiKhoan>> LayDanhSachAsync()
    {
        await DamBaoQuanTriAsync();
        var vaiTroTheoNguoi = await (from ur in _coSoDuLieu.UserRoles
                                     join r in _coSoDuLieu.Roles on ur.RoleId equals r.Id
                                     select new { ur.UserId, r.Name }).AsNoTracking().ToListAsync();
        var bangVaiTro = vaiTroTheoNguoi.GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => VaiTroMacDinh.TatCa.FirstOrDefault(v => g.Any(x => x.Name == v)) ?? VaiTroMacDinh.NguoiXem);
        var bayGio = DateTimeOffset.UtcNow;

        var nguoiDung = await _coSoDuLieu.Users.AsNoTracking().OrderBy(u => u.Email).ToListAsync();
        return nguoiDung.Select(u => new DongTaiKhoan
        {
            Id = u.Id,
            Email = u.Email ?? u.UserName ?? string.Empty,
            HoVaTen = u.HoVaTen,
            VaiTro = bangVaiTro.GetValueOrDefault(u.Id, VaiTroMacDinh.NguoiXem),
            VungQuanLy = u.VungQuanLy,
            KhoaPhuTrachId = u.KhoaPhuTrachId,
            MaNhanSuLienKet = u.MaNhanSuLienKet,
            CoHoatDong = u.CoHoatDong,
            DangKhoaTam = u.LockoutEnd.HasValue && u.LockoutEnd > bayGio,
            SoLanSai = u.AccessFailedCount,
            LanDangNhapCuoi = u.LanDangNhapCuoi,
            NgayTao = u.NgayTao
        }).ToList();
    }

    /// <inheritdoc />
    public async Task LuuAsync(YeuCauTaiKhoan yeuCau)
    {
        ArgumentNullException.ThrowIfNull(yeuCau);
        var nguoiDung = await DamBaoQuanTriAsync();
        KiemTraPhanQuyen(nguoiDung, yeuCau);
        var email = (yeuCau.Email ?? string.Empty).Trim();
        if (email.Length == 0 || !email.Contains('@'))
        {
            throw new InvalidOperationException("Email đăng nhập không hợp lệ.");
        }

        if (string.IsNullOrWhiteSpace(yeuCau.HoVaTen))
        {
            throw new InvalidOperationException("Vui lòng nhập họ và tên.");
        }

        var laQuanLy = yeuCau.VaiTro == VaiTroMacDinh.QuanLy;
        if (string.IsNullOrEmpty(yeuCau.Id))
        {
            if (string.IsNullOrWhiteSpace(yeuCau.MatKhau) || yeuCau.MatKhau.Length < DoDaiMatKhauToiThieu)
            {
                throw new InvalidOperationException($"Mật khẩu khởi tạo cần ít nhất {DoDaiMatKhauToiThieu} ký tự, có chữ thường và số.");
            }

            if (await _quanLyNguoiDung.FindByEmailAsync(email) is not null)
            {
                throw new InvalidOperationException("Email này đã được dùng cho tài khoản khác.");
            }

            var moi = new TaiKhoan
            {
                UserName = email, Email = email, EmailConfirmed = true, HoVaTen = yeuCau.HoVaTen.Trim(),
                MaNhanSuLienKet = string.IsNullOrWhiteSpace(yeuCau.MaNhanSuLienKet) ? null : yeuCau.MaNhanSuLienKet.Trim(),
                VungQuanLy = laQuanLy ? yeuCau.VungQuanLy : null, KhoaPhuTrachId = laQuanLy ? yeuCau.KhoaPhuTrachId : null,
                CoHoatDong = true, LockoutEnabled = true, NgayTao = DateTime.UtcNow
            };
            DamBaoThanhCong(await _quanLyNguoiDung.CreateAsync(moi, yeuCau.MatKhau));
            DamBaoThanhCong(await _quanLyNguoiDung.AddToRoleAsync(moi, yeuCau.VaiTro));
            await HoanTatThaoTacAsync(nguoiDung, "Tạo tài khoản", TenBang, email, null, MoTa(yeuCau), CancellationToken.None);
            return;
        }

        var taiKhoan = await _quanLyNguoiDung.FindByIdAsync(yeuCau.Id)
            ?? throw new KeyNotFoundException("Tài khoản không còn tồn tại.");
        var vaiTroCu = await _quanLyNguoiDung.GetRolesAsync(taiKhoan);
        if (vaiTroCu.Contains(VaiTroMacDinh.SuperAdmin) && !nguoiDung.LaSuperAdmin)
        {
            throw new UnauthorizedAccessException("Chỉ Superadmin được sửa tài khoản Superadmin.");
        }

        if (string.Equals(taiKhoan.Email, nguoiDung.TenDangNhap, StringComparison.OrdinalIgnoreCase) &&
            !vaiTroCu.Contains(yeuCau.VaiTro))
        {
            throw new InvalidOperationException("Không thể tự đổi vai trò của tài khoản đang đăng nhập.");
        }

        var giaTriCu = $"{string.Join(",", vaiTroCu)} | vùng {taiKhoan.VungQuanLy ?? "-"} | khoa {taiKhoan.KhoaPhuTrachId?.ToString() ?? "-"}";
        var doiQuyen = !vaiTroCu.Contains(yeuCau.VaiTro) || taiKhoan.VungQuanLy != (laQuanLy ? yeuCau.VungQuanLy : null) ||
                       taiKhoan.KhoaPhuTrachId != (laQuanLy ? yeuCau.KhoaPhuTrachId : null);

        taiKhoan.HoVaTen = yeuCau.HoVaTen.Trim();
        taiKhoan.MaNhanSuLienKet = string.IsNullOrWhiteSpace(yeuCau.MaNhanSuLienKet) ? null : yeuCau.MaNhanSuLienKet.Trim();
        taiKhoan.VungQuanLy = laQuanLy ? yeuCau.VungQuanLy : null;
        taiKhoan.KhoaPhuTrachId = laQuanLy ? yeuCau.KhoaPhuTrachId : null;
        DamBaoThanhCong(await _quanLyNguoiDung.UpdateAsync(taiKhoan));

        if (!vaiTroCu.Contains(yeuCau.VaiTro) || vaiTroCu.Count != 1)
        {
            DamBaoThanhCong(await _quanLyNguoiDung.RemoveFromRolesAsync(taiKhoan, vaiTroCu));
            DamBaoThanhCong(await _quanLyNguoiDung.AddToRoleAsync(taiKhoan, yeuCau.VaiTro));
        }

        if (!string.IsNullOrWhiteSpace(yeuCau.MatKhau))
        {
            if (yeuCau.MatKhau.Length < DoDaiMatKhauToiThieu)
            {
                throw new InvalidOperationException($"Mật khẩu mới cần ít nhất {DoDaiMatKhauToiThieu} ký tự.");
            }

            var maDatLai = await _quanLyNguoiDung.GeneratePasswordResetTokenAsync(taiKhoan);
            DamBaoThanhCong(await _quanLyNguoiDung.ResetPasswordAsync(taiKhoan, maDatLai, yeuCau.MatKhau));
            doiQuyen = true;
        }

        if (doiQuyen)
        {
            // Phiên đang mở của tài khoản sẽ bị từ chối ở lần kiểm tra SecurityStamp kế tiếp (≤ 1 phút).
            await _quanLyNguoiDung.UpdateSecurityStampAsync(taiKhoan);
        }

        await HoanTatThaoTacAsync(nguoiDung, doiQuyen ? "Đổi phân quyền" : "Cập nhật tài khoản", TenBang, taiKhoan.Email,
            giaTriCu, MoTa(yeuCau) + (string.IsNullOrWhiteSpace(yeuCau.MatKhau) ? string.Empty : " | đặt lại mật khẩu"), CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task DoiTrangThaiAsync(string id)
    {
        var nguoiDung = await DamBaoQuanTriAsync();
        var taiKhoan = await LayVaKiemTraAsync(id, nguoiDung);
        taiKhoan.CoHoatDong = !taiKhoan.CoHoatDong;
        DamBaoThanhCong(await _quanLyNguoiDung.UpdateAsync(taiKhoan));
        await _quanLyNguoiDung.UpdateSecurityStampAsync(taiKhoan);
        await HoanTatThaoTacAsync(nguoiDung, taiKhoan.CoHoatDong ? "Mở khóa tài khoản" : "Khóa tài khoản", TenBang, taiKhoan.Email,
            null, null, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task MoKhoaTamAsync(string id)
    {
        var nguoiDung = await DamBaoQuanTriAsync();
        var taiKhoan = await _quanLyNguoiDung.FindByIdAsync(id) ?? throw new KeyNotFoundException("Tài khoản không còn tồn tại.");
        await _quanLyNguoiDung.SetLockoutEndDateAsync(taiKhoan, null);
        await _quanLyNguoiDung.ResetAccessFailedCountAsync(taiKhoan);
        await HoanTatThaoTacAsync(nguoiDung, "Mở khóa tạm", TenBang, taiKhoan.Email, null, null, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task XoaAsync(string id)
    {
        var nguoiDung = await DamBaoQuanTriAsync();
        var taiKhoan = await LayVaKiemTraAsync(id, nguoiDung);
        DamBaoThanhCong(await _quanLyNguoiDung.DeleteAsync(taiKhoan));
        await HoanTatThaoTacAsync(nguoiDung, "Xóa tài khoản", TenBang, taiKhoan.Email, taiKhoan.HoVaTen, null, CancellationToken.None);
    }

    private async Task<TaiKhoan> LayVaKiemTraAsync(string id, NguoiDungHienTai nguoiDung)
    {
        var taiKhoan = await _quanLyNguoiDung.FindByIdAsync(id) ?? throw new KeyNotFoundException("Tài khoản không còn tồn tại.");
        if (string.Equals(taiKhoan.Email, nguoiDung.TenDangNhap, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Không thể khóa hoặc xóa tài khoản đang đăng nhập.");
        }

        if (await _quanLyNguoiDung.IsInRoleAsync(taiKhoan, VaiTroMacDinh.SuperAdmin) && !nguoiDung.LaSuperAdmin)
        {
            throw new UnauthorizedAccessException("Chỉ Superadmin được thao tác trên tài khoản Superadmin.");
        }

        return taiKhoan;
    }

    private static void KiemTraPhanQuyen(NguoiDungHienTai nguoiDung, YeuCauTaiKhoan yeuCau)
    {
        if (!VaiTroMacDinh.TatCa.Contains(yeuCau.VaiTro))
        {
            throw new InvalidOperationException("Vai trò không hợp lệ.");
        }

        if (yeuCau.VaiTro == VaiTroMacDinh.SuperAdmin && !nguoiDung.LaSuperAdmin)
        {
            throw new UnauthorizedAccessException("Chỉ Superadmin được cấp vai trò Superadmin.");
        }

        if (yeuCau.VaiTro == VaiTroMacDinh.QuanLy && !VungQuanLy.LaHopLe(yeuCau.VungQuanLy))
        {
            throw new InvalidOperationException("Tài khoản Quản lý phải chọn vùng quản lý.");
        }
    }

    private static void DamBaoThanhCong(IdentityResult ketQua)
    {
        if (!ketQua.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", ketQua.Errors.Select(e => e.Description)));
        }
    }

    private static string MoTa(YeuCauTaiKhoan yeuCau) =>
        $"{yeuCau.VaiTro} | vùng {yeuCau.VungQuanLy ?? "-"} | khoa {yeuCau.KhoaPhuTrachId?.ToString() ?? "-"}";
}
