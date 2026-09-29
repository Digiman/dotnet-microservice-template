using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace DotNet.ServiceName.Api.Tests.Infrastructure;

/// <summary>
/// Boots the API with a custom dashboard stylesheet that does not exist in wwwroot, so the
/// fallback to the dashboard defaults can be exercised.
/// </summary>
public sealed class MissingStylesheetApiTestFactory : ApiTestFactory
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
                ["HealthCheckOptions:CustomStylesheet"] = "css/does-not-exist.css"
            });
        });
    }
}