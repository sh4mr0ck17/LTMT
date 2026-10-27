using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models.Api;
using QuanLyDaoTao.Models.Excel;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers.Api;

/// <summary>
/// Bộ điều khiển RESTful API cho các tác vụ Tải mẫu, Nhập (Import) và Xuất (Export) tệp Excel.
/// </summary>
[ApiController]
[Route("api/v1/excel")]
[Produces("application/json")]
public class ExcelApiController : ControllerBase
{
    private readonly IExcelDichVu _excelDichVu;

    /// <summary>
    /// Khởi tạo ExcelApiController.
    /// </summary>
    public ExcelApiController(IExcelDichVu excelDichVu)
    {
        ArgumentNullException.ThrowIfNull(excelDichVu);
        _excelDichVu = excelDichVu;
    }

    /// <summary>
    /// Tải tệp Excel mẫu chuẩn (.xlsx) của một module (Khoa, BoMon, LopHoc, HocVien, NhanSu, MonHoc, PhanCongGiangDay).
    /// </summary>
    [HttpGet("template/{module}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public IActionResult TaiTemplate(string module)
    {
        try
        {
            var bytes = _excelDichVu.TaoTepMauExcel(module);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Mau_{module}.xlsx");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse(ex.Message, 400));
        }
    }

    /// <summary>
    /// Xuất danh sách toàn bộ dữ liệu của module ra tệp Excel chuẩn (.xlsx) có bảo vệ chống Formula Injection.
    /// </summary>
    [HttpGet("export/{module}")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> XuatDuLieu(string module)
    {
        try
        {
            var bytes = await _excelDichVu.XuatDanhSachDuLieuExcelAsync(module, HttpContext.RequestAborted);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DanhSach_{module}_{DateTime.UtcNow:yyyyMMdd}.xlsx");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse(ex.Message, 400));
        }
    }

    /// <summary>
    /// Tải tệp Excel lên thẩm định nghiêm ngặt (Header, Format, Duplicate, FK) và trả về báo cáo xem trước JSON.
    /// </summary>
    [HttpPost("import-preview/{module}")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<KetQuaKiemTraExcel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> KiemTraVaXemTruoc(string module, IFormFile? tepExcel)
    {
        if (tepExcel == null || tepExcel.Length == 0)
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Vui lòng đính kèm tệp Excel (.xlsx).", 400));
        }

        if (!tepExcel.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Chỉ chấp nhận tệp định dạng .xlsx.", 400));
        }

        using var ms = new MemoryStream();
        await tepExcel.CopyToAsync(ms);
        ms.Position = 0;

        var ketQua = await _excelDichVu.KiemTraVaDocTepExcelAsync(module, ms, tepExcel.FileName, HttpContext.RequestAborted);
        return Ok(ApiResponse<KetQuaKiemTraExcel>.ThanhCongResponse(ketQua, $"Phân tích tệp xong: {ketQua.SoDongHopLe} dòng hợp lệ, {ketQua.SoDongLoi} dòng lỗi."));
    }
}
