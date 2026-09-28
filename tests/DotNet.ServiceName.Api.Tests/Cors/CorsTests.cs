using DotNet.ServiceName.Api.Tests.Infrastructure;
using System.Net;
using Xunit;

namespace DotNet.ServiceName.Api.Tests.Cors;

/// <summary>
/// Verifies the configured CORS policy is applied to cross-origin browser requests.
/// </summary>
[Collection("Api")]
public sealed class CorsTests
{
    private const string AllowedOrigin = "http://localhost:4200";
    private const string DisallowedOrigin = "https://evil.example";

    private readonly ApiTestFactory _factory;

    public CorsTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task Preflight_AllowedOrigin_ReturnsPolicyHeaders()
    {
        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/status");
        request.Headers.Add("Origin", AllowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        request.Headers.Add("Access-Control-Request-Headers", ApiTestFactory.ApiKeyHeader);

        using var response = await client.SendAsync(request);

        Assert.True(response.IsSuccessStatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
        Assert.Equal(AllowedOrigin, origins!.Single());
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Methods", out var methods));
        Assert.Contains("GET", methods!.Single());
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Headers", out var headers));
        Assert.Contains(ApiTestFactory.ApiKeyHeader, headers!.Single(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SimpleGet_AllowedOrigin_CarriesCorsHeaders()
    {
        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/status");
        request.Headers.Add("Origin", AllowedOrigin);
        request.Headers.Add(ApiTestFactory.ApiKeyHeader, ApiTestFactory.ApiKey);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var origins));
        Assert.Equal(AllowedOrigin, origins!.Single());
    }

    [Fact]
    public async Task SimpleGet_DisallowedOrigin_HasNoCorsHeaders()
    {
        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/status");
        request.Headers.Add("Origin", DisallowedOrigin);
        request.Headers.Add(ApiTestFactory.ApiKeyHeader, ApiTestFactory.ApiKey);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.TryGetValues("Access-Control-Allow-Origin", out _));
    }
}