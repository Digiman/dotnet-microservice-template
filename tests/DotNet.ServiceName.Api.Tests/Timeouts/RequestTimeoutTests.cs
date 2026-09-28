using DotNet.ServiceName.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace DotNet.ServiceName.Api.Tests.Timeouts;

/// <summary>
/// The default request timeout is bound from configuration and applied globally.
/// </summary>
[Collection("Api")]
public sealed class RequestTimeoutTests
{
    private readonly ApiTestFactory _factory;

    public RequestTimeoutTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public void DefaultTimeout_IsBoundFromConfiguration()
    {
        var options = _factory.Services.GetRequiredService<IOptions<RequestTimeoutOptions>>().Value;

        Assert.NotNull(options.DefaultPolicy);
        Assert.Equal(TimeSpan.FromSeconds(30), options.DefaultPolicy.Timeout);
    }
}