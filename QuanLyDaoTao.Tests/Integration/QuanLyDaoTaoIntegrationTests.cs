using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace QuanLyDaoTao.Tests.Integration;

/// <summary>
/// Bộ kiểm thử tích hợp (Integration Tests) chạy thực tế trên đường ống HTTP request của ASP.NET Core.
/// </summary>
public class QuanLyDaoTaoIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Khởi tạo kiểm thử tích hợp với WebApplicationFactory.
    /// </summary>
    public QuanLyDaoTaoIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Home/Index")]
    [InlineData("/TaiKhoan/DangNhap")]
    [InlineData("/TaiKhoan/TuChoiTruyCap")]
    [InlineData("/Khoa")]
    [InlineData("/BoMon")]
    [InlineData("/LopHoc")]
    [InlineData("/HocVien")]
    [InlineData("/NhanSu")]
    [InlineData("/ChucVu")]
    [InlineData("/TrinhDoChuyenMon")]
    [InlineData("/MonHoc")]
    [InlineData("/PhanCongGiangDay")]
    [InlineData("/CayToChuc")]
    public async Task CacTrangChinh_TraVeHttp200Ok(string url)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(content));
    }

    [Fact]
    public async Task TrangDangNhap_CoChuaNutVaPhomDangNhap()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/TaiKhoan/DangNhap");

        // Assert
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("id=\"btnDangNhap\"", html);
        Assert.Contains("id=\"txtEmail\"", html);
        Assert.Contains("id=\"txtMatKhau\"", html);
    }

    [Theory]
    [InlineData("Khoa")]
    [InlineData("BoMon")]
    [InlineData("LopHoc")]
    [InlineData("HocVien")]
    [InlineData("NhanSu")]
    [InlineData("MonHoc")]
    [InlineData("PhanCongGiangDay")]
    public async Task TaiTepMauExcel_CacModule_TraVeFileXlsx(string module)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/Excel/TaiTepMau?module={module}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", response.Content.Headers.ContentType?.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public async Task TruyCapTrangQuanTriKhiChuaDangNhap_ChuyenHuongDangNhap()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act - Truy cập trang quản lý tài khoản của Admin
        var response = await client.GetAsync("/TaiKhoan/DanhSach");

        // Assert - Phải chuyển hướng về trang đăng nhập
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/TaiKhoan/DangNhap", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task TruyCapThemKhoaKhiChuaDangNhap_ChuyenHuongDangNhap()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/Khoa/ThemMoi");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/TaiKhoan/DangNhap", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData("/images/logo_badge.png", "image/png")]
    [InlineData("/images/logoT01.jpg", "image/jpeg")]
    [InlineData("/css/site.css", "text/css")]
    public async Task TaiNguyenLogoVaCssChuDeCAND_TraVeHttp200Ok(string url, string expectedMime)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedMime, response.Content.Headers.ContentType?.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public async Task GiaoDienTrangChu_HienThiLogoVaChuDeHocVienAnNinhNhanDan()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("HỌC VIỆN AN NINH NHÂN DÂN", html);
        Assert.Contains("logo_badge.png", html);
        Assert.Contains("navbar-cand", html);
    }
}
