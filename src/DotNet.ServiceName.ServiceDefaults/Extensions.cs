using DotNet.ServiceName.Common.Configuration;
using DotNet.ServiceName.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DotNet.ServiceName.ServiceDefaults;

/// <summary>
/// Shared defaults every service of the solution wires up: OpenTelemetry (driven by the
/// TelemetryOptions section of appsettings.json), a liveness health check, service
/// discovery and a standard resilience pipeline for outbound HttpClient calls.
///
/// The template maps its own health endpoints (see AddHealthcheckEndpoints of the Api
/// project), so unlike the stock Aspire service defaults this class deliberately does
/// NOT map /health and /alive - doing both would fail at startup with duplicate endpoints.
/// </summary>
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    /// <summary>
    /// Adds the service defaults to the host (services + configuration of the app).
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // turn on resilience by default
            http.AddStandardResilienceHandler();

            // turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>
    /// Configures the OpenTelemetry traces, metrics and logging bridge.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var telemetryConfig = builder.Configuration.GetTelemetryConfiguration();

        if (telemetryConfig is not { Enabled: true })
        {
            return builder;
        }

        // Aspire injects OTEL_SERVICE_NAME with the resource name of the service, so the
        // dashboard groups the telemetry under the same name as the resource that emitted
        // it - prefer it over the configured name. A standalone run (no AppHost) keeps the
        // name from TelemetryOptions.
        var serviceName = builder.Configuration["OTEL_SERVICE_NAME"] is { Length: > 0 } injectedName
            ? injectedName
            : telemetryConfig.ServiceName;

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: serviceName,
                serviceVersion: typeof(Extensions).Assembly.GetName().Version?.ToString()))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                ConfigureExporters(metrics, telemetryConfig);
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        // exclude the health endpoints from tracing to keep the signal clean
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath))
                    .AddHttpClientInstrumentation();

                ConfigureExporters(tracing, telemetryConfig);
            });

        return builder;
    }

    /// <summary>
    /// Adds a default liveness check to ensure the app is responsive.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The builder for chaining.</returns>
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Registers the configured exporters on the metrics pipeline.
    /// </summary>
    /// <param name="builder">The metrics pipeline builder.</param>
    /// <param name="telemetryConfig">Telemetry configuration.</param>
    private static void ConfigureExporters(MeterProviderBuilder builder, TelemetryOptions telemetryConfig)
    {
        if (telemetryConfig.ConsoleExporter)
        {
            builder.AddConsoleExporter();
        }

        builder.AddOtlpExporter(exporter =>
        {
            if (!string.IsNullOrEmpty(telemetryConfig.OtlpEndpoint))
            {
                exporter.Endpoint = new Uri(telemetryConfig.OtlpEndpoint, UriKind.Absolute);
            }
        });
    }

    /// <summary>
    /// Registers the configured exporters on the tracing pipeline.
    /// </summary>
    /// <param name="builder">The tracing pipeline builder.</param>
    /// <param name="telemetryConfig">Telemetry configuration.</param>
    private static void ConfigureExporters(TracerProviderBuilder builder, TelemetryOptions telemetryConfig)
    {
        if (telemetryConfig.ConsoleExporter)
        {
            builder.AddConsoleExporter();
        }

        builder.AddOtlpExporter(exporter =>
        {
            if (!string.IsNullOrEmpty(telemetryConfig.OtlpEndpoint))
            {
                exporter.Endpoint = new Uri(telemetryConfig.OtlpEndpoint, UriKind.Absolute);
            }
        });
    }
}