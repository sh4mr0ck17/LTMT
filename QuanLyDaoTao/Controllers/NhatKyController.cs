using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển tra cứu và xem Nhật ký kiểm toán hệ thống.
/// </summary>
[Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
public class NhatKyController : Controller
{
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển với dịch vụ nhật ký.
    /// </summary>
    /// <param name="nhatKyDichVu">Dịch vụ ghi và đọc nhật ký kiểm toán.</param>
    public NhatKyController(INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Hiển thị danh sách lịch sử thao tác và kiểm toán hệ thống.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? tuKhoa = null, string? bangDuLieu = null, int soLuong = 100)
    {
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.BangDuLieu = bangDuLieu;
        ViewBag.SoLuong = soLuong;

        var danhSach = await _nhatKyDichVu.LayDanhSachNhatKyAsync(tuKhoa, bangDuLieu, soLuong, HttpContext.RequestAborted);
        return View(danhSach);
    }
}
