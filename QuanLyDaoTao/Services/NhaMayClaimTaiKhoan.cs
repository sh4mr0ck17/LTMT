using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Bổ sung claim họ tên, vùng quản lý và khoa phụ trách vào cookie đăng nhập.
/// Khi admin đổi quyền, SecurityStamp thay đổi nên phiên cũ bị từ chối ở lần kiểm tra kế tiếp.
/// </summary>
public class NhaMayClaimTaiKhoan : UserClaimsPrincipalFactory<TaiKhoan, VaiTro>
{
    /// <summary>
    /// Khởi tạo nhà máy claim.
    /// </summary>
    public NhaMayClaimTaiKhoan(
        UserManager<TaiKhoan> quanLyNguoiDung,
        RoleManager<VaiTro> quanLyVaiTro,
        IOptions<IdentityOptions> tuyChon)
        : base(quanLyNguoiDung, quanLyVaiTro, tuyChon)
    {
    }

    /// <inheritdoc />
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TaiKhoan user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var danhTinh = await base.GenerateClaimsAsync(user);

        danhTinh.AddClaim(new Claim(LoaiClaimTuyChinh.HoVaTen, user.HoVaTen));
        if (VungQuanLy.LaHopLe(user.VungQuanLy))
        {
            danhTinh.AddClaim(new Claim(LoaiClaimTuyChinh.VungQuanLy, user.VungQuanLy!));
        }

        if (user.KhoaPhuTrachId.HasValue)
        {
            danhTinh.AddClaim(new Claim(LoaiClaimTuyChinh.KhoaPhuTrach,
                user.KhoaPhuTrachId.Value.ToString(CultureInfo.InvariantCulture)));
        }

        return danhTinh;
    }
}
