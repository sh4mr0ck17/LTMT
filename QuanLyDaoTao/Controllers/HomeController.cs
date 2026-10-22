using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Services;
using QuanLyDaoTao.ViewModels.ThongKe;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển trang chủ và bảng điều khiển tổng quan (Dashboard) hệ thống kèm báo cáo biểu đồ (Diagrams).
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
    /// Hiển thị bảng điều khiển tổng quan với thống kê số liệu, biểu đồ trực quan và hoạt động gần đây.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var model = new BaoCaoDiagramViewModel
        {
            TongSoKhoa = await _coSoDuLieu.Khoas.CountAsync(),
            TongSoBoMon = await _coSoDuLieu.BoMons.CountAsync(),
            TongSoLop = await _coSoDuLieu.LopHocs.CountAsync(),
            TongSoHocVien = await _coSoDuLieu.HocViens.CountAsync(),
            TongSoNhanSu = await _coSoDuLieu.NhanSus.CountAsync(),
            TongSoMonHoc = await _coSoDuLieu.MonHocs.CountAsync(),
            TongSoPhanCong = await _coSoDuLieu.PhanCongGiangDays.CountAsync()
        };

        // 1. Thống kê theo Khoa (Số lượng học viên & lớp học)
        var khoas = await _coSoDuLieu.Khoas
            .Include(k => k.LopHocs)
                .ThenInclude(l => l.HocViens)
            .Include(k => k.NhanSus)
            .AsNoTracking()
            .ToListAsync();

        foreach (var k in khoas)
        {
            model.DanhSachKhoa.Add(new ThongKeKhoaItem
            {
                TenKhoa = k.TenKhoa,
                SoLop = k.LopHocs.Count,
                SoHocVien = k.LopHocs.Sum(l => l.HocViens.Count),
                SoNhanSu = k.NhanSus.Count
            });
        }

        // 2. Thống kê theo Trình độ chuyên môn
        var trinhDos = await _coSoDuLieu.TrinhDoChuyenMons
            .Include(t => t.NhanSus)
            .AsNoTracking()
            .ToListAsync();

        foreach (var td in trinhDos)
        {
            model.DanhSachTrinhDo.Add(new ThongKeTrinhDoItem
            {
                TenTrinhDo = td.TenTrinhDo,
                SoLuong = td.NhanSus.Count
            });
        }

        // 3. Thống kê theo Chức vụ công tác
        var chucVus = await _coSoDuLieu.ChucVus
            .Include(c => c.NhanSus)
            .AsNoTracking()
            .ToListAsync();

        foreach (var cv in chucVus)
        {
            model.DanhSachChucVu.Add(new ThongKeChucVuItem
            {
                TenChucVu = cv.TenChucVu,
                SoLuong = cv.NhanSus.Count
            });
        }

        // 4. Thống kê theo Lớp học
        var lops = await _coSoDuLieu.LopHocs
            .Include(l => l.HocViens)
            .AsNoTracking()
            .Take(10)
            .ToListAsync();

        foreach (var l in lops)
        {
            model.DanhSachLop.Add(new ThongKeLopItem
            {
                TenLop = l.TenLop,
                SoHocVien = l.HocViens.Count,
                SoLuongToiDa = l.SoLuongToiDa
            });
        }

        // Truyền các biến ViewBag để tương thích ngược với layout/tests hiện có
        ViewBag.TongSoKhoa = model.TongSoKhoa;
        ViewBag.TongSoBoMon = model.TongSoBoMon;
        ViewBag.TongSoLop = model.TongSoLop;
        ViewBag.TongSoHocVien = model.TongSoHocVien;
        ViewBag.TongSoNhanSu = model.TongSoNhanSu;
        ViewBag.TongSoMonHoc = model.TongSoMonHoc;
        ViewBag.TongSoPhanCong = model.TongSoPhanCong;

        ViewBag.HoatDongGanDay = await _nhatKyDichVu.LayDanhSachNhatKyAsync(null, null, 10);

        return View(model);
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
