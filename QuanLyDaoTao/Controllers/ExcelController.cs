using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.Excel;
using QuanLyDaoTao.Services;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Bộ điều khiển phụ trách các chức năng Tải mẫu, Nhập (Import), Xem trước (Preview) và Xuất (Export) Excel.
/// </summary>
public class ExcelController : Controller
{
    private readonly IExcelDichVu _excelDichVu;
    private readonly INhatKyDichVu _nhatKyDichVu;

    /// <summary>
    /// Khởi tạo bộ điều khiển với dịch vụ Excel và Nhật ký.
    /// </summary>
    /// <param name="excelDichVu">Dịch vụ xử lý Excel.</param>
    /// <param name="nhatKyDichVu">Dịch vụ nhật ký kiểm toán.</param>
    public ExcelController(IExcelDichVu excelDichVu, INhatKyDichVu nhatKyDichVu)
    {
        ArgumentNullException.ThrowIfNull(excelDichVu);
        ArgumentNullException.ThrowIfNull(nhatKyDichVu);

        _excelDichVu = excelDichVu;
        _nhatKyDichVu = nhatKyDichVu;
    }

    /// <summary>
    /// Tải tệp Excel mẫu chuẩn (.xlsx) của một module cụ thể.
    /// </summary>
    /// <param name="module">Tên module (Khoa, BoMon, LopHoc, HocVien, NhanSu, MonHoc, PhanCongGiangDay).</param>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult TaiTepMau(string module)
    {
        if (string.IsNullOrWhiteSpace(module))
        {
            return BadRequest("Tên module không được để trống.");
        }

        try
        {
            var duLieuTep = _excelDichVu.TaoTepMauExcel(module);
            var tenTep = $"MauNhap_{module}.xlsx";
            return File(duLieuTep, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", tenTep);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Hiển thị giao diện chọn tệp Excel để nhập dữ liệu cho module.
    /// </summary>
    /// <param name="module">Tên module nghiệp vụ.</param>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao},{VaiTroMacDinh.NguoiDung}")]
    public IActionResult NhapExcel(string module)
    {
        if (string.IsNullOrWhiteSpace(module))
        {
            return BadRequest("Tên module không hợp lệ.");
        }

        ViewBag.Module = module;
        ViewBag.TieuDeModule = LayTieuDeModule(module);
        return View();
    }

    /// <summary>
    /// Tiếp nhận tệp Excel tải lên, kiểm tra nghiêm ngặt cấu trúc header và dữ liệu, sau đó hiển thị màn hình xem trước (Preview).
    /// </summary>
    /// <param name="module">Tên module nghiệp vụ.</param>
    /// <param name="tepExcel">Tệp Excel được tải lên từ máy khách.</param>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao},{VaiTroMacDinh.NguoiDung}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KiemTraTep(string module, IFormFile? tepExcel)
    {
        if (string.IsNullOrWhiteSpace(module))
        {
            return BadRequest("Tên module không hợp lệ.");
        }

        ViewBag.Module = module;
        ViewBag.TieuDeModule = LayTieuDeModule(module);

        if (tepExcel == null || tepExcel.Length == 0)
        {
            ModelState.AddModelError("", "Vui lòng chọn một tệp Excel (.xlsx) để tải lên.");
            return View("NhapExcel");
        }

        if (!tepExcel.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("", "Chỉ chấp nhận tệp định dạng Microsoft Excel OpenXML (.xlsx).");
            return View("NhapExcel");
        }

        using var memoryStream = new MemoryStream();
        await tepExcel.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var ketQuaKiemTra = await _excelDichVu.KiemTraVaDocTepExcelAsync(
            module,
            memoryStream,
            tepExcel.FileName,
            HttpContext.RequestAborted);

        // Lưu thông tin phiên xem trước
        var phienPreview = _excelDichVu.LuuPhienPreview(
            module,
            ketQuaKiemTra,
            User.Identity?.Name ?? "Quản lý dữ liệu");

        ViewBag.MaPhien = phienPreview.MaPhien;
        return View("XemTruocPreview", ketQuaKiemTra);
    }

    /// <summary>
    /// Xác nhận ghi dữ liệu từ phiên xem trước vào cơ sở dữ liệu hệ thống.
    /// </summary>
    /// <param name="maPhien">Mã phiên xem trước (GUID).</param>
    [HttpPost]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao},{VaiTroMacDinh.NguoiDung}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhanCommit(string maPhien)
    {
        var phien = _excelDichVu.LayPhienPreview(maPhien);
        if (phien == null)
        {
            TempData["ThongBaoLoi"] = "Phiên xem trước đã hết hạn hoặc không tồn tại. Vui lòng tải lại tệp Excel.";
            return RedirectToAction("Index", "Home");
        }

        var danhSachHopLe = phien.KetQuaKiemTra.DanhSachDongDuLieu.Where(d => d.LaHopLe).ToList();
        var ketQuaCommit = await _excelDichVu.CommitDuLieuExcelAsync(
            phien.TenModule,
            danhSachHopLe,
            User.Identity?.Name ?? "Quản lý dữ liệu",
            HttpContext.RequestAborted);

        _excelDichVu.XoaPhienPreview(maPhien);

        if (ketQuaCommit.ThanhCong)
        {
            TempData["ThongBaoThanhCong"] = ketQuaCommit.ThongDiep;
            return RedirectToAction("Index", phien.TenModule);
        }

        TempData["ThongBaoLoi"] = ketQuaCommit.ThongDiep;
        return RedirectToAction("NhapExcel", new { module = phien.TenModule });
    }

    /// <summary>
    /// Xuất tệp báo cáo lỗi chi tiết dạng Excel cho phiên xem trước để người dùng tải về sửa chữa.
    /// </summary>
    /// <param name="maPhien">Mã phiên xem trước.</param>
    [HttpGet]
    [Authorize(Roles = $"{VaiTroMacDinh.QuanTriVien},{VaiTroMacDinh.QuanLyDaoTao}")]
    public IActionResult XuatBaoCaoLoi(string maPhien)
    {
        var phien = _excelDichVu.LayPhienPreview(maPhien);
        if (phien == null)
        {
            return NotFound("Không tìm thấy thông tin phiên xem trước.");
        }

        var fileBytes = _excelDichVu.XuatBaoCaoLoiExcel(phien.KetQuaKiemTra);
        var tenTep = $"BaoCaoLoi_{phien.TenModule}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", tenTep);
    }

    /// <summary>
    /// Xuất danh sách toàn bộ dữ liệu hiện có của module ra tệp Excel chuẩn.
    /// </summary>
    /// <param name="module">Tên module nghiệp vụ.</param>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> XuatDuLieu(string module)
    {
        if (string.IsNullOrWhiteSpace(module))
        {
            return BadRequest("Tên module không hợp lệ.");
        }

        var fileBytes = await _excelDichVu.XuatDanhSachDuLieuExcelAsync(module, HttpContext.RequestAborted);
        var tenTep = $"DanhSach_{module}_{DateTime.UtcNow:yyyyMMdd}.xlsx";

        await _nhatKyDichVu.GhiNhatKyAsync(
            User.Identity?.Name ?? "Người dùng",
            "Xuất Excel",
            module,
            null,
            null,
            $"Tên tệp: {tenTep}",
            "Thành công",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", tenTep);
    }

    private static string LayTieuDeModule(string module)
    {
        return module.ToLowerInvariant() switch
        {
            "khoa" => "Khoa Đào Tạo",
            "bomon" => "Bộ Môn",
            "lophoc" => "Lớp Học",
            "hocvien" => "Học Viên",
            "nhansu" => "Hồ Sơ Nhân Sự",
            "monhoc" => "Môn Học",
            "phanconggiangday" or "phancong" => "Phân Công Giảng Dạy",
            _ => module
        };
    }
}
