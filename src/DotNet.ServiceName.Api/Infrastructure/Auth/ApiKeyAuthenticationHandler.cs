using DotNet.ServiceName.Common.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace DotNet.ServiceName.Api.Infrastructure.Auth;

/// <summary>
/// Authentication handler for the API Key schema - reads the key from the configured HTTP header and validates it.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    /// Name of the authentication scheme. Also used as the security scheme name in OpenAPI documents.
    /// </summary>
    public const string SchemeName = "ApiKey";

    private readonly ApiKeyOptions _apiKeyConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="options">The authentication scheme options.</param>
    /// <param name="apiKeyConfig">The API Key configuration from file.</param>
    /// <param name="logger">The logger factory.</param>
    /// <param name="encoder">The URL encoder.</param>
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        IOptions<ApiKeyOptions> apiKeyConfig,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _apiKeyConfig = apiKeyConfig.Value;
    }

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(_apiKeyConfig.HeaderName, out var providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!IsApiKeyValid(providedApiKey.ToString()))
        {
            Logger.LogWarning("Request with invalid API Key value for the '{HeaderName}' header.", _apiKeyConfig.HeaderName);
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "ApiKey")
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    /// <inheritdoc />
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = Scheme.Name;

        await WriteProblemDetailsAsync(
            StatusCodes.Status401Unauthorized,
            $"Missing or invalid API key. Provide the key in the '{_apiKeyConfig.HeaderName}' header.");
    }

    /// <inheritdoc />
    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;

        await WriteProblemDetailsAsync(
            StatusCodes.Status403Forbidden,
            "The provided API key does not have access to this resource.");
    }

    private async Task WriteProblemDetailsAsync(int statusCode, string detail)
    {
        var problemDetailsService = Context.RequestServices.GetService<IProblemDetailsService>();
        if (problemDetailsService is null)
        {
            return;
        }

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = Context,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = ReasonPhrases.GetReasonPhrase(statusCode),
                Detail = detail
            }
        });
    }

    private bool IsApiKeyValid(string providedApiKey)
    {
        // use constant time comparison to prevent timing attacks
        var expectedApiKey = Encoding.UTF8.GetBytes(_apiKeyConfig.ApiKey);
        var providedApiKeyBytes = Encoding.UTF8.GetBytes(providedApiKey);

        return CryptographicOperations.FixedTimeEquals(expectedApiKey, providedApiKeyBytes);
    }
}