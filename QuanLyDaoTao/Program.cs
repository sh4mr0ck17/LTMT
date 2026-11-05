using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;
using Microsoft.AspNetCore.RateLimiting;
using Radzen;
using Microsoft.AspNetCore.HttpOverrides;

// Định dạng ngày, số và so sánh chuỗi tiếng Việt thống nhất toàn hệ thống.
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");

var builder = WebApplication.CreateBuilder(args);

// Cấu hình kết nối cơ sở dữ liệu SQLite
var chuoiKetNoi = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=QuanLyDaoTao.db";
builder.Services.AddDbContext<QuanLyDaoTaoDbContext>(options =>
    options.UseSqlite(chuoiKetNoi));

// Cấu hình ASP.NET Core Identity cho Tài khoản và Vai trò
builder.Services.AddIdentity<TaiKhoan, VaiTro>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.SignIn.RequireConfirmedAccount = false;
    // Khóa tài khoản 15 phút sau 5 lần đăng nhập sai liên tiếp.
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<QuanLyDaoTaoDbContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<NhaMayClaimTaiKhoan>();
builder.Services.AddCascadingAuthenticationState();

// Kiểm tra lại SecurityStamp mỗi phút: đổi quyền / khóa / đặt lại mật khẩu làm phiên cũ hết hiệu lực gần như ngay lập tức.
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromMinutes(1));

// Cấu hình Cookie xác thực cho giao diện Web/Blazor
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/TaiKhoan/DangNhap";
    options.AccessDeniedPath = "/TaiKhoan/TuChoiTruyCap";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Giới hạn tần suất các điểm đăng nhập/đăng ký để chống dò mật khẩu.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(TenChinhSachGioiHanDangNhap, cauHinh =>
    {
        cauHinh.PermitLimit = 20;
        cauHinh.Window = TimeSpan.FromMinutes(1);
        cauHinh.QueueLimit = 0;
    });
});

// Cấu hình JWT Bearer Authentication cho RESTful API. Khóa bí mật bắt buộc cấu hình ở môi trường thật.
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"];
if (string.IsNullOrWhiteSpace(secretKey))
{
    if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
    {
        throw new InvalidOperationException("Thiếu cấu hình Jwt:SecretKey. Đặt qua biến môi trường hoặc User Secrets.");
    }

    secretKey = "ChiDungChoMoiTruongPhatTrien_QuanLyDaoTao_KhongDungKhiTrienKhai_2026";
}

var keyBytes = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"] ?? "QuanLyDaoTao.Api",
            ValidAudience = jwtSection["Audience"] ?? "QuanLyDaoTao.Clients",
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero
        };
    });


// Đăng ký dịch vụ Kiểm tra sức khỏe hệ thống (Health Check)
builder.Services.AddHealthChecks();

// Đăng ký OpenAPI / Swagger Generator
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Học Viện An Ninh Nhân Dân (C500) - API Đào Tạo & Nhân Sự",
        Version = "v1",
        Description = "Cổng RESTful Web API cung cấp dịch vụ dữ liệu đào tạo, hồ sơ cán bộ, sinh viên và phân công giảng dạy",
        Contact = new OpenApiContact
        {
            Name = "Học viện An ninh Nhân dân - C500",
            Email = "cntt@c500.edu.vn"
        }
    });

    // Cấu hình chứng thực JWT Bearer trong Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập token JWT theo định dạng: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Phân quyền tại tầng dịch vụ và kênh đồng bộ thay đổi giữa các tab
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPhanQuyenDichVu, PhanQuyenDichVu>();
builder.Services.AddSingleton<IBusThayDoiDuLieu, BusThayDoiDuLieu>();
builder.Services.AddScoped<IThongBaoDichVu, ThongBaoDichVu>();
builder.Services.AddScoped<ITruongTuyChinhDichVu, TruongTuyChinhDichVu>();
builder.Services.AddScoped<ITaiKhoanDichVu, TaiKhoanDichVu>();

// Đăng ký các dịch vụ nghiệp vụ (3-Tier Services)
builder.Services.AddScoped<INhatKyDichVu, NhatKyDichVu>();
builder.Services.AddScoped<IKhoaDichVu, KhoaDichVu>();
builder.Services.AddScoped<IBoMonDichVu, BoMonDichVu>();
builder.Services.AddScoped<ILopHocDichVu, LopHocDichVu>();
builder.Services.AddScoped<IHocVienDichVu, HocVienDichVu>();
builder.Services.AddScoped<INhanSuDichVu, NhanSuDichVu>();
builder.Services.AddScoped<IChucVuDichVu, ChucVuDichVu>();
builder.Services.AddScoped<ITrinhDoChuyenMonDichVu, TrinhDoChuyenMonDichVu>();
builder.Services.AddScoped<IMonHocDichVu, MonHocDichVu>();
builder.Services.AddScoped<IPhanCongGiangDayDichVu, PhanCongGiangDayDichVu>();
builder.Services.AddScoped<IExcelDichVu, ExcelDichVu>();
builder.Services.AddScoped<IQuanTriDuLieuDichVu, QuanTriDuLieuDichVu>();
builder.Services.AddScoped<ICayToChucDichVu, CayToChucDichVu>();

// Đăng ký Blazor Server & Razor Components .NET 8
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRadzenComponents();

// Đăng ký MVC Controllers & API Controllers
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

// Khởi tạo dữ liệu mặc định ban đầu an toàn
await DuLieuKhoiTao.KhoiTaoDuLieuAsync(app.Services);

// Kích hoạt Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "C500 API v1");
    c.RoutePrefix = "swagger";
});

// Cấu hình đường ống HTTP request
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.UseAntiforgery();

// Endpoint kiểm tra sức khỏe hệ thống (Health Check) phục vụ CI/CD & Monitoring
app.MapHealthChecks("/healthz").AllowAnonymous();

// Tải tệp đính kèm thông báo: bắt buộc đăng nhập, luôn tải xuống (không hiển thị nội tuyến) để tránh thực thi nội dung.
app.MapGet("/ThongBao/tep/{id:int}", async (int id, IThongBaoDichVu dichVuThongBao, HttpContext http) =>
{
    var tep = await dichVuThongBao.LayTepAsync(id, http.RequestAborted);
    if (tep is null)
    {
        return Results.NotFound();
    }

    http.Response.Headers["X-Content-Type-Options"] = "nosniff";
    return Results.File(tep.DuLieu, tep.KieuNoiDung, tep.TenTep);
}).RequireAuthorization();

// Endpoint Blazor Razor Components
app.MapRazorComponents<QuanLyDaoTao.Components.App>()
    .AddInteractiveServerRenderMode();

// Endpoint MVC & API Controllers
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

/// <summary>
/// Partial class Program cho WebApplicationFactory trong Integration tests.
/// </summary>
/// <summary>
/// Điểm vào ứng dụng (khai báo partial để dự án kiểm thử dùng WebApplicationFactory).
/// </summary>
public partial class Program
{
    /// <summary>
    /// Tên chính sách giới hạn tần suất cho đăng nhập / đăng ký.
    /// </summary>
    public const string TenChinhSachGioiHanDangNhap = "dang-nhap";
}
