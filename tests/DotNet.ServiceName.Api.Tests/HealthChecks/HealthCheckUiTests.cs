using DotNet.ServiceName.Api.Tests.Infrastructure;
using System.Net;
using Xunit;

namespace DotNet.ServiceName.Api.Tests.HealthChecks;

/// <summary>
/// The Health Checks dashboard is a packaged single page application - the page must serve the
/// configured header text as its title and load the custom stylesheet after its own one.
/// </summary>
[Collection("Api")]
public sealed class HealthCheckUiTests : IClassFixture<HealthCheckUiApiTestFactory>
{
    private readonly HealthCheckUiApiTestFactory _factory;

    public HealthCheckUiTests(HealthCheckUiApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Dashboard_ReturnsPageWithConfiguredTitle()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/healthcheck-dashboard");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadAsStringAsync();
        Assert.Contains("<title>Simple Service - Health Checks Status</title>", page);
    }

    [Fact]
    public async Task Dashboard_LoadsCustomStylesheetAfterTheDefaultOne()
    {
        using var client = _factory.CreateClient();

        var page = await (await client.GetAsync("/healthcheck-dashboard")).Content.ReadAsStringAsync();

        var defaultStylesheet = page.IndexOf("healthchecksui-min.css", StringComparison.Ordinal);
        var customStylesheet = page.IndexOf("healthcheck-dashboard.css", StringComparison.Ordinal);

        Assert.True(defaultStylesheet >= 0, "the dashboard default stylesheet is missing");
        Assert.True(customStylesheet > defaultStylesheet, "the custom stylesheet must be loaded last to win");
    }

    [Fact]
    public async Task Dashboard_CustomStylesheet_IsServedAsCss()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/ui/resources/css/healthcheck-dashboard.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
        var styles = await response.Content.ReadAsStringAsync();
        Assert.Contains("--logoImageUrl: url('/images/healthcheck-logo.svg')", styles);
    }

    [Fact]
    public async Task Dashboard_Logo_IsServedFromWwwroot()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/images/healthcheck-logo.svg");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/svg+xml", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Dashboard_MissingStylesheet_FallsBackToTheDefaultStyles()
    {
        using var factory = new MissingStylesheetApiTestFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/healthcheck-dashboard");

        // a stylesheet that is not in wwwroot must not break the dashboard
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("does-not-exist.css", page);
    }
}