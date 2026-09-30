using DotNet.ServiceName.Api.Infrastructure.Extensions;
using DotNet.ServiceName.Common.Extensions;
using DotNet.ServiceName.ServiceDefaults;
using Facet.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

var builder = WebApplication.CreateBuilder();

// add the shared service defaults - OpenTelemetry, service discovery and
// HttpClient resilience (see DotNet.ServiceName.ServiceDefaults)
builder.AddServiceDefaults();

// configure Serilog for logging
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);

    // forward the logs over OTLP when an endpoint is configured - the Aspire AppHost
    // injects the dashboard endpoint, docker-compose points at the local collector
    var otlpEndpoint = context.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        // the sink builds its own OTLP resource, so the service name must be given to it
        // explicitly - resolved the same way as in ServiceDefaults (environment preferred)
        // to keep every signal reported under one name
        var telemetryConfig = context.Configuration.GetTelemetryConfiguration();
        var serviceName = context.Configuration["OTEL_SERVICE_NAME"] is { Length: > 0 } injectedName
            ? injectedName
            : telemetryConfig?.ServiceName ?? "dotnet-servicename";

        loggerConfiguration.WriteTo.OpenTelemetry(
            otlpEndpoint,
            OtlpProtocol.HttpProtobuf,
            resourceAttributes: new Dictionary<string, object>
            {
                ["service.name"] = serviceName,
                ["service.version"] = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
            });
    }
});

// configure application services
ConfigureServices();

// create the app to configure the middleware
var app = builder.Build();

// configure the web app middleware components
ConfigureApplication(app, builder.Environment);

// run the application
await app.RunAsync();
return;

void ConfigureServices()
{
    builder.Services.ConfigureApiService(builder.Configuration, builder.Environment, true);
}

void ConfigureApplication(WebApplication appBuilder, IWebHostEnvironment env)
{
    var healthCheckConfig = builder.Configuration.GetHealthCheckConfiguration();

    // configure Forwarder headers for proxies and Load Balancers
    appBuilder.ConfigureForwarderOptions(builder.Configuration);

    if (!env.IsEnvironment("Local"))
    {
        // custom configuration for security headers
        appBuilder.ConfigureSecurityHeaders();
    }

    // redirect to the HTTPS connection
    appBuilder.UseHttpsRedirection();

    // Add using exception handler middleware to handle errors and use RFC-7807 standard (ProblemDetails)
    appBuilder.UseExceptionHandler();

    // add logger for all requests in the web server
    appBuilder.ConfigureSerilog();

    // enable cross-origin requests, rate limiting and request timeouts - all before authentication
    appBuilder.ConfigureCors(builder.Configuration);
    appBuilder.UseRateLimiter();
    appBuilder.UseRequestTimeouts();

    // enable Authentication and Authorization middlewares - API Key is validated for all secured endpoints
    appBuilder.UseAuthentication();
    appBuilder.UseAuthorization();

    // use default files
    appBuilder.UseDefaultFiles();

    // allow using static files
    appBuilder.UseStaticFiles();

    // add controller endpoints
    appBuilder.MapControllers();

    // add the Razor Pages of the service (home page) when it is enabled in configuration
    if (builder.Configuration.GetHomePageConfiguration() is { Enabled: true })
    {
        appBuilder.MapRazorPages();
    }

    // add health checks, endpoints and configurations
    appBuilder.AddHealthcheckEndpoints(healthCheckConfig);

    if (builder.Configuration.IsSwaggerEnabled())
    {
        // configure Swagger UI with API versions discovered from the mapped endpoints
        appBuilder.ConfigureSwagger(appBuilder.DescribeApiVersions());

        // configure Scalar API reference as an alternative UI for the same OpenAPI documents
        appBuilder.AddScalarApiReferenceEndpoint(builder.Configuration);

        // configure Facet Dashboard page with configuration for all facets
        appBuilder.MapFacetDashboard();
    }
}

/// <summary>
/// Marker class to expose the entry point for integration tests (WebApplicationFactory&lt;Program&gt;).
/// </summary>
public partial class Program;