using DotNet.ServiceName.Api.Tests.Infrastructure;
using DotNet.ServiceName.Common.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Xunit;

namespace DotNet.ServiceName.Api.Tests.Telemetry;

/// <summary>
/// The OpenTelemetry providers are registered and their options are bound from configuration.
/// </summary>
[Collection("Api")]
public sealed class TelemetryTests
{
    private readonly ApiTestFactory _factory;

    public TelemetryTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public void TelemetryProviders_AreRegistered()
    {
        Assert.NotNull(_factory.Services.GetService<TracerProvider>());
        Assert.NotNull(_factory.Services.GetService<MeterProvider>());
    }

    [Fact]
    public void TelemetryOptions_AreBoundFromConfiguration()
    {
        var options = _factory.Services.GetRequiredService<IOptions<TelemetryOptions>>().Value;

        Assert.True(options.Enabled);
        Assert.Equal("dotnet-servicename", options.ServiceName);
        Assert.False(options.ConsoleExporter);
    }
}