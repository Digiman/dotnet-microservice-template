using Asp.Versioning.ApiExplorer;
using DotNet.ServiceName.Api.Infrastructure.Helpers;
using DotNet.ServiceName.Common.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Net;

namespace DotNet.ServiceName.Api.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configure Swagger UI for web application with versions support.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <param name="apiDescriptions">Descriptions of all API versions discovered from the mapped endpoints.</param>
    /// <returns>Returns updated object with application builder.</returns>
    public static IApplicationBuilder ConfigureSwagger(this IApplicationBuilder app, IEnumerable<ApiVersionDescription> apiDescriptions)
    {
        app.UseSwagger();

        // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
        // specifying the Swagger JSON endpoint.
        app.UseSwaggerUI(options =>
        {

            // build a swagger endpoint for each discovered API version
            foreach (var description in apiDescriptions)
            {
                var apiName = $"{Constants.ApiName} {description.GroupName.ToUpperInvariant()}";
                options.SwaggerEndpoint($"{description.GroupName}/swagger.json", apiName);
            }

            options.DocumentTitle = $"{Constants.ApiName} - Swagger UI";

            options.DocExpansion(DocExpansion.None);
        });

        return app;
    }

    /// <summary>
    /// Extends some features of Serilog. Added diagnostic context values.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <returns>Returns updated object with application builder.</returns>
    public static IApplicationBuilder ConfigureSerilog(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = LogHelper.EnrichFromRequest;
            options.GetLevel = LogHelper.ExcludeHealthChecks; // Use the custom level to filter the Health Checks from logs (information messages)
        });

        return app;
    }

    /// <summary>
    /// Configure application to work after the load balancers and proxies.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>Returns updated object with application builder.</returns>
    /// <remarks>
    /// See details here: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer
    /// By default only the loopback proxy is trusted. When running behind a reverse proxy or load
    /// balancer, list its IP addresses in the "ForwardedHeaders:KnownProxies" configuration section
    /// (e.g. "ForwardedHeaders": { "KnownProxies": [ "10.0.0.5" ] }) - otherwise X-Forwarded-* headers
    /// from any client would be trusted (spoofing the perceived client IP).
    /// </remarks>
    public static IApplicationBuilder ConfigureForwarderOptions(this IApplicationBuilder app, IConfiguration configuration)
    {
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.All
        };

        var knownProxies = configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>();
        if (knownProxies is { Length: > 0 })
        {
            // trust only the explicitly configured proxies
            forwardedHeadersOptions.KnownProxies.Clear();
            forwardedHeadersOptions.KnownIPNetworks.Clear();

            foreach (var proxy in knownProxies)
            {
                if (IPAddress.TryParse(proxy, out var ipAddress))
                {
                    forwardedHeadersOptions.KnownProxies.Add(ipAddress);
                }
            }
        }

        app.UseForwardedHeaders(forwardedHeadersOptions);

        return app;
    }

    /// <summary>
    /// Enable the configured CORS policy for the cross-origin browser clients.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>Returns updated object with application builder.</returns>
    /// <remarks>
    /// Registered before the authentication middleware so the preflight (OPTIONS) requests
    /// are answered without challenging for credentials.
    /// </remarks>
    public static IApplicationBuilder ConfigureCors(this IApplicationBuilder app, IConfiguration configuration)
    {
        var corsConfig = configuration.GetCorsPolicyConfiguration();

        if (corsConfig is { Enabled: true })
        {
            app.UseCors(Constants.CorsPolicyName);
        }

        return app;
    }

    /// <summary>
    /// Configure Security Headers for the web/api application.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <returns>Returns updated builder.</returns>
    /// <remarks>
    /// More details here: https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders
    /// And here: https://andrewlock.net/adding-default-security-headers-in-asp-net-core/
    /// </remarks>
    public static IApplicationBuilder ConfigureSecurityHeaders(this IApplicationBuilder app)
    {
        var policyHeaders = new HeaderPolicyCollection()
            .AddDefaultSecurityHeaders()
            .AddStrictTransportSecurityMaxAgeIncludeSubDomains()
            .AddPermissionsPolicy(builder =>
            {
                // recommended "secure" directives based on OWASP recommendations
                builder.AddDefaultSecureDirectives();
            })
            .RemoveCustomHeader("X-Powered-By");

        app.UseSecurityHeaders(policyHeaders);

        return app;
    }
}