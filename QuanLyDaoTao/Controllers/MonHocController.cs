using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các thao tác CRUD và quản trị Môn học đào tạo.
/// </summary>
public class MonHocController : Controller
{
    private readonly IMonHocDichVu _monHocDichVu;
    private readonly IBoMonDichVu _boMonDichVu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển với dịch vụ môn học, bộ môn và nhật ký.
    /// </summary>
    public MonHocController(IMonHocDichVu monHocDichVu, IBoMonDichVu boMonDichVu, INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(monHocDichVu);
        ArgumentNullException.ThrowIfNull(boMonDichVu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _monHocDichVu = monHocDichVu;
        _boMonDichVu = boMonDichVu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Hiển thị danh sách môn học có lọc theo bộ môn và tìm kiếm.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int? boMonId = null, string? tuKhoa = null, bool? coHoatDong = null)
    {
        ViewBag.BoMonId = boMonId;
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.CoHoatDong = coHoatDong;

        ViewBag.DanhSachBoMon = await _boMonDichVu.LayDanhSachAsync(null, null, true, HttpContext.RequestAborted);
        var danhSach = await _monHocDichVu.LayDanhSachAsync(boMonId, tuKhoa, coHoatDong, HttpContext.RequestAborted);
        return View(danhSach);
    }

    /// <summary>
    /// Hiển thị chi tiết một môn học cùng các phân công giảng dạy tương ứng.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ChiTiet(int id)
    {
        var monHoc = await _monHocDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (monHoc == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy môn học có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.CoTheXoa = await _monHocDichVu.CoTheXoaAsync(id, HttpContext.RequestAborted);
        return View(monHoc);
    }

    /// <summary>
    /// Hiển thị biểu mẫu thêm mới một môn học.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ThemMoi()
    {
        ViewBag.DanhSachBoMon = await _boMonDichVu.LayDanhSachAsync(null, null, true, HttpContext.RequestAborted);
        return View(new MonHoc { CoHoatDong = true, SoTinChi = 3, SoTietLyThuyet = 30, SoTietThucHanh = 15 });
    }

    /// <summary>
    /// Xử lý lưu thông tin môn học mới.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemMoi(MonHoc monHoc)
    {
        ArgumentNullException.ThrowIfNull(monHoc);

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachBoMon = await _boMonDichVu.LayDanhSachAsync(null, null, true, HttpContext.RequestAborted);
            return View(monHoc);
        }

        try
        {
            await _monHocDichVu.ThemMonHocAsync(monHoc, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Thêm mới",
                "MonHoc",
                monHoc.MaMonHoc,
                null,
                $"Tên môn: {monHoc.TenMonHoc}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Thêm mới môn học '{monHoc.TenMonHoc}' ({monHoc.MaMonHoc}) thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.DanhSachBoMon = await _boMonDichVu.LayDanhSachAsync(null, null, true, HttpContext.RequestAborted);
            return View(monHoc);
        }
    }

    /// <summary>
    /// Hiển thị biểu mẫu chỉnh sửa môn học.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ChinhSua(int id)
    {
        var monHoc = await _monHocDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (monHoc == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy môn học có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DanhSachBoMon = await _boMonDichVu.LayDanhSachAsync(null, null, true, HttpContext.RequestAborted);
        return View(monHoc);
    }

    /// <summary>
    /// Xử lý cập nhật thông tin môn học.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChinhSua(int id, MonHoc monHoc)
    {
        ArgumentNullException.ThrowIfNull(monHoc);

        if (id != monHoc.MonHocId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.DanhSachBoMon = await _boMonDichVu.LayDanhSachAsync(null, null, true, HttpContext.RequestAborted);
            return View(monHoc);
        }

        try
        {
            await _monHocDichVu.CapNhatMonHocAsync(monHoc, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Cập nhật",
                "MonHoc",
                monHoc.MaMonHoc,
                null,
                $"Cập nhật môn: {monHoc.TenMonHoc}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Cập nhật môn học '{monHoc.TenMonHoc}' thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.DanhSachBoMon = await _boMonDichVu.LayDanhSachAsync(null, null, true, HttpContext.RequestAborted);
            return View(monHoc);
        }
    }

    /// <summary>
    /// Xóa môn học khỏi hệ thống nếu không còn phân công.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            var mh = await _monHocDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
            var maMh = mh?.MaMonHoc ?? id.ToString();
            var tenMh = mh?.TenMonHoc ?? "";

            await _monHocDichVu.XoaMonHocAsync(id, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Xóa",
                "MonHoc",
                maMh,
                $"Môn học: {tenMh}",
                null,
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Xóa môn học '{tenMh}' thành công!";
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
