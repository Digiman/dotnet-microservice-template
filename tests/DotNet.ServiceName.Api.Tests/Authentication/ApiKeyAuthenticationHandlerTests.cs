using DotNet.ServiceName.Api.Infrastructure.Auth;
using DotNet.ServiceName.Common.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xunit;

namespace DotNet.ServiceName.Api.Tests.Authentication;

/// <summary>
/// Unit tests for the <see cref="ApiKeyAuthenticationHandler"/> authentication logic.
/// </summary>
public sealed class ApiKeyAuthenticationHandlerTests
{
    private const string HeaderName = "X-API-Key";
    private const string ApiKey = "unit-test-api-key";

    private static ApiKeyOptions CreateOptions()
    {
        return new ApiKeyOptions { HeaderName = HeaderName, ApiKey = ApiKey };
    }

    private static ApiKeyAuthenticationHandler CreateHandler()
    {
        return new ApiKeyAuthenticationHandler(
            new StaticOptionsMonitor<AuthenticationSchemeOptions>(),
            Options.Create(CreateOptions()),
            NullLoggerFactory.Instance,
            UrlEncoder.Default);
    }

    private static async Task<HttpContext> CreateInitializedContextAsync(ApiKeyAuthenticationHandler handler, string? providedKey)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddOptions().AddProblemDetails().BuildServiceProvider()
        };
        context.Response.Body = new MemoryStream();

        if (providedKey is not null)
        {
            context.Request.Headers[HeaderName] = providedKey;
        }

        await ((IAuthenticationHandler)handler).InitializeAsync(
            new AuthenticationScheme(ApiKeyAuthenticationHandler.SchemeName, null, typeof(ApiKeyAuthenticationHandler)),
            context);

        return context;
    }

    private static string ReadResponseBody(HttpContext context)
    {
        var stream = (MemoryStream)context.Response.Body;
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [Fact]
    public async Task Authenticate_WithoutApiKeyHeader_ReturnsNoResult()
    {
        var handler = CreateHandler();
        await CreateInitializedContextAsync(handler, null);

        var result = await ((IAuthenticationHandler)handler).AuthenticateAsync();

        Assert.True(result.None);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Authenticate_WithInvalidApiKey_ReturnsFailure()
    {
        var handler = CreateHandler();
        await CreateInitializedContextAsync(handler, "wrong-key");

        var result = await ((IAuthenticationHandler)handler).AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid API Key.", result.Failure?.Message);
    }

    [Fact]
    public async Task Authenticate_WithApiKeyOfDifferentLength_ReturnsFailureWithoutThrowing()
    {
        var handler = CreateHandler();
        await CreateInitializedContextAsync(handler, ApiKey + "-extra-suffix-longer-than-expected");

        var result = await ((IAuthenticationHandler)handler).AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid API Key.", result.Failure?.Message);
    }

    [Fact]
    public async Task Authenticate_WithEmptyApiKey_ReturnsFailure()
    {
        var handler = CreateHandler();
        await CreateInitializedContextAsync(handler, string.Empty);

        var result = await ((IAuthenticationHandler)handler).AuthenticateAsync();

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Authenticate_WithValidApiKey_Succeeds_WithApiKeyIdentity()
    {
        var handler = CreateHandler();
        await CreateInitializedContextAsync(handler, ApiKey);

        var result = await ((IAuthenticationHandler)handler).AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Principal);
        Assert.NotNull(result.Principal.Identity);
        Assert.True(result.Principal.Identity.IsAuthenticated);
        Assert.Equal("ApiKey", result.Principal.Identity.Name);
        Assert.Equal(ApiKeyAuthenticationHandler.SchemeName, result.Principal.Identity.AuthenticationType);
    }

    [Fact]
    public async Task Challenge_WithoutApiKey_Writes401ProblemDetailsAndWwwAuthenticateHeader()
    {
        var handler = CreateHandler();
        var context = await CreateInitializedContextAsync(handler, null);

        await ((IAuthenticationHandler)handler).ChallengeAsync(null);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(ApiKeyAuthenticationHandler.SchemeName, context.Response.Headers.WWWAuthenticate.ToString());
        Assert.StartsWith("application/problem+json", context.Response.ContentType);

        using var body = JsonDocument.Parse(ReadResponseBody(context));
        Assert.Equal(StatusCodes.Status401Unauthorized, body.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Unauthorized", body.RootElement.GetProperty("title").GetString());
        Assert.Contains(HeaderName, body.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Forbid_Writes403ProblemDetails()
    {
        var handler = CreateHandler();
        var context = await CreateInitializedContextAsync(handler, ApiKey);

        await ((IAuthenticationHandler)handler).ForbidAsync(null);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);

        using var body = JsonDocument.Parse(ReadResponseBody(context));
        Assert.Equal(StatusCodes.Status403Forbidden, body.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Forbidden", body.RootElement.GetProperty("title").GetString());
    }

    /// <summary>
    /// Simple options monitor returning a fixed instance for the base handler options.
    /// </summary>
    private sealed class StaticOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
        where TOptions : class, new()
    {
        public TOptions CurrentValue { get; } = new();

        public TOptions Get(string? name) => CurrentValue;

        public IDisposable? OnChange(Action<TOptions, string?> listener) => null;
    }
}