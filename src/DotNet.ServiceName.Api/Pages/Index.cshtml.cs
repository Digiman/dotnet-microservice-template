using DotNet.ServiceName.Api.Infrastructure;
using DotNet.ServiceName.Api.Infrastructure.Models;
using DotNet.ServiceName.Common.Configuration;
using DotNet.ServiceName.Common.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace DotNet.ServiceName.Api.Pages;

/// <summary>
/// Home page of the service. The links are derived from the configuration of the running
/// environment, so a production deployment without Swagger and Scalar shows only what it exposes.
/// </summary>
public sealed class IndexModel(IConfiguration configuration, IWebHostEnvironment environment) : PageModel
{
    /// <summary>
    /// Content of the page.
    /// </summary>
    public HomePageModel Home { get; private set; } = null!;

    /// <summary>
    /// Builds the page content from the configuration of the current environment.
    /// </summary>
    public void OnGet()
    {
        var options = configuration.GetHomePageConfiguration() ?? new HomePageOptions { Title = Constants.ApiName };
        var documentationEnabled = configuration.IsSwaggerEnabled();
        var dashboardEnabled = configuration.GetHealthCheckConfiguration() is { HealthCheckUiEnabled: true };

        var links = new List<HomePageLink>();

        if (options.ShowDocumentation && documentationEnabled)
        {
            links.Add(new HomePageLink("Swagger", "/swagger", "Interactive API documentation", "/icons/swagger.svg"));
            links.Add(new HomePageLink("Scalar", "/scalar", "Alternative API reference", "/icons/scalar.svg"));
            links.Add(new HomePageLink("Facets", "/facets", "Generated DTOs and mappers", "/icons/facets.svg"));
        }

        if (options.ShowHealthChecks)
        {
            links.Add(new HomePageLink("Health status", "/health", "Detailed health report", "/icons/simple-healthcheck.svg"));

            if (dashboardEnabled)
            {
                links.Add(new HomePageLink("Health dashboard", "/healthcheck-dashboard", "History and status of all checks",
                    "/icons/healthcheck-dashboard.svg"));
            }
        }

        links.AddRange(options.Links
            .Where(link => link.Enabled)
            .Select(link => new HomePageLink(link.Title, link.Url, link.Description, link.Icon, link.OpenInNewTab)));

        Home = new HomePageModel
        {
            Title = options.Title,
            Description = options.Description,
            Environment = environment.EnvironmentName,
            ShowEnvironment = options.ShowEnvironment,
            Links = links,
            DocumentationDisabled = options.ShowDocumentation && !documentationEnabled
        };
    }
}