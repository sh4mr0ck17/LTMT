using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using QuanLyDaoTao.Models;
using QuanLyDaoTao.Models.Api;
using Xunit;

namespace QuanLyDaoTao.Tests.Api;

/// <summary>
/// Bộ kiểm thử cho RESTful API Quản lý Khoa (/api/v1/khoa).
/// </summary>
public class KhoaApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public KhoaApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LayDanhSach_TraVeHttp200OkVaJsonDungChuan()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/khoa");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var apiResult = await response.Content.ReadFromJsonAsync<ApiResponse<List<Khoa>>>();
        Assert.NotNull(apiResult);
        Assert.True(apiResult.ThanhCong);
        Assert.Equal(200, apiResult.MaTrangThai);
        Assert.NotNull(apiResult.DuLieu);
    }

    [Fact]
    public async Task LayTheoId_KhongTonTai_TraVeHttp404NotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/khoa/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var apiResult = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(apiResult);
        Assert.False(apiResult.ThanhCong);
        Assert.Equal(404, apiResult.MaTrangThai);
    }
}
