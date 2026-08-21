using DotNet.ServiceName.Api.Infrastructure.Extensions;
using DotNet.ServiceName.Common.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder();

// configure Serilog for logging
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

// configure application services
ConfigureServices();

// create the app to configure the middleware
var app = builder.Build();

// configure the web app middleware components
ConfigureApplication(app, builder.Environment);

// run the application
await app.RunAsync();

void ConfigureServices()
{
    builder.Services.ConfigureApiService(builder.Configuration, builder.Environment, true);
}

void ConfigureApplication(WebApplication appBuilder, IWebHostEnvironment env)
{
    var healthCheckConfig = builder.Configuration.GetHealthCheckConfiguration();

    // configure Forwarder headers for proxies and Load Balancers
    appBuilder.ConfigureForwarderOptions();

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

    // use default files
    appBuilder.UseDefaultFiles();

    // allow to use static files
    appBuilder.UseStaticFiles();

    // add controllers endpoints
    appBuilder.MapControllers();

    // add health checks endpoints and configurations
    appBuilder.AddHealthcheckEndpoints(healthCheckConfig);

    if (builder.Configuration.IsSwaggerEnabled())
    {
        // configure Swagger UI with API versions discovered from the mapped endpoints
        appBuilder.ConfigureSwagger(appBuilder.DescribeApiVersions());
    }
}