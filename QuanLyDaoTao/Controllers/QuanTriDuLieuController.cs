using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Yêu cầu cập nhật một bản ghi trực tuyến từ giao diện Quản trị.
/// </summary>
public class YeuCauCapNhatNhanhDto
{
    /// <summary>
    /// Tên bảng dữ liệu cần cập nhật.
    /// </summary>
    public string TenBang { get; set; } = string.Empty;

    /// <summary>
    /// Khóa chính của bản ghi cần cập nhật.
    /// </summary>
    public string KhoaBanGhi { get; set; } = string.Empty;

    /// <summary>
    /// Dữ liệu các trường cần cập nhật.
    /// </summary>
    public Dictionary<string, string?> DuLieu { get; set; } = new();
}

/// <summary>
/// Yêu cầu cập nhật hàng loạt nhiều bản ghi cùng lúc.
/// </summary>
public class YeuCauCapNhatHangLoatDto
{
    /// <summary>
    /// Tên bảng dữ liệu.
    /// </summary>
    public string TenBang { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách các dòng cần cập nhật.
    /// </summary>
    public List<YeuCauCapNhatNhanhDto> DanhSachCapNhat { get; set; } = new();
}

/// <summary>
/// Yêu cầu thêm mới nhanh một bản ghi trực tiếp từ bảng.
/// </summary>
public class YeuCauThemNhanhDto
{
    /// <summary>
    /// Tên bảng dữ liệu.
    /// </summary>
    public string TenBang { get; set; } = string.Empty;

    /// <summary>
    /// Dữ liệu các trường của bản ghi mới.
    /// </summary>
    public Dictionary<string, string?> DuLieu { get; set; } = new();
}

/// <summary>
/// Yêu cầu xóa nhanh một bản ghi trực tiếp từ bảng.
/// </summary>
public class YeuCauXoaNhanhDto
{
    /// <summary>
    /// Tên bảng dữ liệu.
    /// </summary>
    public string TenBang { get; set; } = string.Empty;

    /// <summary>
    /// Khóa chính của bản ghi cần xóa.
    /// </summary>
    public string KhoaBanGhi { get; set; } = string.Empty;
}

/// <summary>
/// Yêu cầu xóa hàng loạt nhiều bản ghi trực tiếp từ bảng.
/// </summary>
public class YeuCauXoaHangLoatDto
{
    /// <summary>
    /// Tên bảng dữ liệu.
    /// </summary>
    public string TenBang { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách các khóa chính cần xóa.
    /// </summary>
    public List<string> DanhSachKhoaBanGhi { get; set; } = new();
}

/// <summary>
/// Bộ điều khiển Quản trị Dữ liệu Trực tuyến (Admin Live Manual CRUD &amp; Custom Schema Studio) dành riêng cho Administrator.
/// </summary>
[Authorize(Roles = VaiTroMacDinh.QuanTriVien)]
public class QuanTriDuLieuController : Controller
{
    private readonly IQuanTriDuLieuDichVu _quanTriDuLieuDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển Quản trị dữ liệu.
    /// </summary>
    public QuanTriDuLieuController(IQuanTriDuLieuDichVu quanTriDuLieuDichVu)
    {
        ArgumentNullException.ThrowIfNull(quanTriDuLieuDichVu);
        _quanTriDuLieuDichVu = quanTriDuLieuDichVu;
    }

    /// <summary>
    /// Hiển thị giao diện trung tâm Quản trị &amp; Tùy biến dữ liệu trực tiếp cho cả 11 nhóm trong hệ thống.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string bang = "Khoa", string? tuKhoa = null)
    {
        var bangChuan = QuanTriDuLieuDichVu.ChuanHoaTenBang(bang);
        ViewBag.DanhSachBang = await _quanTriDuLieuDichVu.LayDanhSachBangHeThongAsync(HttpContext.RequestAborted);
        ViewBag.TuKhoa = tuKhoa;

        var duLieuBang = await _quanTriDuLieuDichVu.LayDuLieuBangTrucTuyenAsync(bangChuan, tuKhoa, HttpContext.RequestAborted);
        return View(duLieuBang);
    }

