using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các thao tác CRUD và quản trị Lớp học đào tạo.
/// </summary>
public class LopHocController : Controller
{
    private readonly ILopHocDichVu _lopHocDichVu;
    private readonly IKhoaDichVu _khoaDichVu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển với dịch vụ lớp học, khoa và nhật ký.
    /// </summary>
    public LopHocController(ILopHocDichVu lopHocDichVu, IKhoaDichVu khoaDichVu, INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(lopHocDichVu);
        ArgumentNullException.ThrowIfNull(khoaDichVu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _lopHocDichVu = lopHocDichVu;
        _khoaDichVu = khoaDichVu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Hiển thị danh sách toàn bộ lớp học có hỗ trợ lọc theo khoa, niên khóa và tìm kiếm.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int? khoaId = null, string? nienKhoa = null, string? tuKhoa = null, bool? coHoatDong = null)
    {
        ViewBag.KhoaId = khoaId;
        ViewBag.NienKhoa = nienKhoa;
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.CoHoatDong = coHoatDong;

        ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
        ViewBag.DanhSachNienKhoa = await _lopHocDichVu.LayDanhSachNienKhoaAsync(HttpContext.RequestAborted);

        var danhSach = await _lopHocDichVu.LayDanhSachAsync(khoaId, nienKhoa, tuKhoa, coHoatDong, HttpContext.RequestAborted);
        return View(danhSach);
    }

    /// <summary>
    /// Hiển thị chi tiết một lớp học cùng danh sách học viên và phân công giảng dạy.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ChiTiet(int id)
    {
        var lopHoc = await _lopHocDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (lopHoc == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy lớp học có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.CoTheXoa = await _lopHocDichVu.CoTheXoaAsync(id, HttpContext.RequestAborted);
        return View(lopHoc);
    }

    /// <summary>
    /// Hiển thị biểu mẫu thêm mới một lớp học.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ThemMoi()
    {
        ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
        return View(new LopHoc { CoHoatDong = true, SoLuongToiDa = 50, NienKhoa = "2024-2028" });
    }

    /// <summary>
    /// Xử lý lưu thông tin lớp học mới.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemMoi(LopHoc lopHoc)
    {
        ArgumentNullException.ThrowIfNull(lopHoc);

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
            return View(lopHoc);
        }

        try
        {
            await _lopHocDichVu.ThemLopHocAsync(lopHoc, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Thêm mới",
                "LopHoc",
                lopHoc.MaLop,
                null,
                $"Tên lớp: {lopHoc.TenLop}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Thêm mới lớp '{lopHoc.TenLop}' ({lopHoc.MaLop}) thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
            return View(lopHoc);
        }
    }

    /// <summary>
    /// Hiển thị biểu mẫu chỉnh sửa thông tin lớp học.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ChinhSua(int id)
    {
        var lopHoc = await _lopHocDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (lopHoc == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy lớp học có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
        return View(lopHoc);
    }

    /// <summary>
    /// Xử lý cập nhật thông tin lớp học.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChinhSua(int id, LopHoc lopHoc)
    {
        ArgumentNullException.ThrowIfNull(lopHoc);

        if (id != lopHoc.LopHocId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
            return View(lopHoc);
        }

        try
        {
            await _lopHocDichVu.CapNhatLopHocAsync(lopHoc, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Cập nhật",
                "LopHoc",
                lopHoc.MaLop,
                null,
                $"Cập nhật lớp: {lopHoc.TenLop}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Cập nhật lớp '{lopHoc.TenLop}' thành công!";
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
            return View(lopHoc);
        }
    }

    /// <summary>
    /// Xóa một lớp học khỏi hệ thống nếu không còn ràng buộc.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            var lopHoc = await _lopHocDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
            var maLop = lopHoc?.MaLop ?? id.ToString();
            var tenLop = lopHoc?.TenLop ?? "";

            await _lopHocDichVu.XoaLopHocAsync(id, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Xóa",
                "LopHoc",
                maLop,
                $"Lớp: {tenLop}",
                null,
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Xóa lớp học '{tenLop}' thành công!";
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
