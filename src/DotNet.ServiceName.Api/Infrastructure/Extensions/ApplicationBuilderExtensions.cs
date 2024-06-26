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
    /// <param name="provider">Provider for API versions.</param>
    /// <returns>Returns updated object with application builder.</returns>
    public static IApplicationBuilder ConfigureSwagger(this IApplicationBuilder app, IApiVersionDescriptionProvider provider)
    {
        app.UseSwagger();

        // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
        // specifying the Swagger JSON endpoint.
        app.UseSwaggerUI(options =>
        {

            // build a swagger endpoint for each discovered API version
            foreach (var description in provider.ApiVersionDescriptions)
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
        forwardedHeadersOptions.KnownNetworks.Clear();
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
        const int hstsMaxAgeInSeconds = 60 * 60 * 24 * 60; // 60 days in seconds

        var policyHeaders = new HeaderPolicyCollection()
            .AddDefaultSecurityHeaders()
            .AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: hstsMaxAgeInSeconds)
            .AddPermissionsPolicy(builder =>
            {
                builder.AddDefaultPermissionsPolicies();
            })
            .RemoveCustomHeader("X-Powered-By");// 60 days in seconds

        app.UseSecurityHeaders(policyHeaders);

        return app;
    }

    /// <summary>
    /// Configure permissions policy HTTP header for the browser.
    /// </summary>
    /// <param name="builder">Permission policy builder.</param>
    /// <remarks>
    /// See more here:
    /// 1. https://www.w3.org/TR/permissions-policy-1/
    /// 2. https://github.com/w3c/webappsec-permissions-policy/blob/main/permissions-policy-explainer.md
    /// 3. https://w3c.github.io/webappsec-permissions-policy/
    /// 4. https://www.permissionspolicy.com/
    /// </remarks>
    private static void AddDefaultPermissionsPolicies(this PermissionsPolicyBuilder builder)
    {
        builder.AddAccelerometer() // accelerometer 'none'
            .None();

        // non supported in Google
        // builder.AddAmbientLightSensor() // ambient-light-sensor 'none'
        //     .None();

        builder.AddAutoplay() // autoplay 'none'
            .None();

        builder.AddCamera() // camera 'none'
            .None();

        builder.AddEncryptedMedia() // encrypted-media 'none'
            .None();

        builder.AddFullscreen() // fullscreen *
            .All();

        builder.AddGeolocation() // geolocation 'none'
            .None();

        builder.AddGyroscope() // gyroscope 'none'
            .None();

        builder.AddMagnetometer() // magnetometer 'none'
            .None();

        builder.AddMicrophone() // microphone 'none'
            .None();

        builder.AddMidi() // midi 'none'
            .None();

        builder.AddPayment() // payment 'none'
            .None();

        builder.AddPictureInPicture() // picture-in-picture 'none'
            .None();

        // non supported in Google
        // builder.AddSpeaker() // speaker 'none'
        //     .None();

        builder.AddSyncXHR() // sync-xhr 'none'
            .None();

        builder.AddUsb() // usb 'none'
            .None();

        // non supported in Google
        // builder.AddVR() // vr 'none'
        //     .None();
    }
}