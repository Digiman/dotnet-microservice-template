using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace DotNet.ServiceName.Common.Configuration;

/// <summary>
/// Configuration for the home page of the service. The page is a Razor Page, so its links are
/// built per environment instead of being hardcoded in the markup.
/// </summary>
public sealed class HomePageOptions
{
    /// <summary>
    /// Serve the home page on the root of the service.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Title (heading) of the home page.
    /// </summary>
    [Required]
    public required string Title { get; set; }

    /// <summary>
    /// Short description shown under the title.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Show the name of the current environment as a badge - handy to see which deployment the
    /// home page belongs to.
    /// </summary>
    public bool ShowEnvironment { get; set; } = true;

    /// <summary>
    /// Show the links to the API documentation UIs (Swagger, Scalar, Facet Dashboard). The links
    /// are rendered only when the documentation itself is enabled for the environment.
    /// </summary>
    public bool ShowDocumentation { get; set; } = true;

    /// <summary>
    /// Show the links to the health check endpoints and, when it is enabled, to the dashboard.
    /// </summary>
    public bool ShowHealthChecks { get; set; } = true;

    /// <summary>
    /// Theme the home page starts with. The visitor can still switch between the system theme,
    /// light and dark - the choice is stored in the browser, not in the service configuration.
    /// </summary>
    public HomePageTheme DefaultTheme { get; set; } = HomePageTheme.System;

    /// <summary>
    /// Additional links rendered on the page - use them for anything the service exposes that is
    /// not derived from the configuration above (a company portal, a runbook, a mailbox, ...).
    /// </summary>
    public List<HomePageLinkOptions> Links { get; set; } = [];
}

/// <summary>
/// Theme the home page is rendered with.
/// </summary>
public enum HomePageTheme
{
    /// <summary>
    /// Follow the theme of the operating system or the browser.
    /// </summary>
    System,

    /// <summary>
    /// Always the light theme.
    /// </summary>
    Light,

    /// <summary>
    /// Always the dark theme.
    /// </summary>
    Dark
}

/// <summary>
/// Configuration for a single custom link on the home page.
/// </summary>
public sealed class HomePageLinkOptions
{
    /// <summary>
    /// Text of the link.
    /// </summary>
    [Required]
    public required string Title { get; set; }

    /// <summary>
    /// Target of the link - can be relative to the service root or absolute.
    /// </summary>
    [Required]
    public required string Url { get; set; }

    /// <summary>
    /// Optional text shown under the title of the link.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional image shown in the link - a path in wwwroot or an absolute url.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Render the link. Disabled links are kept in configuration but skipped on the page.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Open the link in a new browser tab.
    /// </summary>
    public bool OpenInNewTab { get; set; }
}

/// <summary>
/// Custom validator for HomePageOptions with FluentValidator.
/// </summary>
public sealed class HomePageOptionsValidator : AbstractValidator<HomePageOptions>
{
    public HomePageOptionsValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
        RuleForEach(x => x.Links).ChildRules(link =>
        {
            link.RuleFor(x => x.Title).NotEmpty();
            link.RuleFor(x => x.Url).NotEmpty();
        });
    }
}