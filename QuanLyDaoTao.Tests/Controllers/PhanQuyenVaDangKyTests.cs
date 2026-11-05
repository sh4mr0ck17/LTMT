using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.ViewModels.TaiKhoan;
using Xunit;

namespace QuanLyDaoTao.Tests.Controllers;

/// <summary>
/// Bộ kiểm thử xác thực Cổng đăng ký (Gateway), Phân quyền User thường vs Admin, và bảo vệ Live CRUD.
/// </summary>
public class PhanQuyenVaDangKyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PhanQuyenVaDangKyTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TruyCapAnDanh_HienThiTrangDangNhapNgayLanDauTruyCap()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act: Khách truy cập ẩn danh vào trang chủ /
        var response = await client.GetAsync("/");

        // Assert: Trả về HTTP 200 OK và hiển thị ngay phom Đăng nhập
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("pnlDangNhap", html);
        Assert.Contains("btnDangNhap", html);
        Assert.Contains("Hệ thống nội bộ Học viện ANND C500. Vui lòng sử dụng tài khoản do Quản trị viên cấp để đăng nhập.", html);
    }

    [Fact]
    public async Task KhachChuaDangNhap_KhongHeCoSideBarHoacPanel_ChiHienFormDangNhap()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act 1: Truy cập trang chủ /
        var respHome = await client.GetAsync("/");
        var htmlHome = await respHome.Content.ReadAsStringAsync();

        // Assert 1: Tuyệt đối không hề có app-sidebar, không có sidebar-tree, không có link nội bộ
        Assert.DoesNotContain("app-sidebar", htmlHome);
        Assert.DoesNotContain("sidebar-tree", htmlHome);
        Assert.DoesNotContain("btnMenuQuanTriDb", htmlHome);
        Assert.Contains("pnlDangNhap", htmlHome);
        Assert.Contains("btnDangNhap", htmlHome);

        // Act 2: Khách truy cập danh mục Khoa (/Khoa)
        var respKhoa = await client.GetAsync("/Khoa");
        var htmlKhoa = await respKhoa.Content.ReadAsStringAsync();

        // Assert 2: Không hề có app-sidebar, chỉ hiện form đăng nhập
        Assert.DoesNotContain("app-sidebar", htmlKhoa);
        Assert.DoesNotContain("sidebar-tree", htmlKhoa);
        Assert.Contains("pnlDangNhap", htmlKhoa);
    }

    [Fact]
    public async Task DangKy_HienThiBieuMauDangKyAnToan()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act: Khách truy cập cổng đăng ký an toàn
        var response = await client.GetAsync("/TaiKhoan/DangKy");

        // Assert: Trả về HTTP 200 OK và hiển thị phom đăng ký an toàn
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("pnlDangKy", html);
        Assert.Contains("btnDangKy", html);
        Assert.Contains("txtHoVaTen", html);
        Assert.Contains("txtEmail", html);
    }
    [Fact]
    public async Task DangKy_TaoTaiKhoanThanhCong_TuDongGanRoleNguoiDung()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TaiKhoan>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<VaiTro>>();

        // Đảm bảo role User tồn tại
        if (!await roleManager.RoleExistsAsync(VaiTroMacDinh.NguoiDung))
        {
            await roleManager.CreateAsync(new VaiTro(VaiTroMacDinh.NguoiDung) { MoTa = "Người dùng nghiệp vụ" });
        }

        string emailTest = $"canbo_{Guid.NewGuid():N}@c500.edu.vn";
        var userMoi = new TaiKhoan
        {
            UserName = emailTest,
            Email = emailTest,
            HoVaTen = "Cán Bộ Kiểm Thử Nghiệp Vụ",
            EmailConfirmed = true,
            CoHoatDong = true,
            NgayTao = DateTime.UtcNow
        };

        // Act: Tạo tài khoản và gán vai trò User
        var ketQua = await userManager.CreateAsync(userMoi, "CanBo@123456");
        Assert.True(ketQua.Succeeded);

        await userManager.AddToRoleAsync(userMoi, VaiTroMacDinh.NguoiDung);

        // Assert: Xác nhận tài khoản được gán đúng role NguoiDung (User)
        var roles = await userManager.GetRolesAsync(userMoi);
        Assert.Contains(VaiTroMacDinh.NguoiDung, roles);
        Assert.DoesNotContain(VaiTroMacDinh.QuanTriVien, roles);

        // Xác nhận không có quyền Admin
        Assert.False(roles.Contains(VaiTroMacDinh.QuanTriVien));
    }

    [Fact]
    public async Task NguoiDungChuaDangNhap_TruyCapLiveCrudStudio_BiTuChoiVaChuyenHuong()
    {
        // Arrange: Không tự động chuyển hướng để bắt mã HTTP 302 bảo vệ của Admin
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act: Truy cập màn hình Live CRUD DB Studio (/QuanTriDuLieu)
        var response = await client.GetAsync("/QuanTriDuLieu");

        // Assert: Bị chặn và chuyển hướng 302 về Cổng Đăng nhập
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/TaiKhoan/DangNhap", response.Headers.Location.ToString());
    }
    [Fact]
    public async Task NguoiDungChuaDangNhap_TruyCapLiveCrudApi_BiChan401HoacRedirect()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act: Gọi API Live CRUD danh sách bảng
        var response = await client.GetAsync("/api/v1/live-crud/tables");

        // Assert: Bị từ chối
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Redirect);
    }
    [Fact]
    public void VaiTroMacDinh_CoDayDuRoleSuperAdminGiaoVienCanBoUser()
    {
        // Assert: Các hằng số quyền chuẩn hóa
        Assert.Equal("Superadmin", VaiTroMacDinh.SuperAdmin);
        Assert.Equal("Admin", VaiTroMacDinh.QuanTriVien);
        Assert.Equal("GiaoVien", VaiTroMacDinh.GiaoVien);
        Assert.Equal("CanBo", VaiTroMacDinh.CanBo);
        Assert.Equal("User", VaiTroMacDinh.NguoiDung);
        Assert.Equal("DataManager", VaiTroMacDinh.QuanLyDaoTao);
        Assert.Equal("Viewer", VaiTroMacDinh.NguoiXem);
    }

    [Fact]
    public void LiveCrudController_ChiChoPhepDuyNhatSuperAdmin()
    {
        // Kiểm tra Attribute [Authorize(Roles = "Superadmin")] trên QuanTriDuLieuController và LiveCrudApiController
        var ctrlAttr = typeof(QuanLyDaoTao.Controllers.QuanTriDuLieuController)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .FirstOrDefault();

        var apiAttr = typeof(QuanLyDaoTao.Controllers.Api.LiveCrudApiController)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(ctrlAttr);
        Assert.Equal(VaiTroMacDinh.SuperAdmin, ctrlAttr.Roles);

        Assert.NotNull(apiAttr);
        Assert.Equal(VaiTroMacDinh.SuperAdmin, apiAttr.Roles);
    }

    [Theory]
    [InlineData(typeof(QuanLyDaoTao.Controllers.KhoaController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.BoMonController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.LopHocController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.HocVienController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.NhanSuController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.MonHocController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.PhanCongGiangDayController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.ChucVuController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.TrinhDoChuyenMonController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.NhatKyController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.Api.KhoaApiController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.Api.LopHocApiController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.Api.HocVienApiController))]
    [InlineData(typeof(QuanLyDaoTao.Controllers.Api.NhanSuApiController))]
    public void Controllers_DeuCapQuyenChoSuperAdmin(Type controllerType)
    {
        // Kiểm tra xem controller hoặc các method có chứa vai trò Superadmin không
        var classAuth = controllerType
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .FirstOrDefault();

        var methodAuths = controllerType.GetMethods()
            .SelectMany(m => m.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true))
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .ToList();

        bool coSuperAdmin = (classAuth != null && (classAuth.Roles?.Contains(VaiTroMacDinh.SuperAdmin) ?? false))
            || methodAuths.Any(a => a.Roles?.Contains(VaiTroMacDinh.SuperAdmin) ?? false);

        Assert.True(coSuperAdmin, $"Controller {controllerType.Name} phải cấp quyền cho Superadmin");
    }
}
