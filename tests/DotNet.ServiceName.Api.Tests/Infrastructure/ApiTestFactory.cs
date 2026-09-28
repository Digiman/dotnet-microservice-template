using DotNet.ServiceName.Common.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.ServiceName.Api.Tests.Infrastructure;

/// <summary>
/// Boots the API in-process for integration tests using the "Local" environment configuration.
/// </summary>
public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// API key configured for the Local environment (appsettings.json).
    /// </summary>
    public const string ApiKey = "local-dev-api-key";

    /// <summary>
    /// HTTP header name used to send the API key.
    /// </summary>
    public const string ApiKeyHeader = "X-API-Key";

    static ApiTestFactory()
    {
        // WebApplication.CreateBuilder reads the environment from process variables before
        // ConfigureWebHost runs - set it up front so appsettings.Local.json is loaded
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Local");
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Local");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            // the health check UI runs a background collector polling itself over HTTP -
            // skip it in tests, the raw health endpoints are still covered
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HealthCheckOptions:HealthCheckUiEnabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            // the test host's entry assembly is "testhost" - without this, AddControllers
            // would not discover the API's controllers and all API routes would 404
            var apiAssembly = typeof(Program).Assembly;
            var partManager = services.AddControllers().PartManager;
            if (!partManager.ApplicationParts.Any(part => part.Name == apiAssembly.GetName().Name))
            {
                partManager.ApplicationParts.Add(new AssemblyPart(apiAssembly));
            }

            // keep the memory health check deterministic - the configured threshold
            // could be exceeded by the test host itself and flip the check to degraded
            services.Configure<MemoryCheckOptions>(
                nameof(MemoryCheckOptions),
                options => options.Threshold = long.MaxValue);
        });
    }
}