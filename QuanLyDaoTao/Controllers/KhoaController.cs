using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các thao tác CRUD và quản trị Khoa đào tạo.
/// </summary>
public class KhoaController : Controller
{
    private readonly IKhoaDichVu _khoaDichVu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển Khoa với các dịch vụ nghiệp vụ.
    /// </summary>
    /// <param name="khoaDichVu">Dịch vụ quản lý Khoa.</param>
    /// <param name="nhatKyDichVu">Dịch vụ nhật ký kiểm toán.</param>
    public KhoaController(IKhoaDichVu khoaDichVu, INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(khoaDichVu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _khoaDichVu = khoaDichVu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Hiển thị danh sách toàn bộ các khoa có tìm kiếm và bộ lọc.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? tuKhoa = null, bool? coHoatDong = null)
    {
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.CoHoatDong = coHoatDong;

        var danhSach = await _khoaDichVu.LayDanhSachAsync(tuKhoa, coHoatDong, HttpContext.RequestAborted);
        return View(danhSach);
    }

    /// <summary>
    /// Hiển thị thông tin chi tiết một khoa và các bộ môn, lớp học trực thuộc.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ChiTiet(int id)
    {
        var khoa = await _khoaDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (khoa == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy khoa có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.CoTheXoa = await _khoaDichVu.CoTheXoaAsync(id, HttpContext.RequestAborted);
        return View(khoa);
    }

    /// <summary>
    /// Hiển thị biểu mẫu thêm mới một khoa.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public IActionResult ThemMoi()
    {
        return View(new Khoa { CoHoatDong = true });
    }

    /// <summary>
    /// Xử lý lưu thông tin khoa mới vào hệ thống.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemMoi(Khoa khoa)
    {
        ArgumentNullException.ThrowIfNull(khoa);

        if (!ModelState.IsValid)
        {
            return View(khoa);
        }

        try
        {
            await _khoaDichVu.ThemKhoaAsync(khoa, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Thêm mới",
                "Khoa",
                khoa.MaKhoa,
                null,
                $"Tên khoa: {khoa.TenKhoa}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Thêm mới khoa '{khoa.TenKhoa}' ({khoa.MaKhoa}) thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("MaKhoa", ex.Message);
            return View(khoa);
        }
    }

    /// <summary>
    /// Hiển thị biểu mẫu chỉnh sửa thông tin khoa.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ChinhSua(int id)
    {
        var khoa = await _khoaDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (khoa == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy khoa có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        return View(khoa);
    }

    /// <summary>
    /// Xử lý cập nhật thông tin khoa.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChinhSua(int id, Khoa khoa)
    {
        ArgumentNullException.ThrowIfNull(khoa);

        if (id != khoa.KhoaId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(khoa);
        }

        try
        {
            await _khoaDichVu.CapNhatKhoaAsync(khoa, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Cập nhật",
                "Khoa",
                khoa.MaKhoa,
                null,
                $"Cập nhật khoa: {khoa.TenKhoa}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Cập nhật khoa '{khoa.TenKhoa}' thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("MaKhoa", ex.Message);
            return View(khoa);
        }
    }

    /// <summary>
    /// Xử lý xóa một khoa khỏi hệ thống có kiểm tra dữ liệu phụ thuộc.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            var khoa = await _khoaDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
            var maKhoa = khoa?.MaKhoa ?? id.ToString();
            var tenKhoa = khoa?.TenKhoa ?? "";

            await _khoaDichVu.XoaKhoaAsync(id, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Xóa",
                "Khoa",
                maKhoa,
                $"Khoa: {tenKhoa}",
                null,
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Xóa khoa '{tenKhoa}' thành công!";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ThongBaoLoi"] = ex.Message;
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ThongBaoLoi"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
