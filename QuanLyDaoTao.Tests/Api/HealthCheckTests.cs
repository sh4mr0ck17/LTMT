using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace QuanLyDaoTao.Tests.Api;

/// <summary>
/// Bộ kiểm thử cho điểm cuối Sức khỏe hệ thống (/healthz) và OpenAPI Spec (/swagger).
/// </summary>
public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthCheck_TraVeHttp200OkHealthy()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/healthz");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task SwaggerJson_TraVeHttp200OkOpenApiDoc()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("openapi", content);
        Assert.Contains("Học Viện An Ninh Nhân Dân", content);
    }
}