    /// <summary>
    /// API trả về cấu trúc cột, trường tùy chỉnh và dữ liệu dòng dạng JSON để nhúng trực tiếp vào từng trang nhóm quản lý.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LayDuLieuBangJson(string bang = "Khoa", string? tuKhoa = null)
    {
        var duLieu = await _quanTriDuLieuDichVu.LayDuLieuBangTrucTuyenAsync(bang, tuKhoa, HttpContext.RequestAborted);
        return Json(duLieu);
    }

    /// <summary>
    /// API cập nhật trực tiếp một bản ghi xuống cơ sở dữ liệu ngay lập tức.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatNhanh([FromBody] YeuCauCapNhatNhanhDto yeuCau)
    {
        if (yeuCau == null || string.IsNullOrWhiteSpace(yeuCau.TenBang) || string.IsNullOrWhiteSpace(yeuCau.KhoaBanGhi))
        {
            return BadRequest(new { thanhCong = false, thongDiep = "Dữ liệu yêu cầu cập nhật không hợp lệ." });
        }

        try
        {
            await _quanTriDuLieuDichVu.CapNhatNhanhBanGhiAsync(
                yeuCau.TenBang,
                yeuCau.KhoaBanGhi,
                yeuCau.DuLieu,
                User.Identity?.Name ?? "Admin",
                HttpContext.RequestAborted);

            return Ok(new { thanhCong = true, thongDiep = $"Đã lưu thay đổi bản ghi #{yeuCau.KhoaBanGhi} trực tiếp vào DB!" });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException || ex is ArgumentException)
        {
            return BadRequest(new { thanhCong = false, thongDiep = ex.Message });
        }
    }

    /// <summary>
    /// API lưu hàng loạt nhiều dòng đã chỉnh sửa trên bảng xuống cơ sở dữ liệu.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatHangLoat([FromBody] YeuCauCapNhatHangLoatDto yeuCau)
    {
        if (yeuCau == null || string.IsNullOrWhiteSpace(yeuCau.TenBang) || yeuCau.DanhSachCapNhat.Count == 0)
        {
            return BadRequest(new { thanhCong = false, thongDiep = "Không có dữ liệu thay đổi để lưu." });
        }

        int soDongThanhCong = 0;
        try
        {
            foreach (var dong in yeuCau.DanhSachCapNhat)
            {
                await _quanTriDuLieuDichVu.CapNhatNhanhBanGhiAsync(
                    yeuCau.TenBang,
                    dong.KhoaBanGhi,
                    dong.DuLieu,
                    User.Identity?.Name ?? "Admin",
                    HttpContext.RequestAborted);
                soDongThanhCong++;
            }

            return Ok(new { thanhCong = true, thongDiep = $"Đã áp dụng thành công {soDongThanhCong} bản ghi trực tiếp lên DB!" });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException || ex is ArgumentException)
        {
            return BadRequest(new { thanhCong = false, thongDiep = $"Lỗi sau khi lưu {soDongThanhCong} dòng: {ex.Message}" });
        }
    }

