using DotNet.ServiceName.Api.Tests.Infrastructure;
using System.Net;
using System.Text.Json;
using Xunit;

namespace DotNet.ServiceName.Api.Tests.Swagger;

/// <summary>
/// Verifies the OpenAPI document advertises the API Key security scheme and stays public.
/// </summary>
[Collection("Api")]
public sealed class SwaggerTests
{
    private readonly ApiTestFactory _factory;

    public SwaggerTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerJson_IsPublicWithoutApiKey()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerJson_DefinesApiKeySecurityScheme()
    {
        using var client = _factory.CreateClient();

        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        using var document = JsonDocument.Parse(json);

        var securitySchemes = document.RootElement.GetProperty("components").GetProperty("securitySchemes");
        Assert.True(securitySchemes.TryGetProperty("ApiKey", out var scheme));

        Assert.Equal("apiKey", scheme.GetProperty("type").GetString());
        Assert.Equal("header", scheme.GetProperty("in").GetString());
        Assert.Equal(ApiTestFactory.ApiKeyHeader, scheme.GetProperty("name").GetString());
    }

    [Fact]
    public async Task SwaggerJson_AppliesSecurityRequirementGlobally()
    {
        using var client = _factory.CreateClient();

        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        using var document = JsonDocument.Parse(json);

        var security = document.RootElement.GetProperty("security");
        var hasApiKeyRequirement = security.EnumerateArray()
            .Any(entry => entry.TryGetProperty("ApiKey", out _));

        Assert.True(hasApiKeyRequirement, "Expected a global security requirement for the ApiKey scheme.");
    }

    [Fact]
    public async Task SecuredOperation_DocumentsUnauthorizedResponse()
    {
        using var client = _factory.CreateClient();

        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        using var document = JsonDocument.Parse(json);

        var valuesPath = document.RootElement.GetProperty("paths").GetProperty("/api/v1/values");
        var okResponse = valuesPath.GetProperty("get").GetProperty("responses");
        Assert.True(okResponse.TryGetProperty("401", out _), "Expected the 401 response to be documented.");
    }
}