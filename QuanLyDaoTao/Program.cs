using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

// Cấu hình Cookie xác thực
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/TaiKhoan/DangNhap";
    options.AccessDeniedPath = "/TaiKhoan/TuChoiTruyCap";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
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

// Đăng ký MVC Controllers và Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Khởi tạo dữ liệu mặc định ban đầu an toàn
await DuLieuKhoiTao.KhoiTaoDuLieuAsync(app.Services);

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

/// <summary>
/// Partial class Program cho WebApplicationFactory trong Integration tests.
/// </summary>
public partial class Program { }
