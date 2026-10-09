using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các thao tác CRUD và hồ sơ chi tiết Nhân sự / Giảng viên.
/// </summary>
public class NhanSuController : Controller
{
    private readonly INhanSuDichVu _nhanSuDichVu;
    private readonly IKhoaDichVu _khoaDichVu;
    private readonly IBoMonDichVu _boMonDichVu;
    private readonly IChucVuDichVu _chucVuDichVu;
    private readonly ITrinhDoChuyenMonDichVu _trinhDoChuyenMonDichVu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển nhân sự với các dịch vụ nghiệp vụ cần thiết.
    /// </summary>
    public NhanSuController(
        INhanSuDichVu nhanSuDichVu,
        IKhoaDichVu khoaDichVu,
        IBoMonDichVu boMonDichVu,
        IChucVuDichVu chucVuDichVu,
        ITrinhDoChuyenMonDichVu trinhDoChuyenMonDichVu,
        INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(nhanSuDichVu);
        ArgumentNullException.ThrowIfNull(khoaDichVu);
        ArgumentNullException.ThrowIfNull(boMonDichVu);
        ArgumentNullException.ThrowIfNull(chucVuDichVu);
        ArgumentNullException.ThrowIfNull(trinhDoChuyenMonDichVu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _nhanSuDichVu = nhanSuDichVu;
        _khoaDichVu = khoaDichVu;
        _boMonDichVu = boMonDichVu;
        _chucVuDichVu = chucVuDichVu;
        _trinhDoChuyenMonDichVu = trinhDoChuyenMonDichVu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Hiển thị danh sách hồ sơ nhân sự có bộ lọc đa tiêu chí và tìm kiếm.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(
        int? khoaId = null,
        int? boMonId = null,
        int? chucVuId = null,
        string? trangThai = null,
        string? tuKhoa = null,
        bool? coHoatDong = null)
    {
        ViewBag.KhoaId = khoaId;
        ViewBag.BoMonId = boMonId;
        ViewBag.ChucVuId = chucVuId;
        ViewBag.TrangThai = trangThai;
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.CoHoatDong = coHoatDong;

        ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
        ViewBag.DanhSachBoMon = await _boMonDichVu.LayDanhSachAsync(khoaId, null, true, HttpContext.RequestAborted);
        ViewBag.DanhSachChucVu = await _chucVuDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
        ViewBag.DanhSachTrangThai = await _nhanSuDichVu.LayDanhSachTrangThaiCongTacAsync(HttpContext.RequestAborted);

        var danhSach = await _nhanSuDichVu.LayDanhSachAsync(khoaId, boMonId, chucVuId, trangThai, tuKhoa, coHoatDong, HttpContext.RequestAborted);
        return View(danhSach);
    }

    /// <summary>
    /// Hiển thị hồ sơ chi tiết nhân sự nhiều tab (Thông tin cá nhân, Công tác, Chuyên môn, Phân công giảng dạy).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ChiTiet(int id)
    {
        var nhanSu = await _nhanSuDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (nhanSu == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy hồ sơ nhân sự có ID {id}.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.CoTheXoa = await _nhanSuDichVu.CoTheXoaAsync(id, HttpContext.RequestAborted);
        return View(nhanSu);
    }

    /// <summary>
    /// Hiển thị biểu mẫu thêm mới một hồ sơ nhân sự.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ThemMoi()
    {
        await NapDuLieuDanhMucViewBagAsync();
        return View(new NhanSu
        {
            CoHoatDong = true,
            NgaySinh = DateTime.Today.AddYears(-30),
            GioiTinh = "Nam",
            TrangThaiCongTac = "Đang làm việc",
            LoaiHopDong = "Chính thức"
        });
    }

    /// <summary>
    /// Xử lý lưu hồ sơ nhân sự mới vào hệ thống.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemMoi(NhanSu nhanSu)
    {
        ArgumentNullException.ThrowIfNull(nhanSu);

        if (!ModelState.IsValid)
        {
            await NapDuLieuDanhMucViewBagAsync();
            return View(nhanSu);
        }

        try
        {
            await _nhanSuDichVu.ThemNhanSuAsync(nhanSu, User.Identity?.Name, HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Thêm mới nhân sự '{nhanSu.HoVaTen}' ({nhanSu.MaNhanSu}) thành công!";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            await NapDuLieuDanhMucViewBagAsync();
            return View(nhanSu);
        }
    }

    /// <summary>
    /// Hiển thị biểu mẫu chỉnh sửa hồ sơ nhân sự.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ChinhSua(int id)
    {
        var nhanSu = await _nhanSuDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (nhanSu == null)
        {
            TempData["ThongBaoLoi"] = $"Không tìm thấy hồ sơ nhân sự có ID {id}.";
            return RedirectToAction(nameof(Index));
        }

        await NapDuLieuDanhMucViewBagAsync();
        return View(nhanSu);
    }

    /// <summary>
    /// Xử lý cập nhật hồ sơ nhân sự.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChinhSua(int id, NhanSu nhanSu)
    {
        ArgumentNullException.ThrowIfNull(nhanSu);

        if (id != nhanSu.NhanSuId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await NapDuLieuDanhMucViewBagAsync();
            return View(nhanSu);
        }

        try
        {
            await _nhanSuDichVu.CapNhatNhanSuAsync(nhanSu, User.Identity?.Name, HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Cập nhật hồ sơ '{nhanSu.HoVaTen}' thành công!";
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
            return View(nhanSu);
        }
    }

    /// <summary>
    /// Xóa một hồ sơ nhân sự khỏi hệ thống.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            var ns = await _nhanSuDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
            var hoTen = ns?.HoVaTen ?? "";

            await _nhanSuDichVu.XoaNhanSuAsync(id, User.Identity?.Name, HttpContext.RequestAborted);

            TempData["ThongBaoThanhCong"] = $"Xóa hồ sơ nhân sự '{hoTen}' thành công!";
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

    private async Task NapDuLieuDanhMucViewBagAsync()
    {
        ViewBag.DanhSachKhoa = await _khoaDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
        ViewBag.DanhSachBoMon = await _boMonDichVu.LayDanhSachAsync(null, null, true, HttpContext.RequestAborted);
        ViewBag.DanhSachChucVu = await _chucVuDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
        ViewBag.DanhSachTrinhDo = await _trinhDoChuyenMonDichVu.LayDanhSachAsync(null, true, HttpContext.RequestAborted);
        ViewBag.DanhSachTrangThai = await _nhanSuDichVu.LayDanhSachTrangThaiCongTacAsync(HttpContext.RequestAborted);
    }
}
