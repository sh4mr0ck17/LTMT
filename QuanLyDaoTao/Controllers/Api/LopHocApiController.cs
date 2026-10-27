using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.Api;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers.Api;

/// <summary>
/// Bộ điều khiển RESTful API Quản lý Lớp học Đào tạo.
/// </summary>
[ApiController]
[Route("api/v1/lophoc")]
[Produces("application/json")]
public class LopHocApiController : ControllerBase
{
    private readonly ILopHocDichVu _lopHocDichVu;

    /// <summary>
    /// Khởi tạo LopHocApiController.
    /// </summary>
    public LopHocApiController(ILopHocDichVu lopHocDichVu)
    {
        ArgumentNullException.ThrowIfNull(lopHocDichVu);
        _lopHocDichVu = lopHocDichVu;
    }

    /// <summary>
    /// Lấy danh sách lớp học theo khoa, niên khóa và từ khóa.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<LopHoc>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LayDanhSach(
        [FromQuery] int? khoaId = null,
        [FromQuery] string? nienKhoa = null,
        [FromQuery] string? tuKhoa = null,
        [FromQuery] bool? coHoatDong = null)
    {
        var danhSach = await _lopHocDichVu.LayDanhSachAsync(khoaId, nienKhoa, tuKhoa, coHoatDong, HttpContext.RequestAborted);
        return Ok(ApiResponse<List<LopHoc>>.ThanhCongResponse(danhSach, $"Tìm thấy {danhSach.Count} lớp học."));
    }

    /// <summary>
    /// Lấy chi tiết lớp học theo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<LopHoc>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LayTheoId(int id)
    {
        var lop = await _lopHocDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (lop == null)
        {
            return NotFound(ApiResponse<object>.ThatBaiResponse($"Không tìm thấy lớp học ID {id}.", 404));
        }

        return Ok(ApiResponse<LopHoc>.ThanhCongResponse(lop));
    }

    /// <summary>
    /// Tạo mới một lớp học.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    [ProducesResponseType(typeof(ApiResponse<LopHoc>), StatusCodes.Status201Created)]
    public async Task<IActionResult> ThemLop([FromBody] LopHoc lopHoc)
    {
        if (lopHoc == null || !ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Dữ liệu lớp học không hợp lệ.", 400));
        }

        try
        {
            var ketQua = await _lopHocDichVu.ThemLopHocAsync(lopHoc, HttpContext.RequestAborted);
            return CreatedAtAction(nameof(LayTheoId), new { id = ketQua.LopHocId }, ApiResponse<LopHoc>.ThanhCongResponse(ketQua, "Thêm lớp học thành công.", 201));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse(ex.Message, 400));
        }
    }

    /// <summary>
    /// Cập nhật thông tin lớp học.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> CapNhatLop(int id, [FromBody] LopHoc lopHoc)
    {
        if (lopHoc == null || id != lopHoc.LopHocId || !ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Dữ liệu cập nhật không khớp hoặc không hợp lệ.", 400));
        }

        try
        {
            var ketQua = await _lopHocDichVu.CapNhatLopHocAsync(lopHoc, HttpContext.RequestAborted);
            return Ok(ApiResponse<LopHoc>.ThanhCongResponse(ketQua, "Cập nhật lớp học thành công."));
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
    /// Xóa một lớp học nếu không còn học viên hoặc phân công.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> XoaLop(int id)
    {
        try
        {
            await _lopHocDichVu.XoaLopHocAsync(id, HttpContext.RequestAborted);
            return Ok(ApiResponse<object?>.ThanhCongResponse(null, "Xóa lớp học thành công."));
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
