using System;

namespace DotNet.ServiceName.Application.Models;

/// <summary>
/// Application status and details for each connected service to use to identify current configuration.
/// </summary>
public sealed class StatusResponse
{
    /// <summary>
    /// Datetime when status response created (request processing time).
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Application information.
    /// </summary>
    public AppInfo? AppInfo { get; set; }
}

/// <summary>
/// Application info.
/// </summary>
public sealed class AppInfo
{
    /// <summary>
    /// Machine name where the application is running.
    /// </summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// Environment name.
    /// </summary>
    public string? EnvironmentName { get; set; }

    /// <summary>
    /// Hosting .NET environment name.
    /// </summary>
    public required string HostingEnvironmentName { get; set; }

    /// <summary>
    /// Release date.
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// Application start time.
    /// </summary>
    public DateTime AppStartTime { get; set; }

    /// <summary>
    /// Application version.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Runtime version of the .NET.
    /// </summary>
    public string? Runtime { get; set; }
}