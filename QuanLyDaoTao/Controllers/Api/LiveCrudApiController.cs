using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.Api;
using QuanLyDaoTao.Models.QuanTri;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers.Api;

/// <summary>
/// Bộ điều khiển RESTful API cho công cụ Quản trị Dữ liệu Trực tiếp (Live CRUD Studio) trên tất cả 11 nhóm hệ thống.
/// </summary>
[ApiController]
[Route("api/v1/live-crud")]
[Authorize(Roles = VaiTroMacDinh.QuanTriVien)]
[Produces("application/json")]
public class LiveCrudApiController : ControllerBase
{
    private readonly IQuanTriDuLieuDichVu _quanTriDuLieuDichVu;

    /// <summary>
    /// Khởi tạo LiveCrudApiController.
    /// </summary>
    public LiveCrudApiController(IQuanTriDuLieuDichVu quanTriDuLieuDichVu)
    {
        ArgumentNullException.ThrowIfNull(quanTriDuLieuDichVu);
        _quanTriDuLieuDichVu = quanTriDuLieuDichVu;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ 11 nhóm bảng dữ liệu trong hệ thống kèm số bản ghi.
    /// </summary>
    [HttpGet("tables")]
    [ProducesResponseType(typeof(ApiResponse<List<ThongTinBangHeThong>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LayDanhSachBang()
    {
        var ds = await _quanTriDuLieuDichVu.LayDanhSachBangHeThongAsync(HttpContext.RequestAborted);
        return Ok(ApiResponse<List<ThongTinBangHeThong>>.ThanhCongResponse(ds));
    }

    /// <summary>
    /// Lấy cấu trúc cột (gốc + tùy chỉnh) và toàn bộ các dòng dữ liệu của một bảng để hiển thị và sửa trực tiếp.
    /// </summary>
    [HttpGet("data/{table}")]
    [ProducesResponseType(typeof(ApiResponse<DuLieuBangTrucTuyen>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LayDuLieuBang(string table, [FromQuery] string? tuKhoa = null)
    {
        var duLieu = await _quanTriDuLieuDichVu.LayDuLieuBangTrucTuyenAsync(table, tuKhoa, HttpContext.RequestAborted);
        return Ok(ApiResponse<DuLieuBangTrucTuyen>.ThanhCongResponse(duLieu));
    }

    /// <summary>
    /// Cập nhật trực tiếp một bản ghi (gồm cả trường gốc và trường tùy chỉnh) ngay lập tức vào cơ sở dữ liệu.
    /// </summary>
    [HttpPost("update")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CapNhatNhanh([FromBody] YeuCauCapNhatNhanhDto yeuCau)
    {
        if (yeuCau == null || string.IsNullOrWhiteSpace(yeuCau.TenBang) || string.IsNullOrWhiteSpace(yeuCau.KhoaBanGhi))
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Dữ liệu cập nhật không hợp lệ.", 400));
        }

        try
        {
            await _quanTriDuLieuDichVu.CapNhatNhanhBanGhiAsync(
                yeuCau.TenBang,
                yeuCau.KhoaBanGhi,
                yeuCau.DuLieu,
                User.Identity?.Name ?? "API",
                HttpContext.RequestAborted);

            return Ok(ApiResponse<object?>.ThanhCongResponse(null, $"Đã cập nhật bản ghi #{yeuCau.KhoaBanGhi} bảng {yeuCau.TenBang}!"));
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException || ex is ArgumentException)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse(ex.Message, 400));
        }
    }

    /// <summary>
    /// Thêm mới nhanh một bản ghi trực tiếp vào bảng kèm các trường tùy chỉnh.
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    public async Task<IActionResult> ThemNhanh([FromBody] YeuCauThemNhanhDto yeuCau)
    {
        if (yeuCau == null || string.IsNullOrWhiteSpace(yeuCau.TenBang))
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Dữ liệu thêm mới không hợp lệ.", 400));
        }

        try
        {
            var idMoi = await _quanTriDuLieuDichVu.ThemNhanhBanGhiAsync(
                yeuCau.TenBang,
                yeuCau.DuLieu,
                User.Identity?.Name ?? "API",
                HttpContext.RequestAborted);

            return Created("", ApiResponse<object>.ThanhCongResponse(new { KhoaBanGhi = idMoi }, $"Đã thêm bản ghi #{idMoi} vào bảng {yeuCau.TenBang}!", 201));
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse(ex.Message, 400));
        }
    }

    /// <summary>
    /// Xóa một bản ghi khỏi bảng (có kiểm tra toàn vẹn dữ liệu).
    /// </summary>
    [HttpPost("delete")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> XoaNhanh([FromBody] YeuCauXoaNhanhDto yeuCau)
    {
        if (yeuCau == null || string.IsNullOrWhiteSpace(yeuCau.TenBang) || string.IsNullOrWhiteSpace(yeuCau.KhoaBanGhi))
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Dữ liệu xóa không hợp lệ.", 400));
        }

        try
        {
            await _quanTriDuLieuDichVu.XoaNhanhBanGhiAsync(
                yeuCau.TenBang,
                yeuCau.KhoaBanGhi,
                User.Identity?.Name ?? "API",
                HttpContext.RequestAborted);

            return Ok(ApiResponse<object?>.ThanhCongResponse(null, $"Đã xóa bản ghi #{yeuCau.KhoaBanGhi} khỏi bảng {yeuCau.TenBang}!"));
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException || ex is ArgumentException)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse(ex.Message, 400));
        }
    }
}
