using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Controllers;

/// <summary>
/// Đường dẫn MVC cũ của hồ sơ cán bộ. Trang Blazor <c>/NhanSu</c> đã thay thế hoàn toàn (một biểu mẫu, một bộ trường,
/// một nguồn quy tắc), nên các địa chỉ cũ chỉ chuyển hướng để liên kết đã lưu vẫn dùng được.
/// Trước đây các view này cho nhập 19 trường không được lưu vào CSDL (mất dữ liệu âm thầm) nên đã gỡ bỏ.
/// </summary>
[Authorize]
public class NhanSuController : Controller
{
    private const string TrangNhanSu = "/NhanSu";

    /// <summary>Chi tiết cán bộ (đường dẫn cũ) → trang danh sách cán bộ.</summary>
    [HttpGet]
    public IActionResult ChiTiet(int id) => LocalRedirect(TrangNhanSu);

    /// <summary>Thêm cán bộ (đường dẫn cũ) → trang cán bộ, dùng nút “Thêm cán bộ”.</summary>
    [HttpGet]
    [Authorize(Roles = VaiTroMacDinh.NhomCoQuyenGhi)]
    public IActionResult ThemMoi() => LocalRedirect(TrangNhanSu);

    /// <summary>Sửa cán bộ (đường dẫn cũ) → trang cán bộ.</summary>
    [HttpGet]
    [Authorize(Roles = VaiTroMacDinh.NhomCoQuyenGhi)]
    public IActionResult ChinhSua(int id) => LocalRedirect(TrangNhanSu);
}