    /// <summary>
    /// API thêm mới trực tiếp một bản ghi vào bảng và trả về ID mới.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemNhanh([FromBody] YeuCauThemNhanhDto yeuCau)
    {
        if (yeuCau == null || string.IsNullOrWhiteSpace(yeuCau.TenBang))
        {
            return BadRequest(new { thanhCong = false, thongDiep = "Dữ liệu thêm mới không hợp lệ." });
        }

        try
        {
            var idMoi = await _quanTriDuLieuDichVu.ThemNhanhBanGhiAsync(
                yeuCau.TenBang,
                yeuCau.DuLieu,
                User.Identity?.Name ?? "Admin",
                HttpContext.RequestAborted);

            return Ok(new { thanhCong = true, khoaBanGhi = idMoi, thongDiep = $"Đã thêm mới bản ghi #{idMoi} trực tiếp vào DB!" });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
        {
            return BadRequest(new { thanhCong = false, thongDiep = ex.Message });
        }
    }

    /// <summary>
    /// API xóa trực tiếp một bản ghi khỏi cơ sở dữ liệu.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaNhanh([FromBody] YeuCauXoaNhanhDto yeuCau)
    {
        if (yeuCau == null || string.IsNullOrWhiteSpace(yeuCau.TenBang) || string.IsNullOrWhiteSpace(yeuCau.KhoaBanGhi))
        {
            return BadRequest(new { thanhCong = false, thongDiep = "Thông tin bản ghi cần xóa không hợp lệ." });
        }

        try
        {
            await _quanTriDuLieuDichVu.XoaNhanhBanGhiAsync(
                yeuCau.TenBang,
                yeuCau.KhoaBanGhi,
                User.Identity?.Name ?? "Admin",
                HttpContext.RequestAborted);

            return Ok(new { thanhCong = true, thongDiep = $"Đã xóa bản ghi #{yeuCau.KhoaBanGhi} khỏi DB!" });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException || ex is ArgumentException)
        {
            return BadRequest(new { thanhCong = false, thongDiep = ex.Message });
        }
    }

    /// <summary>
    /// API xóa hàng loạt nhiều bản ghi đã chọn khỏi cơ sở dữ liệu.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaHangLoat([FromBody] YeuCauXoaHangLoatDto yeuCau)
    {
        if (yeuCau == null || string.IsNullOrWhiteSpace(yeuCau.TenBang) || yeuCau.DanhSachKhoaBanGhi.Count == 0)
        {
            return BadRequest(new { thanhCong = false, thongDiep = "Chưa chọn bản ghi nào để xóa." });
        }

        int daXoa = 0;
        try
        {
            foreach (var pk in yeuCau.DanhSachKhoaBanGhi)
            {
                if (!string.IsNullOrWhiteSpace(pk))
                {
                    await _quanTriDuLieuDichVu.XoaNhanhBanGhiAsync(
                        yeuCau.TenBang,
                        pk,
                        User.Identity?.Name ?? "Admin",
                        HttpContext.RequestAborted);
                    daXoa++;
                }
            }

            return Ok(new { thanhCong = true, soLuongDaXoa = daXoa, thongDiep = $"Đã xóa thành công {daXoa} bản ghi khỏi DB!" });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException || ex is ArgumentException)
        {
            return BadRequest(new { thanhCong = false, thongDiep = $"Đã xóa {daXoa} dòng, dừng tại lỗi: {ex.Message}" });
        }
    }

    /// <summary>
    /// Thêm mới một trường (cột) tùy chỉnh cho bảng.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemTruongTuyChinh([FromBody] TruongTuyChinh truongMoi)
    {
        if (truongMoi == null)
        {
            return BadRequest(new { thanhCong = false, thongDiep = "Thông tin trường tùy chỉnh không hợp lệ." });
        }

        try
        {
            var ketQua = await _quanTriDuLieuDichVu.ThemTruongTuyChinhAsync(
                truongMoi,
                User.Identity?.Name ?? "Admin",
                HttpContext.RequestAborted);

            return Ok(new { thanhCong = true, truong = ketQua, thongDiep = $"Đã thêm trường '{ketQua.TenHienThi}' vào bảng {ketQua.TenBang}!" });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
        {
            return BadRequest(new { thanhCong = false, thongDiep = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật thông tin cấu hình một trường (cột) tùy chỉnh.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CapNhatTruongTuyChinh([FromBody] TruongTuyChinh truongCapNhat)
    {
        if (truongCapNhat == null || truongCapNhat.TruongTuyChinhId <= 0)
        {
            return BadRequest(new { thanhCong = false, thongDiep = "Thông tin trường cần sửa không hợp lệ." });
        }

        try
        {
            var ketQua = await _quanTriDuLieuDichVu.CapNhatTruongTuyChinhAsync(
                truongCapNhat,
                User.Identity?.Name ?? "Admin",
                HttpContext.RequestAborted);

            return Ok(new { thanhCong = true, truong = ketQua, thongDiep = $"Đã cập nhật trường '{ketQua.TenHienThi}'!" });
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            return BadRequest(new { thanhCong = false, thongDiep = ex.Message });
        }
    }

    /// <summary>
    /// Xóa một trường (cột) tùy chỉnh khỏi bảng.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaTruongTuyChinh([FromBody] int truongTuyChinhId)
    {
        if (truongTuyChinhId <= 0)
        {
            return BadRequest(new { thanhCong = false, thongDiep = "ID trường tùy chỉnh không hợp lệ." });
        }

        try
        {
            await _quanTriDuLieuDichVu.XoaTruongTuyChinhAsync(
                truongTuyChinhId,
                User.Identity?.Name ?? "Admin",
                HttpContext.RequestAborted);

            return Ok(new { thanhCong = true, thongDiep = "Đã xóa trường tùy chỉnh khỏi hệ thống!" });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { thanhCong = false, thongDiep = ex.Message });
        }
    }
}
