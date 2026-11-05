using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using QuanLyDaoTao.Models;

namespace QuanLyDaoTao.Services;

/// <summary>
/// Đọc quyền từ claim của phiên hiện tại. Hoạt động được cả trong request MVC/API (HttpContext)
/// và trong mạch Blazor Interactive Server (AuthenticationStateProvider).
/// </summary>
public class PhanQuyenDichVu : IPhanQuyenDichVu
{
    private readonly IHttpContextAccessor _truyCapHttp;
    private readonly AuthenticationStateProvider? _nhaCungCapXacThuc;

    /// <summary>
    /// Khởi tạo dịch vụ phân quyền.
    /// </summary>
    public PhanQuyenDichVu(IHttpContextAccessor truyCapHttp, AuthenticationStateProvider? nhaCungCapXacThuc = null)
    {
        ArgumentNullException.ThrowIfNull(truyCapHttp);
        _truyCapHttp = truyCapHttp;
        _nhaCungCapXacThuc = nhaCungCapXacThuc;
    }

    /// <inheritdoc />
    public async Task<NguoiDungHienTai> LayNguoiDungAsync()
    {
        var nguoiDung = await LayPrincipalAsync();
        return TaoTuPrincipal(nguoiDung);
    }

    /// <inheritdoc />
    public async Task<NguoiDungHienTai> DamBaoQuyenGhiAsync(string vung, int? khoaId)
    {
        var nguoiDung = await LayNguoiDungAsync();
        if (!nguoiDung.CoQuyenGhi(vung, khoaId))
        {
            throw new UnauthorizedAccessException(nguoiDung.VaiTro == VaiTroMacDinh.QuanLy
                ? $"Tài khoản chỉ được thay đổi dữ liệu thuộc vùng \"{VungQuanLy.LayNhan(nguoiDung.VungQuanLy)}\"" +
                  (nguoiDung.KhoaPhuTrachId.HasValue ? " của khoa được giao." : ".")
                : "Tài khoản hiện tại không có quyền thay đổi dữ liệu.");
        }

        return nguoiDung;
    }

    /// <inheritdoc />
    public async Task<NguoiDungHienTai> DamBaoQuanTriAsync()
    {
        var nguoiDung = await LayNguoiDungAsync();
        if (!nguoiDung.LaQuanTri)
        {
            throw new UnauthorizedAccessException("Chỉ quản trị viên được thực hiện thao tác này.");
        }

        return nguoiDung;
    }

    /// <summary>
    /// Chuyển ClaimsPrincipal thành thông tin quyền.
    /// </summary>
    public static NguoiDungHienTai TaoTuPrincipal(ClaimsPrincipal? nguoiDung)
    {
        if (nguoiDung?.Identity?.IsAuthenticated != true)
        {
            return NguoiDungHienTai.AnDanh;
        }

        var vaiTro = VaiTroMacDinh.TatCa.FirstOrDefault(nguoiDung.IsInRole) ?? VaiTroMacDinh.NguoiXem;
        var vung = nguoiDung.FindFirstValue(LoaiClaimTuyChinh.VungQuanLy);
        int? khoaId = int.TryParse(nguoiDung.FindFirstValue(LoaiClaimTuyChinh.KhoaPhuTrach), NumberStyles.Integer,
            CultureInfo.InvariantCulture, out var giaTri) ? giaTri : null;

        return new NguoiDungHienTai(true, nguoiDung.Identity.Name ?? "Không rõ", vaiTro,
            VungQuanLy.LaHopLe(vung) ? vung : null, khoaId);
    }

    private async Task<ClaimsPrincipal?> LayPrincipalAsync()
    {
        if (_nhaCungCapXacThuc is not null)
        {
            try
            {
                return (await _nhaCungCapXacThuc.GetAuthenticationStateAsync()).User;
            }
            catch (InvalidOperationException)
            {
                // Ngoài phạm vi component Blazor (request MVC/API): dùng HttpContext bên dưới.
            }
        }

        return _truyCapHttp.HttpContext?.User;
    }
}
