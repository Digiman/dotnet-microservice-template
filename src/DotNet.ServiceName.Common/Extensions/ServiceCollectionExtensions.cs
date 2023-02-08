using DotNet.ServiceName.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNet.ServiceName.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure application custom configuration.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <param name="configuration">Application configuration.</param>
        public static void AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<HealthCheckOptions>()
                .Bind(configuration.GetSection(nameof(HealthCheckOptions)))
                .ValidateDataAnnotations()
                .ValidateOnStart();
            
            services.AddOptions<MemoryCheckOptions>(nameof(MemoryCheckOptions))
                .Bind(configuration.GetSection(nameof(MemoryCheckOptions)))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }
    }
}