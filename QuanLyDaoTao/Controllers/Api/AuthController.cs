using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.Api;

namespace QuanLyDaoTao.Controllers.Api;

/// <summary>
/// Gói tin yêu cầu đăng nhập lấy JWT Token.
/// </summary>
public class LoginApiRequest
{
    /// <summary>
    /// Địa chỉ email đăng nhập.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu xác thực.
    /// </summary>
    public string MatKhau { get; set; } = string.Empty;
}

/// <summary>
/// Dữ liệu trả về khi đăng nhập thành công.
/// </summary>
public class AuthTokenResult
{
    /// <summary>
    /// Chuỗi JWT Access Token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Loại token (Bearer).
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Thời gian hết hạn tính bằng giây.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Email của người dùng.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Họ và tên hiển thị.
    /// </summary>
    public string HoVaTen { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách các vai trò được phân quyền.
    /// </summary>
    public IList<string> VaiTro { get; set; } = new List<string>();
}

/// <summary>
/// Bộ điều khiển API Xác thực và Cấp phát JWT Access Token.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<TaiKhoan> _userManager;
    private readonly SignInManager<TaiKhoan> _signInManager;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Khởi tạo AuthController.
    /// </summary>
    public AuthController(
        UserManager<TaiKhoan> userManager,
        SignInManager<TaiKhoan> signInManager,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(signInManager);
        ArgumentNullException.ThrowIfNull(configuration);

        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    /// <summary>
    /// Xác thực thông tin đăng nhập và cấp JWT Token cho ứng dụng di động hoặc bên thứ ba.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DangNhap([FromBody] LoginApiRequest yeuCau)
    {
        if (yeuCau == null || string.IsNullOrWhiteSpace(yeuCau.Email) || string.IsNullOrWhiteSpace(yeuCau.MatKhau))
        {
            return BadRequest(ApiResponse<object>.ThatBaiResponse("Email và mật khẩu không được để trống.", 400));
        }

        var user = await _userManager.FindByEmailAsync(yeuCau.Email);
        if (user == null || !user.CoHoatDong)
        {
            return Unauthorized(ApiResponse<object>.ThatBaiResponse("Tài khoản không tồn tại hoặc đã bị khóa.", 401));
        }

        var checkPassword = await _signInManager.CheckPasswordSignInAsync(user, yeuCau.MatKhau, false);
        if (!checkPassword.Succeeded)
        {
            return Unauthorized(ApiResponse<object>.ThatBaiResponse("Mật khẩu không chính xác.", 401));
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Sinh JWT Token
        var jwtSection = _configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"] ?? "HocVienAnNinhNhanDanC500_QuanLyDaoTao_SuperSecretKey_2026_SecureToken";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        int expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out int exp) ? exp : 480;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new("HoVaTen", user.HoVaTen)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = jwtSection["Issuer"] ?? "QuanLyDaoTao.Api",
            Audience = jwtSection["Audience"] ?? "QuanLyDaoTao.Clients",
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        var ketQua = new AuthTokenResult
        {
            AccessToken = tokenString,
            TokenType = "Bearer",
            ExpiresIn = expiryMinutes * 60,
            Email = user.Email ?? "",
            HoVaTen = user.HoVaTen,
            VaiTro = roles
        };

        return Ok(ApiResponse<AuthTokenResult>.ThanhCongResponse(ketQua, "Đăng nhập thành công, token hợp lệ."));
    }

    /// <summary>
    /// Lấy thông tin hồ sơ của tài khoản đang đăng nhập qua Bearer Token.
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LayThongTinCaNhan()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound(ApiResponse<object>.ThatBaiResponse("Không tìm thấy thông tin tài khoản.", 404));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var thongTin = new
        {
            user.Id,
            user.Email,
            user.HoVaTen,
            user.PhoneNumber,
            user.MaNhanSuLienKet,
            user.CoHoatDong,
            user.NgayTao,
            VaiTro = roles
        };

        return Ok(ApiResponse<object>.ThanhCongResponse(thongTin, "Lấy thông tin tài khoản thành công."));
    }
}
