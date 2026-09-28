using DotNet.ServiceName.Api.Pages;
using DotNet.ServiceName.Api.Tests.Infrastructure;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DotNet.ServiceName.Api.Tests.HomePage;

/// <summary>
/// The home page is a Razor Page whose links follow the configuration of the running
/// environment, so a deployment without Swagger and Scalar must not show them.
/// </summary>
public sealed class HomePageModelTests
{
    private static readonly Dictionary<string, string?> BaseSettings = new()
    {
        ["SwaggerEnabled"] = "true",
        ["HealthCheckOptions:HealthCheckUiEnabled"] = "true",
        ["HealthCheckOptions:HeaderText"] = "Health Checks Status",
        ["HomePageOptions:Enabled"] = "true",
        ["HomePageOptions:Title"] = "Simple Service API",
        ["HomePageOptions:ShowEnvironment"] = "true",
        ["HomePageOptions:ShowDocumentation"] = "true",
        ["HomePageOptions:ShowHealthChecks"] = "true"
    };

    [Fact]
    public void HomePage_RendersDocumentationAndHealthLinks()
    {
        var page = Build(new Dictionary<string, string?>());

        Assert.Equal("Simple Service API", page.Home.Title);
        Assert.True(page.Home.ShowEnvironment);
        Assert.Equal("Local", page.Home.Environment);
        Assert.False(page.Home.DocumentationDisabled);
        Assert.Equal(
            ["/swagger", "/scalar", "/facets", "/health", "/healthcheck-dashboard"],
            page.Home.Links.Select(link => link.Url));
    }

    [Fact]
    public void HomePage_WithoutSwagger_HidesTheDocumentationLinks()
    {
        var page = Build(new Dictionary<string, string?> { ["SwaggerEnabled"] = "false" });

        Assert.DoesNotContain(page.Home.Links, link => link.Url == "/swagger");
        Assert.DoesNotContain(page.Home.Links, link => link.Url == "/scalar");
        Assert.DoesNotContain(page.Home.Links, link => link.Url == "/facets");
        // the page says so instead of silently showing fewer links
        Assert.True(page.Home.DocumentationDisabled);
        Assert.Contains(page.Home.Links, link => link.Url == "/health");
    }

    [Fact]
    public void HomePage_WithoutHealthCheckUi_HidesTheDashboardLink()
    {
        var page = Build(new Dictionary<string, string?> { ["HealthCheckOptions:HealthCheckUiEnabled"] = "false" });

        Assert.DoesNotContain(page.Home.Links, link => link.Url == "/healthcheck-dashboard");
        Assert.Contains(page.Home.Links, link => link.Url == "/health");
    }

    [Fact]
    public void HomePage_WithGroupsDisabled_ShowsNoLinks()
    {
        var page = Build(new Dictionary<string, string?>
        {
            ["HomePageOptions:ShowDocumentation"] = "false",
            ["HomePageOptions:ShowHealthChecks"] = "false"
        });

        Assert.Empty(page.Home.Links);
        Assert.False(page.Home.DocumentationDisabled);
    }

    [Fact]
    public void HomePage_RendersEnabledCustomLinksOnly()
    {
        var page = Build(new Dictionary<string, string?>
        {
            ["HomePageOptions:Links:0:Title"] = "Runbook",
            ["HomePageOptions:Links:0:Description"] = "Operations documentation",
            ["HomePageOptions:Links:0:Url"] = "https://www.companyname.net/runbooks/service-name",
            ["HomePageOptions:Links:0:Icon"] = "/icons/facets.svg",
            ["HomePageOptions:Links:0:OpenInNewTab"] = "true",
            ["HomePageOptions:Links:1:Title"] = "Mailbox",
            ["HomePageOptions:Links:1:Url"] = "https://www.companyname.net/mailbox",
            ["HomePageOptions:Links:1:Enabled"] = "false"
        });

        var custom = Assert.Single(page.Home.Links, link => link.Url.StartsWith("https://", StringComparison.Ordinal));
        Assert.Equal("Runbook", custom.Title);
        Assert.Equal("Operations documentation", custom.Description);
        Assert.Equal("/icons/facets.svg", custom.Icon);
        Assert.True(custom.OpenInNewTab);
        // a link disabled in configuration is skipped
        Assert.DoesNotContain(page.Home.Links, link => link.Title == "Mailbox");
    }

    [Fact]
    public void HomePage_UsesConfiguredTitleDescriptionAndEnvironmentBadge()
    {
        var page = Build(new Dictionary<string, string?>
        {
            ["HomePageOptions:Title"] = "Payments API",
            ["HomePageOptions:Description"] = "Handles the payments",
            ["HomePageOptions:ShowEnvironment"] = "false"
        });

        Assert.Equal("Payments API", page.Home.Title);
        Assert.Equal("Handles the payments", page.Home.Description);
        Assert.False(page.Home.ShowEnvironment);
    }

    [Fact]
    public void HomePage_SampleProductionConfiguration_HidesTheDocumentation()
    {
        // the shipped appsettings.Production.json is the sample of a deployment without Swagger
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Production.json"), optional: false)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthCheckOptions:HealthCheckUiEnabled"] = "true",
                ["HealthCheckOptions:HeaderText"] = "Health Checks Status",
                ["HomePageOptions:Enabled"] = "true"
            })
            .Build();

        var page = new IndexModel(configuration, new TestWebHostEnvironment { EnvironmentName = "Production" });
        page.OnGet();

        Assert.Equal("Simple Service API", page.Home.Title);
        Assert.Equal("Production deployment", page.Home.Description);
        Assert.False(page.Home.ShowEnvironment);
        Assert.True(page.Home.DocumentationDisabled);
        Assert.DoesNotContain(page.Home.Links, link => link.Url == "/swagger");
        Assert.DoesNotContain(page.Home.Links, link => link.Url == "/scalar");
        Assert.Contains(page.Home.Links, link => link.Url == "https://www.companyname.net/runbooks/service-name");
        Assert.DoesNotContain(page.Home.Links, link => link.Url == "https://www.companyname.net/mailbox");
    }

    private static IndexModel Build(Dictionary<string, string?> overrides)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(BaseSettings)
            .AddInMemoryCollection(overrides)
            .Build();

        var page = new IndexModel(configuration, new TestWebHostEnvironment());
        page.OnGet();

        return page;
    }
}