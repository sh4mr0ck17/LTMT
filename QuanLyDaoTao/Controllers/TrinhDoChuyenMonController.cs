using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển quản lý danh mục Trình độ chuyên môn.
/// </summary>
public class TrinhDoChuyenMonController : Controller
{
    private readonly ITrinhDoChuyenMonDichVu _trinhDoChuyenMonDichVu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển trình độ chuyên môn với các dịch vụ nghiệp vụ.
    /// </summary>
    public TrinhDoChuyenMonController(ITrinhDoChuyenMonDichVu trinhDoChuyenMonDichVu, INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(trinhDoChuyenMonDichVu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _trinhDoChuyenMonDichVu = trinhDoChuyenMonDichVu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Hiển thị danh sách toàn bộ trình độ chuyên môn.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? tuKhoa = null, bool? coHoatDong = null)
    {
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.CoHoatDong = coHoatDong;

        var danhSach = await _trinhDoChuyenMonDichVu.LayDanhSachAsync(tuKhoa, coHoatDong, HttpContext.RequestAborted);
        return View(danhSach);
    }

    /// <summary>
    /// Hiển thị biểu mẫu thêm mới trình độ chuyên môn.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public IActionResult ThemMoi()
    {
        return View(new TrinhDoChuyenMon { CoHoatDong = true });
    }

    /// <summary>
    /// Xử lý lưu trình độ chuyên môn mới.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemMoi(TrinhDoChuyenMon trinhDo)
    {
        ArgumentNullException.ThrowIfNull(trinhDo);

        if (!ModelState.IsValid)
        {
            return View(trinhDo);
        }

        try
        {
            await _trinhDoChuyenMonDichVu.ThemTrinhDoAsync(trinhDo, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Thêm mới",
                "TrinhDoChuyenMon",
                trinhDo.MaTrinhDo,
                null,
                $"Tên trình độ: {trinhDo.TenTrinhDo}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Thêm mới trình độ '{trinhDo.TenTrinhDo}' thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(trinhDo);
        }
    }

    /// <summary>
    /// Hiển thị biểu mẫu chỉnh sửa trình độ chuyên môn.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ChinhSua(int id)
    {
        var td = await _trinhDoChuyenMonDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (td == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy trình độ có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        return View(td);
    }

    /// <summary>
    /// Xử lý cập nhật thông tin trình độ chuyên môn.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChinhSua(int id, TrinhDoChuyenMon trinhDo)
    {
        ArgumentNullException.ThrowIfNull(trinhDo);

        if (id != trinhDo.TrinhDoChuyenMonId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(trinhDo);
        }

        try
        {
            await _trinhDoChuyenMonDichVu.CapNhatTrinhDoAsync(trinhDo, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Cập nhật",
                "TrinhDoChuyenMon",
                trinhDo.MaTrinhDo,
                null,
                $"Cập nhật trình độ: {trinhDo.TenTrinhDo}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Cập nhật trình độ '{trinhDo.TenTrinhDo}' thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(trinhDo);
        }
    }

    /// <summary>
    /// Xóa trình độ chuyên môn nếu không còn nhân sự liên kết.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            var td = await _trinhDoChuyenMonDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
            var maTrinhDo = td?.MaTrinhDo ?? id.ToString();
            var tenTrinhDo = td?.TenTrinhDo ?? "";

            await _trinhDoChuyenMonDichVu.XoaTrinhDoAsync(id, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Xóa",
                "TrinhDoChuyenMon",
                maTrinhDo,
                $"Trình độ: {tenTrinhDo}",
                null,
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Xóa trình độ '{tenTrinhDo}' thành công!";
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
