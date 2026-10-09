using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các thao tác CRUD và quản trị Bộ môn trực thuộc Khoa.
/// </summary>
public class BoMonController : Controller
{
    private readonly IBoMonDichVu _boMonDichVu;
    private readonly IKhoaDichVu _khoaDichVu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển với dịch vụ bộ môn, khoa và nhật ký.
    /// </summary>
    public BoMonController(IBoMonDichVu boMonDichVu, IKhoaDichVu khoaDichVu, INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(boMonDichVu);
        ArgumentNullException.ThrowIfNull(khoaDichVu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _boMonDichVu = boMonDichVu;
        _khoaDichVu = khoaDichVu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Hiển thị danh sách bộ môn có lọc theo khoa và tìm kiếm.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int? khoaId = null, string? tuKhoa = null, bool? coHoatDong = null)
    {
        ViewBag.KhoaId = khoaId;
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.CoHoatDong = coHoatDong;
        ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);

        var danhSach = await _boMonDichVu.LayDanhSachAsync(khoaId, tuKhoa, coHoatDong, HttpContext.RequestAborted);
        return View(danhSach);
    }

    /// <summary>
    /// Hiển thị chi tiết một bộ môn cùng các nhân sự và môn học phụ trách.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ChiTiet(int id)
    {
        var boMon = await _boMonDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (boMon == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy bộ môn có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.CoTheXoa = await _boMonDichVu.CoTheXoaAsync(id, HttpContext.RequestAborted);
        return View(boMon);
    }

    /// <summary>
    /// Hiển thị biểu mẫu thêm mới một bộ môn.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ThemMoi()
    {
        ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
        return View(new BoMon { CoHoatDong = true });
    }

    /// <summary>
    /// Xử lý lưu thông tin bộ môn mới.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemMoi(BoMon boMon)
    {
        ArgumentNullException.ThrowIfNull(boMon);

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
            return View(boMon);
        }

        try
        {
            await _boMonDichVu.ThemBoMonAsync(boMon, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Thêm mới",
                "BoMon",
                boMon.MaBoMon,
                null,
                $"Tên bộ môn: {boMon.TenBoMon}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Thêm mới bộ môn '{boMon.TenBoMon}' ({boMon.MaBoMon}) thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
            return View(boMon);
        }
    }

    /// <summary>
    /// Hiển thị biểu mẫu chỉnh sửa thông tin bộ môn.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ChinhSua(int id)
    {
        var boMon = await _boMonDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (boMon == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy bộ môn có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
        return View(boMon);
    }

    /// <summary>
    /// Xử lý cập nhật thông tin bộ môn.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChinhSua(int id, BoMon boMon)
    {
        ArgumentNullException.ThrowIfNull(boMon);

        if (id != boMon.BoMonId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
            return View(boMon);
        }

        try
        {
            await _boMonDichVu.CapNhatBoMonAsync(boMon, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Cập nhật",
                "BoMon",
                boMon.MaBoMon,
                null,
                $"Cập nhật bộ môn: {boMon.TenBoMon}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Cập nhật bộ môn '{boMon.TenBoMon}' thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
            return View(boMon);
        }
    }

    /// <summary>
    /// Xóa một bộ môn khỏi hệ thống nếu không còn ràng buộc.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            var boMon = await _boMonDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
            var maBoMon = boMon?.MaBoMon ?? id.ToString();
            var tenBoMon = boMon?.TenBoMon ?? "";

            await _boMonDichVu.XoaBoMonAsync(id, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Xóa",
                "BoMon",
                maBoMon,
                $"Bộ môn: {tenBoMon}",
                null,
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Xóa bộ môn '{tenBoMon}' thành công!";
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
