using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.ViewModels.TaiKhoan;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển quản lý xác thực, phân quyền và tài khoản người dùng trong hệ thống.
/// </summary>
public class TaiKhoanController : Controller
{
    private readonly SignInManager<TaiKhoan> _signInManager;
    private readonly UserManager<TaiKhoan> _userManager;
    private readonly RoleManager<VaiTro> _roleManager;
    private readonly INhatKyDichVu _nhatKyDichVu;
    private readonly QuanLyDaoTaoDbContext? _coSoDuLieu;

    /// <summary>
    /// Khởi tạo bộ điều khiển với các dịch vụ Identity, Nhật ký và Cơ sở dữ liệu.
    /// </summary>
    public TaiKhoanController(
        SignInManager<TaiKhoan> signInManager,
        UserManager<TaiKhoan> userManager,
        RoleManager<VaiTro> roleManager,
        INhatKyDichVu nhatKyDichVu,
        QuanLyDaoTaoDbContext? coSoDuLieu = null)
    {
        ArgumentNullException.ThrowIfNull(signInManager);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(roleManager);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _nhatKyDichVu = nhatKyDichVu;
        _coSoDuLieu = coSoDuLieu;
    }

    /// <summary>
    /// Hiển thị trang đăng nhập vào hệ thống.
    /// </summary>
    /// <param name="duongDanQuayLai">Đường dẫn trang cần quay lại sau khi đăng nhập.</param>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult DangNhap(string? duongDanQuayLai = null)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["DuongDanQuayLai"] = duongDanQuayLai;
        return View(new DangNhapViewModel { DuongDanQuayLai = duongDanQuayLai });
    }

    /// <summary>
    /// Xử lý gửi biểu mẫu đăng nhập vào hệ thống.
    /// </summary>
    /// <param name="moHinh">Dữ liệu thông tin đăng nhập.</param>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapViewModel moHinh)
    {
        ArgumentNullException.ThrowIfNull(moHinh);

        if (!ModelState.IsValid)
        {
            return View(moHinh);
        }

        var taiKhoan = await _userManager.FindByEmailAsync(moHinh.Email);
        if (taiKhoan == null || !taiKhoan.CoHoatDong)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác, hoặc tài khoản đã bị khóa.");
            await _nhatKyDichVu.GhiNhatKyAsync(
                moHinh.Email,
                "Đăng nhập thất bại",
                "TaiKhoan",
                moHinh.Email,
                null,
                null,
                "Thất bại",
                HttpContext.Connection.RemoteIpAddress?.ToString());
            return View(moHinh);
        }

        var ketQua = await _signInManager.PasswordSignInAsync(
            taiKhoan.UserName ?? moHinh.Email,
            moHinh.MatKhau,
            moHinh.GhiNhoDangNhap,
            lockoutOnFailure: false);

        if (ketQua.Succeeded)
        {
            await _nhatKyDichVu.GhiNhatKyAsync(
                taiKhoan.Email ?? taiKhoan.UserName ?? "Ẩn danh",
                "Đăng nhập thành công",
                "TaiKhoan",
                taiKhoan.Id,
                null,
                null,
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            if (!string.IsNullOrEmpty(moHinh.DuongDanQuayLai) && Url.IsLocalUrl(moHinh.DuongDanQuayLai))
            {
                return Redirect(moHinh.DuongDanQuayLai);
            }
            return Redirect("/");
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
        await _nhatKyDichVu.GhiNhatKyAsync(
            moHinh.Email,
            "Đăng nhập sai mật khẩu",
            "TaiKhoan",
            moHinh.Email,
            null,
            null,
            "Thất bại",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        return View(moHinh);
    }

    /// <summary>
    /// Chức năng đăng ký công khai đã bị khóa theo chính sách bảo mật nội bộ C500.
    /// Tài khoản chỉ do Quản trị viên cấp phát trực tiếp trong Admin Workspace (/TaiKhoan/TaoMoi).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult DangKy(string? duongDanQuayLai = null)
    {
        TempData["ThongBaoLoi"] = "Chức năng đăng ký tự do đã bị khóa theo quy định C500. Tài khoản chỉ do Quản trị viên cấp phát trong Admin Workspace.";
        return RedirectToAction("DangNhap", "TaiKhoan", new { duongDanQuayLai });
    }

    /// <summary>
    /// Chặn việc gửi yêu cầu đăng ký tự do từ bên ngoài.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public IActionResult DangKy(DangKyViewModel moHinh)
    {
        TempData["ThongBaoLoi"] = "Chức năng đăng ký tự do đã bị khóa theo quy định C500. Tài khoản chỉ do Quản trị viên cấp phát trong Admin Workspace.";
        return RedirectToAction("DangNhap", "TaiKhoan");
    }

    /// <summary>
    /// Xử lý đăng xuất tài khoản khỏi hệ thống.
    /// </summary>
    [HttpGet]
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> DangXuat()
    {
        var tenNguoiDung = User.Identity?.Name ?? "Ẩn danh";
        await _signInManager.SignOutAsync();

        await _nhatKyDichVu.GhiNhatKyAsync(
            tenNguoiDung,
            "Đăng xuất",
            "TaiKhoan",
            tenNguoiDung,
            null,
            null,
            "Thành công",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        return Redirect("/");
    }

    /// <summary>
    /// Hiển thị trang thông báo từ chối quyền truy cập.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult TuChoiTruyCap()
    {
        return View();
    }

    /// <summary>
    /// Hiển thị biểu mẫu đổi mật khẩu tài khoản.
    /// </summary>
    [HttpGet]
    [Authorize]
    public IActionResult DoiMatKhau()
    {
        return View(new DoiMatKhauViewModel());
    }

    /// <summary>
    /// Xử lý đổi mật khẩu tài khoản.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel moHinh)
    {
        ArgumentNullException.ThrowIfNull(moHinh);

        if (!ModelState.IsValid)
        {
            return View(moHinh);
        }

        var taiKhoan = await _userManager.GetUserAsync(User);
        if (taiKhoan == null)
        {
            return RedirectToAction("DangNhap", "TaiKhoan");
        }

        var ketQua = await _userManager.ChangePasswordAsync(taiKhoan, moHinh.MatKhauHienTai, moHinh.MatKhauMoi);
        if (ketQua.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(taiKhoan);
            TempData["ThongBaoThanhCong"] = "Đổi mật khẩu thành công!";

            await _nhatKyDichVu.GhiNhatKyAsync(
                taiKhoan.Email ?? taiKhoan.UserName ?? "Ẩn danh",
                "Đổi mật khẩu",
                "TaiKhoan",
                taiKhoan.Id,
                null,
                null,
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            return RedirectToAction("Index", "Home");
        }

        foreach (var loi in ketQua.Errors)
        {
            ModelState.AddModelError(string.Empty, loi.Description);
        }

        return View(moHinh);
    }

    /// <summary>
    /// Quản trị viên xem danh sách tất cả tài khoản trong hệ thống.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.SuperAdmin},{VaiTroMacDinh.QuanTriVien}")]
    public async Task<IActionResult> DanhSach(string? tuKhoa = null, string? vaiTro = null)
    {
        var truyVan = _userManager.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var tuKhoaChuan = tuKhoa.Trim();
            truyVan = truyVan.Where(u =>
                (u.Email != null && u.Email.Contains(tuKhoaChuan)) ||
                u.HoVaTen.Contains(tuKhoaChuan) ||
                (u.MaNhanSuLienKet != null && u.MaNhanSuLienKet.Contains(tuKhoaChuan)));
        }

        var danhSachNguoiDung = await truyVan.OrderByDescending(u => u.NgayTao).ToListAsync();
        var ketQua = new List<ThongTinTaiKhoanItemViewModel>();

        foreach (var user in danhSachNguoiDung)
        {
            var dsVaiTro = await _userManager.GetRolesAsync(user);
            if (!string.IsNullOrWhiteSpace(vaiTro) && !dsVaiTro.Contains(vaiTro))
            {
                continue;
            }

            ketQua.Add(new ThongTinTaiKhoanItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                HoVaTen = user.HoVaTen,
                MaNhanSuLienKet = user.MaNhanSuLienKet,
                DanhSachVaiTro = dsVaiTro,
                CoHoatDong = user.CoHoatDong,
                NgayTao = user.NgayTao
            });
        }

        var moHinh = new QuanLyTaiKhoanViewModel
        {
            DanhSachTaiKhoan = ketQua,
            TuKhoa = tuKhoa,
            LocTheoVaiTro = vaiTro
        };

        return View(moHinh);
    }

    /// <summary>
    /// Hiển thị biểu mẫu tạo tài khoản người dùng mới.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.SuperAdmin},{VaiTroMacDinh.QuanTriVien}")]
    public async Task<IActionResult> TaoMoi()
    {
        ViewBag.DanhSachVaiTro = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        if (_coSoDuLieu != null)
        {
            ViewBag.DanhSachNhanSu = await _coSoDuLieu.NhanSus.AsNoTracking().OrderBy(n => n.MaNhanSu).ToListAsync();
        }
        return View(new TaoTaiKhoanViewModel { VaiTro = VaiTroMacDinh.GiaoVien });
    }

    /// <summary>
    /// Xử lý tạo mới tài khoản Giáo viên / Cán bộ / Người dùng và cá nhân hóa Workspace.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.SuperAdmin},{VaiTroMacDinh.QuanTriVien}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TaoMoi(TaoTaiKhoanViewModel moHinh)
    {
        ArgumentNullException.ThrowIfNull(moHinh);

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachVaiTro = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            if (_coSoDuLieu != null)
            {
                ViewBag.DanhSachNhanSu = await _coSoDuLieu.NhanSus.AsNoTracking().OrderBy(n => n.MaNhanSu).ToListAsync();
            }
            return View(moHinh);
        }

        // Chỉ Superadmin mới có quyền cấp vai trò Superadmin
        if (string.Equals(moHinh.VaiTro, VaiTroMacDinh.SuperAdmin, StringComparison.OrdinalIgnoreCase) &&
            !User.IsInRole(VaiTroMacDinh.SuperAdmin))
        {
            ModelState.AddModelError("VaiTro", "Chỉ Superadmin mới có quyền cấp phát tài khoản Superadmin.");
            ViewBag.DanhSachVaiTro = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(moHinh);
        }

        var taiKhoanTonTai = await _userManager.FindByEmailAsync(moHinh.Email);
        if (taiKhoanTonTai != null)
        {
            ModelState.AddModelError("Email", "Địa chỉ email này đã được sử dụng.");
            ViewBag.DanhSachVaiTro = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(moHinh);
        }

        // Tự động tạo hoặc liên kết hồ sơ Nhân sự nếu là Giáo viên / Cán bộ
        var maNs = moHinh.MaNhanSuLienKet?.Trim();
        if (_coSoDuLieu != null && moHinh.TuDongTaoHoSoNhanSu &&
            (string.Equals(moHinh.VaiTro, VaiTroMacDinh.GiaoVien, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(moHinh.VaiTro, VaiTroMacDinh.CanBo, StringComparison.OrdinalIgnoreCase)))
        {
            if (string.IsNullOrWhiteSpace(maNs))
            {
                maNs = $"CB{DateTime.UtcNow:MMddHHmm}";
            }

            var nsTonTai = await _coSoDuLieu.NhanSus.FirstOrDefaultAsync(n => n.MaNhanSu == maNs);
            if (nsTonTai == null)
            {
                var nsMoi = new NhanSu
                {
                    MaNhanSu = maNs,
                    HoVaTen = moHinh.HoVaTen.Trim(),
                    Email = moHinh.Email.Trim(),
                    SoDienThoai = moHinh.SoDienThoai?.Trim(),
                    ChuyenMonDaoTao = moHinh.ChucDanh?.Trim() ?? (moHinh.VaiTro == VaiTroMacDinh.GiaoVien ? "Giảng viên chuyên ngành" : "Cán bộ quản lý đào tạo"),
                    NgaySinh = new DateTime(1988, 1, 1),
                    GioiTinh = "Nam",
                    TrangThai = "Đang làm việc",
                    NgayVaoLam = DateTime.UtcNow
                };
                _coSoDuLieu.NhanSus.Add(nsMoi);
                await _coSoDuLieu.SaveChangesAsync();
            }
        }

        var taiKhoanMoi = new TaiKhoan
        {
            UserName = moHinh.Email,
            Email = moHinh.Email,
            EmailConfirmed = true,
            HoVaTen = moHinh.HoVaTen,
            PhoneNumber = moHinh.SoDienThoai,
            MaNhanSuLienKet = maNs,
            CoHoatDong = true,
            NgayTao = DateTime.UtcNow
        };

        var ketQua = await _userManager.CreateAsync(taiKhoanMoi, moHinh.MatKhau);
        if (ketQua.Succeeded)
        {
            if (!string.IsNullOrEmpty(moHinh.VaiTro))
            {
                if (!await _roleManager.RoleExistsAsync(moHinh.VaiTro))
                {
                    await _roleManager.CreateAsync(new VaiTro(moHinh.VaiTro) { MoTa = moHinh.VaiTro });
                }
                await _userManager.AddToRoleAsync(taiKhoanMoi, moHinh.VaiTro);
            }

            if (!string.IsNullOrWhiteSpace(moHinh.AnhDaiDien))
            {
                await _userManager.AddClaimAsync(taiKhoanMoi, new Claim("Avatar", moHinh.AnhDaiDien.Trim()));
            }

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Tạo tài khoản Giáo viên / Cán bộ",
                "TaiKhoan",
                taiKhoanMoi.Id,
                null,
                $"Email: {taiKhoanMoi.Email}, Vai trò: {moHinh.VaiTro}, Mã NS: {maNs}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["ThongBaoThanhCong"] = $"Đã cấp tài khoản {moHinh.Email} ({moHinh.VaiTro}) thành công!";
            return RedirectToAction(nameof(DanhSach));
        }

        foreach (var loi in ketQua.Errors)
        {
            ModelState.AddModelError(string.Empty, loi.Description);
        }

        ViewBag.DanhSachVaiTro = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        return View(moHinh);
    }

    /// <summary>
    /// Khóa hoặc mở khóa trạng thái hoạt động của một tài khoản.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.SuperAdmin},{VaiTroMacDinh.QuanTriVien}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KhoaMoKhoa(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var taiKhoan = await _userManager.FindByIdAsync(id);
        if (taiKhoan == null)
        {
            return NotFound();
        }

        // Không cho phép tự khóa tài khoản của chính mình
        var nguoiDungHienTai = await _userManager.GetUserAsync(User);
        if (nguoiDungHienTai != null && nguoiDungHienTai.Id == taiKhoan.Id)
        {
            TempData["ThongBaoLoi"] = "Bạn không thể tự khóa tài khoản của chính mình!";
            return RedirectToAction(nameof(DanhSach));
        }

        var trangThaiCu = taiKhoan.CoHoatDong;
        taiKhoan.CoHoatDong = !taiKhoan.CoHoatDong;
        await _userManager.UpdateAsync(taiKhoan);

        await _nhatKyDichVu.GhiNhatKyAsync(
            User.Identity?.Name ?? "Quản trị viên",
            taiKhoan.CoHoatDong ? "Mở khóa tài khoản" : "Khóa tài khoản",
            "TaiKhoan",
            taiKhoan.Id,
            $"CoHoatDong: {trangThaiCu}",
            $"CoHoatDong: {taiKhoan.CoHoatDong}",
            "Thành công",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["ThongBaoThanhCong"] = $"Cập nhật trạng thái tài khoản {taiKhoan.Email} thành công!";
        return RedirectToAction(nameof(DanhSach));
    }

    /// <summary>
    /// Xóa vĩnh viễn một tài khoản người dùng khỏi hệ thống (Dành cho Superadmin / Admin).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.SuperAdmin},{VaiTroMacDinh.QuanTriVien}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaTaiKhoan(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var taiKhoan = await _userManager.FindByIdAsync(id);
        if (taiKhoan == null)
        {
            return NotFound();
        }

        var nguoiDungHienTai = await _userManager.GetUserAsync(User);
        if (nguoiDungHienTai != null && nguoiDungHienTai.Id == taiKhoan.Id)
        {
            TempData["ThongBaoLoi"] = "Bạn không thể tự xóa tài khoản đang đăng nhập của chính mình!";
            return RedirectToAction(nameof(DanhSach));
        }

        // Không cho phép Admin thường xóa tài khoản Superadmin
        if (await _userManager.IsInRoleAsync(taiKhoan, VaiTroMacDinh.SuperAdmin) && !User.IsInRole(VaiTroMacDinh.SuperAdmin))
        {
            TempData["ThongBaoLoi"] = "Chỉ Superadmin mới có quyền xóa tài khoản cấp Superadmin!";
            return RedirectToAction(nameof(DanhSach));
        }

        var emailXoa = taiKhoan.Email ?? taiKhoan.UserName ?? id;
        var ketQuaXoa = await _userManager.DeleteAsync(taiKhoan);
        if (ketQuaXoa.Succeeded)
        {
            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Superadmin",
                "Xóa tài khoản",
                "TaiKhoan",
                id,
                $"Email: {emailXoa}",
                "Đã xóa vĩnh viễn",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["ThongBaoThanhCong"] = $"Đã xóa vĩnh viễn tài khoản {emailXoa} khỏi hệ thống!";
        }
        else
        {
            TempData["ThongBaoLoi"] = "Không thể xóa tài khoản. Vui lòng thử lại.";
        }

        return RedirectToAction(nameof(DanhSach));
    }
}
