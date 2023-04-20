using DotNet.ServiceName.Common.Configuration;
using Microsoft.Extensions.Configuration;

namespace DotNet.ServiceName.Common.Extensions
{
    /// <summary>
    /// Simple extensions for configuration.
    /// </summary>
    public static class ConfigurationExtensions
    {
        public static HealthCheckOptions GetHealthCheckConfiguration(this IConfiguration configuration)
        {
            return configuration.GetSection(nameof(HealthCheckOptions)).Get<HealthCheckOptions>();
        }

        public static MemoryCheckOptions GetMemoryCheckConfiguration(this IConfiguration configuration)
        {
            return configuration.GetSection(nameof(MemoryCheckOptions)).Get<MemoryCheckOptions>();
        }
    }
}