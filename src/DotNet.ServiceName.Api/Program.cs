using Asp.Versioning.ApiExplorer;
using DotNet.ServiceName.Api.Infrastructure.Extensions;
using DotNet.ServiceName.Common.Extensions;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
ConfigureApplication(app, builder.Environment, apiVersionDescriptionProvider);

// run the application
await app.RunAsync();

void ConfigureServices()
{
    builder.Services.ConfigureApiService(builder.Configuration, builder.Environment, true);
}

void ConfigureApplication(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider apiProvider)
{
    var healthCheckConfig = builder.Configuration.GetHealthCheckConfiguration();

    // configure Forwarder headers for proxies and Load Balancers
    app.ConfigureForwarderOptions();

    if (!env.IsEnvironment("Local"))
    {
        // Use HSTS default settings (default for 30 days)
        // app.UseHsts();

        // custom configuration for security headers (HSTS for 60 days)
        app.ConfigureSecurityHeaders();
    }

    // redirect to the HTTPS connection
    app.UseHttpsRedirection();

    // Add using ProblemDetail middleware to handle errors and use RFC-7807 standard
    app.UseProblemDetails();

    if (builder.Configuration.IsSwaggerEnabled())
    {
        // configure Swagger UI
        app.ConfigureSwagger(apiProvider);
    }

    // add logger for all requests in the web server
    app.ConfigureSerilog();

    // use default files
    app.UseDefaultFiles();

    // allow to use static files
    app.UseStaticFiles();

    // Use routing middleware to handle requests to the controllers
    app.UseRouting();

    // configure endpoints routing
    app.UseEndpoints(endpoints =>
    {
        // add controllers endpoints
        endpoints.MapControllers();

        // add health checks endpoints and configurations
        endpoints.AddHealthcheckEndpoints(healthCheckConfig);
    });
}