using DotNet.ServiceName.Common.Configuration;
using Microsoft.Extensions.Configuration;

namespace DotNet.ServiceName.Common.Extensions;

/// <summary>
/// Simple extensions for configuration.
/// </summary>
public static class ConfigurationExtensions
{
    public static HealthCheckOptions? GetHealthCheckConfiguration(this IConfiguration configuration)
    {
        return configuration.GetSection(nameof(HealthCheckOptions)).Get<HealthCheckOptions>();
    }

    public static MemoryCheckOptions? GetMemoryCheckConfiguration(this IConfiguration configuration)
    {
        return configuration.GetSection(nameof(MemoryCheckOptions)).Get<MemoryCheckOptions>();
    }

    public static bool IsSwaggerEnabled(this IConfiguration configuration)
    {
        return System.Convert.ToBoolean(configuration["SwaggerEnabled"]);
    }

    public static bool IsSwaggerAuthEnabled(this IConfiguration configuration)
    {
        return System.Convert.ToBoolean(configuration["SwaggerAuth"]);
    }

    public static ApiKeyOptions? GetApiKeyConfiguration(this IConfiguration configuration)
    {
        return configuration.GetSection(nameof(ApiKeyOptions)).Get<ApiKeyOptions>();
    }

    public static CorsPolicyOptions? GetCorsPolicyConfiguration(this IConfiguration configuration)
    {
        return configuration.GetSection(nameof(CorsPolicyOptions)).Get<CorsPolicyOptions>();
    }

    public static RateLimitingOptions? GetRateLimitingConfiguration(this IConfiguration configuration)
    {
        return configuration.GetSection(nameof(RateLimitingOptions)).Get<RateLimitingOptions>();
    }

    public static HttpTimeoutOptions? GetHttpTimeoutConfiguration(this IConfiguration configuration)
    {
        return configuration.GetSection(nameof(HttpTimeoutOptions)).Get<HttpTimeoutOptions>();
    }

    public static TelemetryOptions? GetTelemetryConfiguration(this IConfiguration configuration)
    {
        return configuration.GetSection(nameof(TelemetryOptions)).Get<TelemetryOptions>();
    }
}