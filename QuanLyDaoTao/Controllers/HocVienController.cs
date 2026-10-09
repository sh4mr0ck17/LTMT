using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các thao tác CRUD và quản trị Học viên đào tạo.
/// </summary>
public class HocVienController : Controller
{
    private readonly IHocVienDichVu _hocVienDichVu;
    private readonly ILopHocDichVu _lopHocDichVu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển với các dịch vụ học viên, lớp học và nhật ký.
    /// </summary>
    public HocVienController(IHocVienDichVu hocVienDichVu, ILopHocDichVu lopHocDichVu, INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(hocVienDichVu);
        ArgumentNullException.ThrowIfNull(lopHocDichVu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _hocVienDichVu = hocVienDichVu;
        _lopHocDichVu = lopHocDichVu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Hiển thị danh sách học viên có hỗ trợ tìm kiếm và lọc theo lớp học.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int? lopHocId = null, string? tuKhoa = null, bool? coHoatDong = null)
    {
        ViewBag.LopHocId = lopHocId;
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.CoHoatDong = coHoatDong;

        ViewBag.DanhSachLop = await _lopHocDichVu.LayDanhSachAsync(null, null, null, true, HttpContext.RequestAborted);
        var danhSach = await _hocVienDichVu.LayDanhSachAsync(lopHocId, tuKhoa, coHoatDong, HttpContext.RequestAborted);
        return View(danhSach);
    }

    /// <summary>
    /// Hiển thị thông tin chi tiết một học viên và lớp đào tạo tương ứng.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ChiTiet(int id)
    {
        var hocVien = await _hocVienDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (hocVien == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy học viên có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        return View(hocVien);
    }

    /// <summary>
    /// Hiển thị biểu mẫu thêm mới một học viên.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ThemMoi()
    {
        ViewBag.DanhSachLop = await _lopHocDichVu.LayDanhSachAsync(null, null, null, true, HttpContext.RequestAborted);
        return View(new HocVien { CoHoatDong = true, NgaySinh = DateTime.Today.AddYears(-20), GioiTinh = "Nam" });
    }

    /// <summary>
    /// Xử lý lưu thông tin học viên mới.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemMoi(HocVien hocVien)
    {
        ArgumentNullException.ThrowIfNull(hocVien);

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachLop = await _lopHocDichVu.LayDanhSachAsync(null, null, null, true, HttpContext.RequestAborted);
            return View(hocVien);
        }

        try
        {
            await _hocVienDichVu.ThemHocVienAsync(hocVien, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Thêm mới",
                "HocVien",
                hocVien.MaHocVien,
                null,
                $"Họ tên: {hocVien.HoVaTen}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Thêm mới học viên '{hocVien.HoVaTen}' ({hocVien.MaHocVien}) thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.DanhSachLop = await _lopHocDichVu.LayDanhSachAsync(null, null, null, true, HttpContext.RequestAborted);
            return View(hocVien);
        }
    }

    /// <summary>
    /// Hiển thị biểu mẫu chỉnh sửa thông tin học viên.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ChinhSua(int id)
    {
        var hocVien = await _hocVienDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (hocVien == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy học viên có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DanhSachLop = await _lopHocDichVu.LayDanhSachAsync(null, null, null, true, HttpContext.RequestAborted);
        return View(hocVien);
    }

    /// <summary>
    /// Xử lý cập nhật thông tin học viên.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChinhSua(int id, HocVien hocVien)
    {
        ArgumentNullException.ThrowIfNull(hocVien);

        if (id != hocVien.HocVienId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachLop = await _lopHocDichVu.LayDanhSachAsync(null, null, null, true, HttpContext.RequestAborted);
            return View(hocVien);
        }

        try
        {
            await _hocVienDichVu.CapNhatHocVienAsync(hocVien, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Cập nhật",
                "HocVien",
                hocVien.MaHocVien,
                null,
                $"Cập nhật học viên: {hocVien.HoVaTen}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Cập nhật học viên '{hocVien.HoVaTen}' thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.DanhSachLop = await _lopHocDichVu.LayDanhSachAsync(null, null, null, true, HttpContext.RequestAborted);
            return View(hocVien);
        }
    }

    /// <summary>
    /// Xóa một học viên khỏi hệ thống.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            var hocVien = await _hocVienDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
            var maHocVien = hocVien?.MaHocVien ?? id.ToString();
            var hoTen = hocVien?.HoVaTen ?? "";

            await _hocVienDichVu.XoaHocVienAsync(id, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Xóa",
                "HocVien",
                maHocVien,
                $"Học viên: {hoTen}",
                null,
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Xóa học viên '{hoTen}' thành công!";
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ThongBaoLoi"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
