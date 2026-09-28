using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace DotNet.ServiceName.Api.Tests.Infrastructure;

/// <summary>
/// Boots the API with a very low rate limit so the 429 responses can be exercised quickly.
/// </summary>
public sealed class RateLimitedApiTestFactory : ApiTestFactory
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RateLimitingOptions:PermitLimit"] = "2"
            });
        });
    }
}