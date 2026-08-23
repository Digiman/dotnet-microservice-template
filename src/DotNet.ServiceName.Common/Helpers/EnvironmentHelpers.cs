namespace DotNet.ServiceName.Common.Helpers;

/// <summary>
/// Simple helpers for environment.
/// </summary>
public static class EnvironmentHelpers
{
    /// <summary>
    /// Get environment name for .NET application.
    /// </summary>
    /// <returns>Returns environment name.</returns>
    public static string? GetEnvironmentName()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (string.IsNullOrWhiteSpace(environmentName))
        {
            environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        }

        return environmentName;
    }

    /// <summary>
    /// Get hosting environment name for .NET application.
    /// </summary>
    /// <returns>Returns environment name.</returns>
    public static string GetHostingEnvironmentName()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (string.IsNullOrWhiteSpace(environmentName))
        {
            environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        }

        if (string.IsNullOrWhiteSpace(environmentName))
        {
            environmentName = "Production";
        }

        return environmentName;
    }
}