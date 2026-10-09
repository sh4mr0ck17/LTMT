using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các thao tác CRUD và quản trị Phân công giảng dạy.
/// </summary>
public class PhanCongGiangDayController : Controller
{
    private readonly IPhanCongGiangDayDichVu _phanCongGiangDayDichVu;
    private readonly INhanSuDichVu _nhanSuDichVu;
    private readonly IMonHocDichVu _monHocDichVu;
    private readonly ILopHocDichVu _lopHocDichVu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển phân công giảng dạy với các dịch vụ nghiệp vụ.
    /// </summary>
    public PhanCongGiangDayController(
        IPhanCongGiangDayDichVu phanCongGiangDayDichVu,
        INhanSuDichVu nhanSuDichVu,
        IMonHocDichVu monHocDichVu,
        ILopHocDichVu lopHocDichVu,
        INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(phanCongGiangDayDichVu);
        ArgumentNullException.ThrowIfNull(nhanSuDichVu);
        ArgumentNullException.ThrowIfNull(monHocDichVu);
        ArgumentNullException.ThrowIfNull(lopHocDichVu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _phanCongGiangDayDichVu = phanCongGiangDayDichVu;
        _nhanSuDichVu = nhanSuDichVu;
        _monHocDichVu = monHocDichVu;
        _lopHocDichVu = lopHocDichVu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Hiển thị danh sách phân công giảng dạy có lọc và tìm kiếm.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(
        int? nhanSuId = null,
        int? monHocId = null,
        int? lopHocId = null,
        string? hocKy = null,
        string? namHoc = null,
        string? tuKhoa = null)
    {
        ViewBag.NhanSuId = nhanSuId;
        ViewBag.MonHocId = monHocId;
        ViewBag.LopHocId = lopHocId;
        ViewBag.HocKy = hocKy;
        ViewBag.NamHoc = namHoc;
        ViewBag.TuKhoa = tuKhoa;

        await NapDuLieuDanhMucViewBagAsync();
        var danhSach = await _phanCongGiangDayDichVu.LayDanhSachAsync(nhanSuId, monHocId, lopHocId, hocKy, namHoc, tuKhoa, HttpContext.RequestAborted);
        return View(danhSach);
    }

    /// <summary>
    /// Hiển thị biểu mẫu thêm mới một phân công giảng dạy.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ThemMoi()
    {
        await NapDuLieuDanhMucViewBagAsync();
        return View(new PhanCongGiangDay
        {
            HocKy = "Học kỳ 1",
            NamHoc = "2026-2027",
            VaiTroGiangDay = "Giảng viên chính",
            LaChuNhiem = false
        });
    }

    /// <summary>
    /// Xử lý lưu phân công giảng dạy mới.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemMoi(PhanCongGiangDay phanCong)
    {
        ArgumentNullException.ThrowIfNull(phanCong);

        if (!ModelState.IsValid)
        {
            await NapDuLieuDanhMucViewBagAsync();
            return View(phanCong);
        }

        try
        {
            await _phanCongGiangDayDichVu.ThemPhanCongAsync(phanCong, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Thêm mới",
                "PhanCongGiangDay",
                phanCong.MaPhanCong,
                null,
                $"Phân công: {phanCong.MaPhanCong}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Thêm mới phân công giảng dạy '{phanCong.MaPhanCong}' thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await NapDuLieuDanhMucViewBagAsync();
            return View(phanCong);
        }
    }

    /// <summary>
    /// Hiển thị biểu mẫu chỉnh sửa phân công giảng dạy.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ChinhSua(int id)
    {
        var pc = await _phanCongGiangDayDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (pc == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy phân công có mã số {id}.";
            return RedirectToAction(nameof(Index));
        }

        await NapDuLieuDanhMucViewBagAsync();
        return View(pc);
    }

    /// <summary>
    /// Xử lý cập nhật thông tin phân công giảng dạy.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChinhSua(int id, PhanCongGiangDay phanCong)
    {
        ArgumentNullException.ThrowIfNull(phanCong);

        if (id != phanCong.PhanCongId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await NapDuLieuDanhMucViewBagAsync();
            return View(phanCong);
        }

        try
        {
            await _phanCongGiangDayDichVu.CapNhatPhanCongAsync(phanCong, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Cập nhật",
                "PhanCongGiangDay",
                phanCong.MaPhanCong,
                null,
                $"Cập nhật phân công: {phanCong.MaPhanCong}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Cập nhật phân công '{phanCong.MaPhanCong}' thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await NapDuLieuDanhMucViewBagAsync();
            return View(phanCong);
        }
    }

    /// <summary>
    /// Xóa một phân công giảng dạy khỏi hệ thống.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            var pc = await _phanCongGiangDayDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
            var maPc = pc?.MaPhanCong ?? id.ToString();

            await _phanCongGiangDayDichVu.XoaPhanCongAsync(id, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "Quản trị viên",
                "Xóa",
                "PhanCongGiangDay",
                maPc,
                $"Phân công: {maPc}",
                null,
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Xóa phân công '{maPc}' thành công!";
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ThongBaoLoi"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task NapDuLieuDanhMucViewBagAsync()
    {
        ViewBag.DanhSachNhanSu = await _nhanSuDichVu.LayDanhSachAsync(null, null, null, null, null, true, HttpContext.RequestAborted);
        ViewBag.DanhSachMonHoc = await _monHocDichVu.LayDanhSachAsync(null, null, true, HttpContext.RequestAborted);
        ViewBag.DanhSachLop = await _lopHocDichVu.LayDanhSachAsync(null, null, null, true, HttpContext.RequestAborted);
    }
}
