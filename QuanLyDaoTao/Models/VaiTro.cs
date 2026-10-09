using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QuanLyDaoTao.Models;

/// <summary>
/// Thực thể đại diện cho Vai trò phân quyền trong hệ thống (kế thừa IdentityRole).
/// </summary>
public class VaiTro : IdentityRole
{
    /// <summary>
    /// Khởi tạo mặc định cho vai trò.
    /// </summary>
    public VaiTro() : base()
    {
    }

    /// <summary>
    /// Khởi tạo vai trò với tên cụ thể.
    /// </summary>
    /// <param name="tenVaiTro">Tên vai trò phân quyền.</param>
    public VaiTro(string tenVaiTro) : base(tenVaiTro)
    {
    }

    /// <summary>
    /// Mô tả chi tiết quyền hạn của vai trò.
    /// </summary>
    [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
    public string? MoTa { get; set; }

    /// <summary>
    /// Ngày khởi tạo vai trò.
    /// </summary>
    public DateTime NgayTao { get; set; } = DateTime.UtcNow;
}
