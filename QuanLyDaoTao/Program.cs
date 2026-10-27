using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuanLyDaoTao.Data;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Services;

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
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<QuanLyDaoTaoDbContext>()
.AddDefaultTokenProviders();

// Cấu hình Cookie xác thực cho giao diện Web/Blazor
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/TaiKhoan/DangNhap";
    options.AccessDeniedPath = "/TaiKhoan/TuChoiTruyCap";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Cấu hình JWT Bearer Authentication cho RESTful API
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"] ?? "HocVienAnNinhNhanDanC500_QuanLyDaoTao_SuperSecretKey_2026_SecureToken";
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

// Đăng ký Blazor Server & Razor Components .NET 8
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Endpoint kiểm tra sức khỏe hệ thống (Health Check) phục vụ CI/CD & Monitoring
app.MapHealthChecks("/healthz");

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
public partial class Program { }
