using DotNet.ServiceName.Api.Tests.Infrastructure;
using Xunit;

namespace DotNet.ServiceName.Api.Tests;

/// <summary>
/// Collection definition sharing a single application instance between endpoint test classes.
/// </summary>
[CollectionDefinition("Api")]
public sealed class ApiCollection : ICollectionFixture<ApiTestFactory>
{
}