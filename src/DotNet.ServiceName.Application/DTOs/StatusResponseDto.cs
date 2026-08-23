using DotNet.ServiceName.Application.Models;
using Facet;

namespace DotNet.ServiceName.Application.DTOs;

/// <summary>
/// Application info facet generated from <see cref="AppInfo"/>.
/// </summary>
[Facet(typeof(AppInfo))]
public sealed partial class AppInfoDto;

/// <summary>
/// Application status and details for each connected service to use to identify current configuration.
/// Facet generated from <see cref="StatusResponse"/> with nested mapping of the application info.
/// </summary>
[Facet(typeof(StatusResponse), NestedFacets = [typeof(AppInfoDto)])]
public sealed partial class StatusResponseDto;