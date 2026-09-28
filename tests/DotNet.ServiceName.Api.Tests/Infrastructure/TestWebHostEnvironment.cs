using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace DotNet.ServiceName.Api.Tests.Infrastructure;

/// <summary>
/// Minimal <see cref="IWebHostEnvironment"/> for the tests that exercise a page model without
/// booting the whole application.
/// </summary>
public sealed class TestWebHostEnvironment : IWebHostEnvironment
{
    /// <inheritdoc />
    public string EnvironmentName { get; set; } = "Local";

    /// <inheritdoc />
    public string ApplicationName { get; set; } = "DotNet.ServiceName.Api";

    /// <inheritdoc />
    public string WebRootPath { get; set; } = AppContext.BaseDirectory;

    /// <inheritdoc />
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

    /// <inheritdoc />
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

    /// <inheritdoc />
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}