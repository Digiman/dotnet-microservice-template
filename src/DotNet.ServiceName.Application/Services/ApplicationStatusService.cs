using System;
using System.Reflection;
using System.Threading.Tasks;
using DotNet.ServiceName.Application.Models;
using DotNet.ServiceName.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotNet.ServiceName.Application.Services
{
    /// <summary>
    /// Simple service to identify application status to get the main information like configuration and etc.
    /// </summary>
    public sealed class ApplicationStatusService : IApplicationStatusService
    {
        private readonly ILogger<ApplicationStatusService> _logger;
        private readonly IConfiguration _configuration;

        public ApplicationStatusService(ILogger<ApplicationStatusService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <inheritdoc />
        public Task<StatusResponse> GetApplicationStatusAsync()
        {
            try
            {
                var response = new StatusResponse
                {
                    Created = DateTime.UtcNow,
                    AppInfo = CollectApplicationInfo(),
                    AmazonInfo = CollectAmazonInfo()
                };

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during identifying application status!");

                throw;
            }
        }

        #region Helpers.

        /// <summary>
        /// Collect application information.
        /// </summary>
        /// <returns>Returns application info.</returns>
        private AppInfo CollectApplicationInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();

            return new AppInfo
            {
                MachineName = Environment.MachineName,
                EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                ReleaseDate = System.IO.File.GetLastWriteTime(assembly.Location).ToUniversalTime(),
                AppStartTime = System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime(),
                Version = Assembly.GetEntryAssembly().GetName().Version.ToString()
            };
        }

        /// <summary>
        /// Collect AWS related information about service instance.
        /// </summary>
        /// <returns>Returns Amazon AWS info.</returns>
        private AmazonInfo CollectAmazonInfo()
        {
            if (!string.IsNullOrEmpty(Amazon.Util.EC2InstanceMetadata.InstanceId))
            {
                return new AmazonInfo
                {
                    Ec2InstanceId = Amazon.Util.EC2InstanceMetadata.InstanceId,
                    Ec2AmiId = Amazon.Util.EC2InstanceMetadata.AmiId,
                    InstanceType = Amazon.Util.EC2InstanceMetadata.InstanceType,
                    Region = Amazon.Util.EC2InstanceMetadata.Region.DisplayName,
                    AvailabilityZone = Amazon.Util.EC2InstanceMetadata.AvailabilityZone
                };
            }

            return null;
        }

        #endregion
    }
}