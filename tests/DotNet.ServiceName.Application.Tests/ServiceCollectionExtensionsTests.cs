using DotNet.ServiceName.Application.Services;
using DotNet.ServiceName.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotNet.ServiceName.Application.Tests;

/// <summary>
/// Tests for the Application dependency registration.
/// </summary>
public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplication_RegistersApplicationStatusService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        // use logger substitute to keep the test independent from the real logging infrastructure
        services.AddSingleton(Substitute.For<ILogger<ApplicationStatusService>>());

        services.AddApplication();

        using var provider = services.BuildServiceProvider();
        var service = provider.GetService<IApplicationStatusService>();

        Assert.NotNull(service);
        Assert.IsType<ApplicationStatusService>(service);
    }
}
