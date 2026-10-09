using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển trang chủ và bảng điều khiển tổng quan (Dashboard) hệ thống.
/// </summary>
public class HomeController : Controller
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển trang chủ với DbContext và dịch vụ nhật ký.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    /// <param name="nhatKyDichVu">Dịch vụ nhật ký kiểm toán.</param>
    public HomeController(QuanLyDaoTaoDbContext coSoDuLieu, INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _coSoDuLieu = coSoDuLieu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Hiển thị bảng điều khiển tổng quan với thống kê số liệu và hoạt động gần đây.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        ViewBag.TongSoKhoa = await _coSoDuLieu.Khoas.CountAsync();
        ViewBag.TongSoBoMon = await _coSoDuLieu.BoMons.CountAsync();
        ViewBag.TongSoLop = await _coSoDuLieu.LopHocs.CountAsync();
        ViewBag.TongSoHocVien = await _coSoDuLieu.HocViens.CountAsync();
        ViewBag.TongSoNhanSu = await _coSoDuLieu.NhanSus.CountAsync();
        ViewBag.TongSoMonHoc = await _coSoDuLieu.MonHocs.CountAsync();
        ViewBag.TongSoPhanCong = await _coSoDuLieu.PhanCongGiangDays.CountAsync();

        ViewBag.HoatDongGanDay = await _nhatKyDichVu.LayDanhSachNhatKyAsync(null, null, 10);

        return View();
    }

    /// <summary>
    /// Hiển thị trang xử lý lỗi hệ thống chung.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
