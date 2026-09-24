using Asp.Versioning.ApiExplorer;
using DotNet.ServiceName.Api.Infrastructure.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;

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
    /// <returns>Returns updated object with application builder.</returns>
    /// <remarks>
    /// See details here: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-5.0
    /// </remarks>
    public static IApplicationBuilder ConfigureForwarderOptions(this IApplicationBuilder app)
    {
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.All
        };
        forwardedHeadersOptions.KnownProxies.Clear();
        forwardedHeadersOptions.KnownIPNetworks.Clear();
        app.UseForwardedHeaders(forwardedHeadersOptions);

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