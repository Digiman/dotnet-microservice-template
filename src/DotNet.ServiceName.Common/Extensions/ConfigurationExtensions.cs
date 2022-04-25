using DotNet.ServiceName.Common.Configuration;
using Microsoft.Extensions.Configuration;

namespace DotNet.ServiceName.Common.Extensions
{
    public static class ConfigurationExtensions
    {
        public static HealthCheckOptions GetHealthCheckConfiguration(this IConfiguration configuration)
        {
            return configuration.GetSection(nameof(HealthCheckOptions)).Get<HealthCheckOptions>();
        }
    }
}