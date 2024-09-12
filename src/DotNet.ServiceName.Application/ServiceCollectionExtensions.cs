using DotNet.ServiceName.Application.Services;
using DotNet.ServiceName.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DotNet.ServiceName.Application;

/// <summary>
/// Dependency registrator for Application stuff.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register application level dependencies and services.
    /// </summary>
    /// <param name="services">Services collection.</param>
    /// <returns>Returns updated services collection.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // register mapping profiles for AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // register application services
        services.AddTransient<IApplicationStatusService, ApplicationStatusService>();

        return services;
    }
}