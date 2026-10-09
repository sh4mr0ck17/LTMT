using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển hiển thị Cây tổ chức và sơ đồ phân cấp đào tạo.
/// </summary>
public class CayToChucController : Controller
{
    private readonly QuanLyDaoTaoDbContext _coSoDuLieu;

    /// <summary>
    /// Khởi tạo bộ điều khiển cây tổ chức với DbContext.
    /// </summary>
    /// <param name="coSoDuLieu">Ngữ cảnh cơ sở dữ liệu.</param>
    public CayToChucController(QuanLyDaoTaoDbContext coSoDuLieu)
    {
        ArgumentNullException.ThrowIfNull(coSoDuLieu);
        _coSoDuLieu = coSoDuLieu;
    }

    /// <summary>
    /// Hiển thị sơ đồ cây tổ chức trực quan có thể thu gọn / mở rộng các cấp.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var danhSachKhoa = await _coSoDuLieu.Khoas
            .Include(k => k.BoMons)
                .ThenInclude(bm => bm.NhanSus)
            .Include(k => k.BoMons)
                .ThenInclude(bm => bm.MonHocs)
            .Include(k => k.LopHocs)
                .ThenInclude(l => l.HocViens)
            .Include(k => k.NhanSus)
                .ThenInclude(ns => ns.ChucVu)
            .AsNoTracking()
            .OrderBy(k => k.MaKhoa)
            .ToListAsync(HttpContext.RequestAborted);

        return View(danhSachKhoa);
    }
}
