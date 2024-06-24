using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Asp.Versioning;
using DotNet.ServiceName.Api.Infrastructure.Configuration;
using DotNet.ServiceName.Api.Infrastructure.HealthCheck;
using DotNet.ServiceName.Api.Infrastructure.Swagger;
using DotNet.ServiceName.Application;
using DotNet.ServiceName.Common.Extensions;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace DotNet.ServiceName.Api.Infrastructure.Extensions;

/// <summary>
/// Extensions for ServicesCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configure all services and dependencies for Web Api.
    /// </summary>
    /// <param name="services">Services collection with all dependencies.</param>
    /// <param name="configuration">Configuration of the whole application.</param>
    /// <param name="environment">Environment settings.</param>
    /// <param name="httpServices">Is it web server configuration?</param>
    /// <returns>Returns updates service collection after configuration.</returns>
    public static IServiceCollection ConfigureApiService(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        bool httpServices = false)
    {
        // TODO: add more configuration here!

        // add all DI modules
        services.AddExternalServices(configuration);

        //-----------------------------------------------
        // Web application specific
        //-----------------------------------------------

        // configure Web Server settings
        if (httpServices)
        {
            services.ConfigureHttpServices(environment, configuration);
        }

        //-----------------------------------------------

        return services;
    }

    private static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: register list of the dependencies here!
        services.AddConfiguration(configuration);
        // or use more complex with FluentValidation
        // services.AddConfigurationWithFluentValidation();
        services.AddApplication();

        return services;
    }

    /// <summary>
    /// Configure all settings related to HTTP web server.
    /// </summary>
    /// <param name="services">Services collection.</param>
    /// <param name="environment">Environment settings.</param>
    /// <param name="configuration">Configuration of the whole application.</param>
    /// <returns>Returns updates service collection.</returns>
    private static IServiceCollection ConfigureHttpServices(this IServiceCollection services,
        IWebHostEnvironment environment, IConfiguration configuration)
    {
        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
            //options.LowercaseQueryStrings = true;
        });

        // add API controllers here
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // add API versions
        services.ConfigureApiVersions();

        // add support for the integration with IHttpContextAccessor
        services.AddHttpContextAccessor();

        if (configuration.IsSwaggerEnabled())
        {
            // configure Swagger Gen rules to generate API documentation
            services.ConfigureSwaggerGeneration();
        }

        // configure error handling and return ProblemDetails standard object with details
        services.AddProblemDetails(options => ConfigureProblemDetails(options, environment));

        // add HealthCheck UI
        services.AddHealthChecksUiConfiguration(configuration);

        // add health checks
        services.AddHealthChecksConfiguration(configuration);

        return services;
    }

    /// <summary>
    /// Configure settings related to versioning of the API.
    /// </summary>
    /// <param name="services">Services collection.</param>
    /// <returns>Returns updates service collection.</returns>
    private static IServiceCollection ConfigureApiVersions(this IServiceCollection services)
    {
        // add API Version support
        services.AddApiVersioning(options =>
        {
            // reporting API versions will return the headers "api-supported-versions" and "api-deprecated-versions"
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        }).AddApiExplorer(options =>
        {
            // add the versioned API explorer, which also adds IApiVersionDescriptionProvider service
            // note: the specified format code will format the version as "'v'major[.minor][-status]"
            options.GroupNameFormat = "'v'VVV";

            // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
            // can also be used to control the format of the API version in route templates
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Configure Swagger Generation for API docs.
    /// </summary>
    /// <param name="services">Services collection.</param>
    /// <returns>Returns updates service collection.</returns>
    private static IServiceCollection ConfigureSwaggerGeneration(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        // Register the Swagger generator, defining 1 or more Swagger documents
        services.AddSwaggerGen(options =>
        {
            // enable annotations for detailed descriptions
            options.EnableAnnotations();

            // add a custom operation filter which sets default values
            options.OperationFilter<SwaggerDefaultValues>();

            // TODO: configure here the list of the XML files with documentation for Swagger!
            // Set the comments path for the Swagger JSON and UI.
            var xmlDocFiles = new[]
            {
                "DotNet.ServiceName.Api.xml",
                "DotNet.ServiceName.Application.xml",
                "DotNet.ServiceName.Common.xml"
            };
            foreach (var xmlDocFile in xmlDocFiles)
            {
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlDocFile);
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    /// <summary>
    /// Configure settings to process errors inside the application and API in general to return ProblemDetail result.
    /// </summary>
    /// <param name="options">Options for ProblemDetails middleware.</param>
    /// <param name="environment">Environment settings.</param>
    /// <remarks>
    /// More details and information provided by the links bellow:
    /// 1. https://www.alexdresko.com/2019/08/30/problem-details-error-handling-net-core-3/
    /// 2. https://tools.ietf.org/html/rfc7807 - standard for the ProblemDetails
    /// 3. https://lurumad.github.io/problem-details-an-standard-way-for-specifying-errors-in-http-api-responses-asp.net-core
    /// 4. https://andrewlock.net/handling-web-api-exceptions-with-problemdetails-middleware/
    /// </remarks>
    private static void ConfigureProblemDetails(ProblemDetailsOptions options, IWebHostEnvironment environment)
    {
        // TODO: add logic to map the exceptions to generate proper ProblemDetails object to report about error inside the API
        // TODO: configure and think better about return codes for all possible situations (4xx or 5xx)

        // This is the default behavior; only include exception details in a local development environment.
        options.IncludeExceptionDetails = (ctx, ex) => (environment.IsEnvironment("Local"));

        // Map custom validation exception with validation errors in the pipeline (MediatR CQRS)
        //options.Map<ValidationException>(exception => new ValidationProblemDetails(exception.Errors));
        // options.Map<ValidationException>(exception =>
        // {
        //     var validationProblemDetails  = new ValidationProblemDetails(exception.Errors);
        //     validationProblemDetails.Status = StatusCodes.Status422UnprocessableEntity;
        //     return validationProblemDetails;
        // });

        // This will map NotImplementedException to the 501 Not Implemented status code.
        options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);

        // This will map HttpRequestException to the 503 Service Unavailable status code.
        options.MapToStatusCode<HttpRequestException>(StatusCodes.Status503ServiceUnavailable);

        // Because exceptions are handled polymorphically, this will act as a "catch all" mapping, which is why it's added last.
        // If an exception other than NotImplementedException and HttpRequestException is thrown, this will handle it.
        options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Adds the health checks UI configuration.
    /// </summary>
    /// <param name="services">Services collection.</param>
    /// <param name="configuration">Application configuration.</param>
    private static void AddHealthChecksUiConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var healthCheckConfig = configuration.GetHealthCheckConfiguration();

        if (healthCheckConfig is { HealthCheckUiEnabled: true })
        {
            services
                .AddHealthChecksUI(opt =>
                {
                    opt.SetHeaderText(healthCheckConfig.HeaderText);
                    opt.SetEvaluationTimeInSeconds(healthCheckConfig.EvaluationTimeInSeconds);
                    opt.MaximumHistoryEntriesPerEndpoint(healthCheckConfig.MaximumHistoryEntriesPerEndpoint);
                    opt.SetApiMaxActiveRequests(1);
                    opt.AddHealthCheckEndpoint("All", GenerateHealthCheckUrl(configuration, "/health"));
                    opt.AddHealthCheckEndpoint("Liveness", GenerateHealthCheckUrl(configuration, "/health/live"));
                    opt.AddHealthCheckEndpoint("Readiness", GenerateHealthCheckUrl(configuration, "/health/ready"));
                })
                .AddInMemoryStorage();
        }
    }

    private static string GenerateHealthCheckUrl(IConfiguration configuration, string url)
    {
        var urlsForApplication = configuration["ASPNETCORE_URLS"];
        if (!string.IsNullOrEmpty(urlsForApplication))
        {
            var urls = urlsForApplication.Split(';');
            var uris = urls.Select(url => Regex.Replace(url,
                    @"^(?<scheme>https?):\/\/((\+)|(\*)|(0.0.0.0))(?=[\:\/]|$)", "${scheme}://localhost"))
                .Select(uri => new Uri(uri, UriKind.Absolute)).ToArray();
            var httpEndpoint = uris.FirstOrDefault(uri => uri.Scheme == "http");
            var httpsEndpoint = uris.FirstOrDefault(uri => uri.Scheme == "https");

            string fullUrl = url;
            if (httpEndpoint != null) // Create an HTTP healthcheck endpoint
            {
                fullUrl = new UriBuilder(httpEndpoint.Scheme, httpEndpoint.Host, httpEndpoint.Port, url).ToString();
            }
            else if (httpsEndpoint != null) // Create an HTTPS healthcheck endpoint
            {
                fullUrl = new UriBuilder(httpsEndpoint.Scheme, httpsEndpoint.Host, httpsEndpoint.Port, url).ToString();
            }

            return fullUrl;
        }

        return url;
    }

    /// <summary>
    /// Add the health checks configuration and entries.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>Returns <see cref="IHealthChecksBuilder"/>.</returns>
    private static IHealthChecksBuilder AddHealthChecksConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var builder = services.AddHealthChecks()
            .AddMemoryHealthCheck(HealthStatus.Degraded, new[] { "internal", "monitoring" });

        return builder;
    }
}