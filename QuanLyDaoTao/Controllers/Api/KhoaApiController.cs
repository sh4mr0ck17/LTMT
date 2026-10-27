using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.Api;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers.Api;

/// <summary>
/// Bộ điều khiển RESTful API Quản lý Khoa Đào tạo.
/// </summary>
[ApiController]
[Route("api/v1/khoa")]
[Produces("application/json")]
public class KhoaApiController : ControllerBase
{
    private readonly IKhoaDichVu _khoaDichVu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo KhoaApiController.
    /// </summary>
    public KhoaApiController(IKhoaDichVu khoaDichVu, INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(khoaDichVu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _khoaDichVu = khoaDichVu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Lấy danh sách toàn bộ khoa có hỗ trợ lọc và tìm kiếm từ khóa.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<Khoa>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LayDanhSach([FromQuery] string? tuKhoa = null, [FromQuery] bool? coHoatDong = null)
    {
        var danhSach = await _khoaDichVu.LayDanhSachAsync(tuKhoa, coHoatDong, HttpContext.RequestAborted);
        return Ok(ApiResponse<List<Khoa>>.ThanhCongResponse(danhSach, $"Tìm thấy {danhSach.Count} khoa đào tạo."));
    }

    /// <summary>
    /// Lấy thông tin chi tiết một khoa theo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<Khoa>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LayTheoId(int id)
    {
        var khoa = await _khoaDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (khoa == null)
        {
            return NotFound(ApiResponse<object>.ThatBaiResponse($"Không tìm thấy khoa có ID {id}.", 404));
        }

        return Ok(ApiResponse<Khoa>.ThanhCongResponse(khoa));
    }

    /// <summary>
    /// Thêm mới một khoa đào tạo.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ProducesResponseType(typeof(ApiResponse<Khoa>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ThemKhoa([FromBody] Khoa khoa)
    {
        if (khoa == null || !ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Dữ liệu khoa không hợp lệ.", 400));
        }

        try
        {
            var ketQua = await _khoaDichVu.ThemKhoaAsync(khoa, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "API",
                "Thêm mới qua API",
                "Khoa",
                khoa.MaKhoa,
                null,
                $"Tên khoa: {khoa.TenKhoa}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            return CreatedAtAction(nameof(LayTheoId), new { id = ketQua.KhoaId }, ApiResponse<Khoa>.ThanhCongResponse(ketQua, "Thêm mới khoa thành công.", 201));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse(ex.Message, 400));
        }
    }

    /// <summary>
    /// Cập nhật thông tin một khoa đào tạo.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ProducesResponseType(typeof(ApiResponse<Khoa>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CapNhatKhoa(int id, [FromBody] Khoa khoa)
    {
        if (khoa == null || id != khoa.KhoaId || !ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Thông tin cập nhật không khớp hoặc không hợp lệ.", 400));
        }

        try
        {
            var ketQua = await _khoaDichVu.CapNhatKhoaAsync(khoa, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "API",
                "Cập nhật qua API",
                "Khoa",
                khoa.MaKhoa,
                null,
                $"Cập nhật khoa: {khoa.TenKhoa}",
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            return Ok(ApiResponse<Khoa>.ThanhCongResponse(ketQua, "Cập nhật khoa thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ThatBaiResponse(ex.Message, 404));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse(ex.Message, 400));
        }
    }

    /// <summary>
    /// Xóa một khoa đào tạo nếu không còn dữ liệu phụ thuộc.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> XoaKhoa(int id)
    {
        try
        {
            var khoa = await _khoaDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
            await _khoaDichVu.XoaKhoaAsync(id, HttpContext.RequestAborted);

            await _nhatKyDichVu.GhiNhatKyAsync(
                User.Identity?.Name ?? "API",
                "Xóa qua API",
                "Khoa",
                khoa?.MaKhoa ?? id.ToString(),
                $"Khoa: {khoa?.TenKhoa}",
                null,
                "Thành công",
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.RequestAborted);

            return Ok(ApiResponse<object?>.ThanhCongResponse(null, "Xóa khoa thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ThatBaiResponse(ex.Message, 404));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse(ex.Message, 400));
        }
    }
}
