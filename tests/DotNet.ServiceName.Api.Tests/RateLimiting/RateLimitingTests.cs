using DotNet.ServiceName.Api.Tests.Infrastructure;
using System.Net;
using System.Text.Json;
using Xunit;

namespace DotNet.ServiceName.Api.Tests.RateLimiting;

/// <summary>
/// Verifies the global fixed-window limiter rejects bursts with ProblemDetails
/// while health check endpoints stay exempt for monitoring probes.
/// </summary>
public sealed class RateLimitingTests
{
    [Fact]
    public async Task RequestsOverLimit_Return429ProblemDetails()
    {
        await using var factory = new RateLimitedApiTestFactory();
        using var client = factory.CreateClient();

        for (var attempt = 0; attempt < 3; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/status");
            request.Headers.Add(ApiTestFactory.ApiKeyHeader, ApiTestFactory.ApiKey);
            using var response = await client.SendAsync(request);

            if (attempt < 2)
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                continue;
            }

            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.True(response.Headers.Contains("Retry-After"));

            using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(429, body.RootElement.GetProperty("status").GetInt32());
            Assert.Equal("Too Many Requests", body.RootElement.GetProperty("title").GetString());
        }
    }

    [Fact]
    public async Task HealthEndpoints_AreExemptFromRateLimiting()
    {
        await using var factory = new RateLimitedApiTestFactory();
        using var client = factory.CreateClient();

        // more requests than PermitLimit=2 - the health endpoint must stay reachable
        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var response = await client.GetAsync("/health/live");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}