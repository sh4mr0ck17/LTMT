using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển quản lý danh mục Chức vụ công tác.
/// </summary>
public class ChucVuController : Controller
{
    private readonly IChucVuDichVu _chucVuDichVu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển chức vụ với dịch vụ chức vụ và nhật ký.
    /// </summary>
    public ChucVuController(IChucVuDichVu chucVuDichVu, INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(chucVuDichVu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _chucVuDichVu = chucVuDichVu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Hiển thị danh sách chức vụ công tác.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? tuKhoa = null, bool? coHoatDong = null)
    {
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.CoHoatDong = coHoatDong;

        var danhSach = await _chucVuDichVu.LayDanhSachAsync(tuKhoa, coHoatDong, HttpContext.RequestAborted);
        return View(danhSach);
    }

    /// <summary>
    /// Hiển thị biểu mẫu thêm mới chức vụ.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public IActionResult ThemMoi()
    {
        return View(new ChucVu { CoHoatDong = true });
    }

    /// <summary>
    /// Xử lý lưu chức vụ mới.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemMoi(ChucVu chucVu)
    {
        ArgumentNullException.ThrowIfNull(chucVu);

        if (!ModelState.IsValid)
        {
            return View(chucVu);
        }

        try
        {
            await _chucVuDichVu.ThemChucVuAsync(chucVu, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Thêm mới",
                "ChucVu",
                chucVu.MaChucVu,
                null,
                $"Tên chức vụ: {chucVu.TenChucVu}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Thêm mới chức vụ '{chucVu.TenChucVu}' thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(chucVu);
        }
    }

    /// <summary>
    /// Hiển thị biểu mẫu chỉnh sửa chức vụ.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ChinhSua(int id)
    {
        var cv = await _chucVuDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (cv == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy chức vụ có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        return View(cv);
    }

    /// <summary>
    /// Xử lý cập nhật thông tin chức vụ.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChinhSua(int id, ChucVu chucVu)
    {
        ArgumentNullException.ThrowIfNull(chucVu);

        if (id != chucVu.ChucVuId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(chucVu);
        }

        try
        {
            await _chucVuDichVu.CapNhatChucVuAsync(chucVu, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Cập nhật",
                "ChucVu",
                chucVu.MaChucVu,
                null,
                $"Cập nhật chức vụ: {chucVu.TenChucVu}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Cập nhật chức vụ '{chucVu.TenChucVu}' thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(chucVu);
        }
    }

    /// <summary>
    /// Xóa chức vụ nếu không còn nhân sự giữ chức vụ này.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            var cv = await _chucVuDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
            var maChucVu = cv?.MaChucVu ?? id.ToString();
            var tenChucVu = cv?.TenChucVu ?? "";

            await _chucVuDichVu.XoaChucVuAsync(id, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Xóa",
                "ChucVu",
                maChucVu,
                $"Chức vụ: {tenChucVu}",
                null,
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Xóa chức vụ '{tenChucVu}' thành công!";
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
