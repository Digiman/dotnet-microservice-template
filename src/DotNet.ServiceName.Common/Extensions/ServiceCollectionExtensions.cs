using DotNet.ServiceName.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.ServiceName.Common.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure application custom configuration with default validation with Data Annotations.
    /// </summary>
    /// <param name="services">Services collection.</param>
    /// <param name="configuration">Application configuration.</param>
    public static void AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWithValidation<HealthCheckOptions>(nameof(HealthCheckOptions));
        services.AddWithValidation<MemoryCheckOptions>(nameof(MemoryCheckOptions));
        services.AddWithValidation<ApiKeyOptions>(nameof(ApiKeyOptions));
        services.AddWithValidation<HomePageOptions>(nameof(HomePageOptions));
        services.AddWithValidation<CorsPolicyOptions>(nameof(CorsPolicyOptions));
        services.AddWithValidation<RateLimitingOptions>(nameof(RateLimitingOptions));
        services.AddWithValidation<HttpTimeoutOptions>(nameof(HttpTimeoutOptions));
        services.AddWithValidation<TelemetryOptions>(nameof(TelemetryOptions));
    }

    /// <summary>
    /// Configure application custom configuration wth validation by using FluentValidation.
    /// </summary>
    /// <param name="services">Services collection.</param>
    public static void AddConfigurationWithFluentValidation(this IServiceCollection services)
    {
        services.AddWithValidation<HealthCheckOptions, HealthCheckOptionsValidator>(nameof(HealthCheckOptions));
        services.AddWithValidation<MemoryCheckOptions, MemoryCheckOptionsValidator>(nameof(MemoryCheckOptions));
        services.AddWithValidation<ApiKeyOptions, ApiKeyOptionsValidator>(nameof(ApiKeyOptions));
        services.AddWithValidation<HomePageOptions, HomePageOptionsValidator>(nameof(HomePageOptions));
        services.AddWithValidation<CorsPolicyOptions, CorsPolicyOptionsValidator>(nameof(CorsPolicyOptions));
        services.AddWithValidation<RateLimitingOptions, RateLimitingOptionsValidator>(nameof(RateLimitingOptions));
        services.AddWithValidation<HttpTimeoutOptions, HttpTimeoutOptionsValidator>(nameof(HttpTimeoutOptions));
        services.AddWithValidation<TelemetryOptions, TelemetryOptionsValidator>(nameof(TelemetryOptions));
    }
}