using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QuanLyDaoTao.Controllers.Api;
using QuanLyDaoTao.Models.Api;
using Xunit;

namespace QuanLyDaoTao.Tests.Api;

/// <summary>
/// Bộ kiểm thử cho API Xác thực và cấp phát JWT Token (/api/v1/auth).
/// </summary>
public class AuthApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DangNhap_ThieuThongTin_TraVeBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new LoginApiRequest { Email = "", MatKhau = "" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DangNhap_TaiKhoanKhongTonTai_TraVeUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new LoginApiRequest { Email = "nguoidung_khongtontai@test.local", MatKhau = "MatKhau@123" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DangNhap_DungThongTinAdmin_TraVeJwtTokenHopLe()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new LoginApiRequest
        {
            Email = "admin@quanlydaotao.local",
            MatKhau = "Admin@123456"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResult = await response.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResult>>();
        Assert.NotNull(apiResult);
        Assert.True(apiResult.ThanhCong);
        Assert.NotNull(apiResult.DuLieu);
        Assert.NotEmpty(apiResult.DuLieu.AccessToken);
        Assert.Equal("Bearer", apiResult.DuLieu.TokenType);
        Assert.Equal("admin@quanlydaotao.local", apiResult.DuLieu.Email);
    }
}
