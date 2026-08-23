using DotNet.ServiceName.Application.Services;
using DotNet.ServiceName.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DotNet.ServiceName.Application.Tests.Services;

/// <summary>
/// Tests for the <see cref="ApplicationStatusService"/>.
/// </summary>
public sealed class ApplicationStatusServiceTests
{
    private readonly ILogger<ApplicationStatusService> _logger = Substitute.For<ILogger<ApplicationStatusService>>();

    private ApplicationStatusService CreateService(IConfiguration? configuration = null)
    {
        return new ApplicationStatusService(_logger, configuration ?? new ConfigurationBuilder().Build());
    }

    [Fact]
    public async Task GetApplicationStatusAsync_ReturnsStatusResponse()
    {
        var service = CreateService();

        var result = await service.GetApplicationStatusAsync();

        Assert.NotNull(result);
        Assert.NotNull(result.AppInfo);
    }

    [Fact]
    public async Task GetApplicationStatusAsync_SetsCreatedAtCloseToUtcNow()
    {
        var service = CreateService();
        var beforeCall = DateTime.UtcNow;

        var result = await service.GetApplicationStatusAsync();

        var afterCall = DateTime.UtcNow;
        Assert.InRange(result.Created, beforeCall, afterCall);
        Assert.Equal(DateTimeKind.Utc, result.Created.Kind);
    }

    [Fact]
    public async Task GetApplicationStatusAsync_FillsMachineName_FromCurrentEnvironment()
    {
        var service = CreateService();

        var result = await service.GetApplicationStatusAsync();

        Assert.NotNull(result.AppInfo);
        Assert.Equal(Environment.MachineName, result.AppInfo.MachineName);
    }

    [Fact]
    public async Task GetApplicationStatusAsync_FillsEnvironmentNames_FromEnvironment()
    {
        var service = CreateService();

        var result = await service.GetApplicationStatusAsync();

        // EnvironmentName is null when no environment variable is configured
        var expectedEnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (string.IsNullOrWhiteSpace(expectedEnvironmentName))
        {
            expectedEnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        }

        Assert.NotNull(result.AppInfo);
        Assert.Equal(expectedEnvironmentName, result.AppInfo.EnvironmentName);
        Assert.False(string.IsNullOrWhiteSpace(result.AppInfo.HostingEnvironmentName));
    }

    [Fact]
    public async Task GetApplicationStatusAsync_FillsReleaseDateAndStartTime_InUtc()
    {
        var service = CreateService();

        var result = await service.GetApplicationStatusAsync();

        Assert.NotNull(result.AppInfo);
        Assert.NotEqual(default, result.AppInfo.ReleaseDate);
        Assert.NotEqual(default, result.AppInfo.AppStartTime);
        Assert.InRange(result.AppInfo.ReleaseDate, DateTime.MinValue, DateTime.UtcNow);
        Assert.InRange(result.AppInfo.AppStartTime, DateTime.MinValue, DateTime.UtcNow);
    }

    [Fact]
    public async Task GetApplicationStatusAsync_FillsVersionAndRuntime()
    {
        var service = CreateService();

        var result = await service.GetApplicationStatusAsync();

        Assert.NotNull(result.AppInfo);
        Assert.False(string.IsNullOrWhiteSpace(result.AppInfo.Version));
        Assert.False(string.IsNullOrWhiteSpace(result.AppInfo.Runtime));
        Assert.Contains(".NET", result.AppInfo.Runtime);
    }
}
