using DotNet.ServiceName.Api.Infrastructure.Auth;
using DotNet.ServiceName.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace DotNet.ServiceName.Api.Tests.Authentication;

/// <summary>
/// End-to-end tests for the API Key authentication on secured endpoints.
/// </summary>
[Collection("Api")]
public sealed class AuthenticationTests
{
    private readonly ApiTestFactory _factory;

    public AuthenticationTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SecuredEndpoint_WithoutApiKey_Returns401ProblemDetails()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/values");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(ApiKeyAuthenticationHandler.SchemeName, Assert.Single(GetWwwAuthenticate(response)));
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(StatusCodes.Status401Unauthorized, body.RootElement.GetProperty("status").GetInt32());
        Assert.Contains(ApiTestFactory.ApiKeyHeader, body.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task SecuredEndpoint_WithInvalidApiKey_Returns401()
    {
        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/values");
        request.Headers.Add(ApiTestFactory.ApiKeyHeader, "definitely-wrong-key");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(ApiKeyAuthenticationHandler.SchemeName, Assert.Single(GetWwwAuthenticate(response)));
    }

    private static IEnumerable<string> GetWwwAuthenticate(HttpResponseMessage response)
    {
        return response.Headers.TryGetValues("WWW-Authenticate", out var values)
            ? values
            : Enumerable.Empty<string>();
    }

    [Fact]
    public async Task SecuredEndpoint_WithValidApiKey_Returns200WithValues()
    {
        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/values");
        request.Headers.Add(ApiTestFactory.ApiKeyHeader, ApiTestFactory.ApiKey);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var values = await response.Content.ReadFromJsonAsync<string[]>();
        Assert.NotNull(values);
        Assert.Equal(new[] { "value1", "value2" }, values);
    }

    [Fact]
    public async Task StatusEndpoint_WithoutApiKey_Returns401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/status");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StatusEndpoint_WithValidApiKey_Returns200()
    {
        using var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/status");
        request.Headers.Add(ApiTestFactory.ApiKeyHeader, ApiTestFactory.ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.TryGetProperty("appInfo", out _));
    }
}