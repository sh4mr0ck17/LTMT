using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.Api;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers.Api;

/// <summary>
/// Bộ điều khiển RESTful API Quản lý Học viên Đào tạo.
/// </summary>
[ApiController]
[Route("api/v1/hocvien")]
[Produces("application/json")]
public class HocVienApiController : ControllerBase
{
    private readonly IHocVienDichVu _hocVienDichVu;

    /// <summary>
    /// Khởi tạo HocVienApiController.
    /// </summary>
    public HocVienApiController(IHocVienDichVu hocVienDichVu)
    {
        ArgumentNullException.ThrowIfNull(hocVienDichVu);
        _hocVienDichVu = hocVienDichVu;
    }

    /// <summary>
    /// Lấy danh sách học viên theo lớp và tìm kiếm từ khóa.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<HocVien>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LayDanhSach(
        [FromQuery] int? lopHocId = null,
        [FromQuery] string? tuKhoa = null,
        [FromQuery] bool? coHoatDong = null)
    {
        var danhSach = await _hocVienDichVu.LayDanhSachAsync(lopHocId, tuKhoa, coHoatDong, HttpContext.RequestAborted);
        return Ok(ApiResponse<List<HocVien>>.ThanhCongResponse(danhSach, $"Tìm thấy {danhSach.Count} học viên."));
    }

    /// <summary>
    /// Lấy chi tiết học viên theo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<HocVien>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LayTheoId(int id)
    {
        var hv = await _hocVienDichVu.LayTheoIdAsync(id, HttpContext.RequestAborted);
        if (hv == null)
        {
            return NotFound(ApiResponse<object>.ThatBaiResponse($"Không tìm thấy học viên ID {id}.", 404));
        }

        return Ok(ApiResponse<HocVien>.ThanhCongResponse(hv));
    }

    /// <summary>
    /// Thêm mới một học viên vào lớp.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> ThemHocVien([FromBody] HocVien hocVien)
    {
        if (hocVien == null || !ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Dữ liệu học viên không hợp lệ.", 400));
        }

        try
        {
            var ketQua = await _hocVienDichVu.ThemHocVienAsync(hocVien, HttpContext.RequestAborted);
            return CreatedAtAction(nameof(LayTheoId), new { id = ketQua.HocVienId }, ApiResponse<HocVien>.ThanhCongResponse(ketQua, "Thêm học viên thành công.", 201));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse(ex.Message, 400));
        }
    }

    /// <summary>
    /// Cập nhật thông tin học viên.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> CapNhatHocVien(int id, [FromBody] HocVien hocVien)
    {
        if (hocVien == null || id != hocVien.HocVienId || !ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Thông tin cập nhật không hợp lệ.", 400));
        }

        try
        {
            var ketQua = await _hocVienDichVu.CapNhatHocVienAsync(hocVien, HttpContext.RequestAborted);
            return Ok(ApiResponse<HocVien>.ThanhCongResponse(ketQua, "Cập nhật học viên thành công."));
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
    /// Xóa một học viên khỏi hệ thống.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public async Task<IActionResult> XoaHocVien(int id)
    {
        try
        {
            await _hocVienDichVu.XoaHocVienAsync(id, HttpContext.RequestAborted);
            return Ok(ApiResponse<object?>.ThanhCongResponse(null, "Xóa học viên thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ThatBaiResponse(ex.Message, 404));
        }
    }
}
