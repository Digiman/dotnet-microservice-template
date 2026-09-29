using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace DotNet.ServiceName.Api.Tests.Infrastructure;

/// <summary>
/// Boots the API with the Health Checks dashboard enabled so the dashboard page and its
/// custom stylesheet can be exercised.
/// </summary>
public sealed class HealthCheckUiApiTestFactory : ApiTestFactory
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthCheckOptions:HealthCheckUiEnabled"] = "true",
                ["HealthCheckOptions:CustomStylesheet"] = "css/healthcheck-dashboard.css"
            });
        });
    }
}