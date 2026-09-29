using DotNet.ServiceName.Common.Configuration;

namespace DotNet.ServiceName.Api.Infrastructure.Models;

/// <summary>
/// Content of the home page, built per environment from the configuration.
/// </summary>
public sealed class HomePageModel
{
    /// <summary>
    /// Title (heading) of the page.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Short description shown under the title.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Name of the current environment, shown when <see cref="ShowEnvironment"/> is set.
    /// </summary>
    public string? Environment { get; init; }

    /// <summary>
    /// Show the environment badge.
    /// </summary>
    public bool ShowEnvironment { get; init; }

    /// <summary>
    /// Links available in the current environment.
    /// </summary>
    public required IReadOnlyList<HomePageLink> Links { get; init; }

    /// <summary>
    /// The documentation UIs are expected but turned off in this environment - the page says so
    /// instead of silently showing fewer links.
    /// </summary>
    public bool DocumentationDisabled { get; init; }

    /// <summary>
    /// Theme the page starts with, before the visitor picks one.
    /// </summary>
    public HomePageTheme DefaultTheme { get; init; }
}

/// <summary>
/// Single link (button) on the home page.
/// </summary>
/// <param name="Title">Text of the link.</param>
/// <param name="Url">Target of the link.</param>
/// <param name="Description">Optional text shown under the title.</param>
/// <param name="Icon">Optional image shown in the link.</param>
/// <param name="OpenInNewTab">Open the link in a new browser tab.</param>
public sealed record HomePageLink(string Title, string Url, string? Description = null, string? Icon = null,
    bool OpenInNewTab = false);