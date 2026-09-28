using DotNet.ServiceName.Api.Tests.Infrastructure;
using System.Net;
using Xunit;

namespace DotNet.ServiceName.Api.Tests.HealthChecks;

/// <summary>
/// Health check endpoints must stay reachable for probes without an API key.
/// </summary>
[Collection("Api")]
public sealed class HealthCheckTests
{
    private readonly ApiTestFactory _factory;

    public HealthCheckTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/healthcheck")]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    [InlineData("/health/live")]
    public async Task HealthEndpoint_IsAnonymous_Returns200WithoutApiKey(string path)
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthyStatus()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", body);
        Assert.DoesNotContain("Degraded", body);
        Assert.DoesNotContain("Unhealthy", body);
    }
}