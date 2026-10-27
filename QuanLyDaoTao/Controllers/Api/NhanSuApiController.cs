using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.Api;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers.Api;

/// <summary>
/// Bộ điều khiển RESTful API Quản lý Hồ sơ Cán bộ &amp; Giảng viên C500.
/// </summary>
[ApiController]
[Route("api/v1/nhansu")]
[Produces("application/json")]
public class NhanSuApiController : ControllerBase
{
    private readonly INhanSuDichVu _nhanSuDichVu;

    /// <summary>
    /// Khởi tạo NhanSuApiController.
    /// </summary>
    public NhanSuApiController(INhanSuDichVu nhanSuDichVu)
    {
        ArgumentNullException.ThrowIfNull(nhanSuDichVu);
        _nhanSuDichVu = nhanSuDichVu;
    }

    /// <summary>
    /// Tra cứu danh sách hồ sơ cán bộ theo nhiều tiêu chí lọc (Khoa, Bộ môn, Chức vụ, Trạng thái, Từ khóa).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<NhanSu>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LayDanhSach(
        [FromQuery] int? khoaId = null,
        [FromQuery] int? boMonId = null,
        [FromQuery] int? chucVuId = null,
        [FromQuery] string? trangThai = null,
        [FromQuery] string? tuKhoa = null,
        [FromQuery] bool? coHoatDong = null)
    {
        var danhSach = await _nhanSuDichVu.LayDanhSachAsync(khoaId, boMonId, chucVuId, trangThai, tuKhoa, coHoatDong, HttpContext.RequestAborted);
        return Ok(ApiResponse<List<NhanSu>>.ThanhCongResponse(danhSach, $"Tìm thấy {danhSach.Count} cán bộ / giảng viên."));
    }

    /// <summary>
    /// Lấy chi tiết toàn diện 7 nhóm trường của một hồ sơ cán bộ theo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<NhanSu>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LayTheoId(int id)
    {
        var ns = await _nhanSuDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (ns == null)
        {
            return NotFound(ApiResponse<object>.ThatBaiResponse($"Không tìm thấy hồ sơ cán bộ ID {id}.", 404));
        }

        return Ok(ApiResponse<NhanSu>.ThanhCongResponse(ns));
    }

    /// <summary>
    /// Tạo mới một hồ sơ cán bộ / giảng viên.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ProducesResponseType(typeof(ApiResponse<NhanSu>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ThemNhanSu([FromBody] NhanSu nhanSu)
    {
        if (nhanSu == null || !ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Dữ liệu hồ sơ cán bộ không hợp lệ.", 400));
        }

        try
        {
            var ketQua = await _nhanSuDichVu.ThemNhanSuAsync(nhanSu, User.Identity?.Name ?? "API", HttpContext.RequestAborted);
            return CreatedAtAction(nameof(LayTheoId), new { id = ketQua.NhanSuId }, ApiResponse<NhanSu>.ThanhCongResponse(ketQua, "Tạo hồ sơ cán bộ thành công.", 201));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse(ex.Message, 400));
        }
    }

    /// <summary>
    /// Cập nhật hồ sơ cán bộ hiện có.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ProducesResponseType(typeof(ApiResponse<NhanSu>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CapNhatNhanSu(int id, [FromBody] NhanSu nhanSu)
    {
        if (nhanSu == null || id != nhanSu.NhanSuId || !ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Thông tin cập nhật không khớp hoặc không hợp lệ.", 400));
        }

        try
        {
            var ketQua = await _nhanSuDichVu.CapNhatNhanSuAsync(nhanSu, User.Identity?.Name ?? "API", HttpContext.RequestAborted);
            return Ok(ApiResponse<NhanSu>.ThanhCongResponse(ketQua, "Cập nhật hồ sơ thành công."));
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
    /// Xóa hồ sơ cán bộ nếu không còn ràng buộc phân công.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> XoaNhanSu(int id)
    {
        try
        {
            await _nhanSuDichVu.XoaNhanSuAsync(id, User.Identity?.Name ?? "API", HttpContext.RequestAborted);
            return Ok(ApiResponse<object?>.ThanhCongResponse(null, "Xóa hồ sơ cán bộ thành công."));
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
