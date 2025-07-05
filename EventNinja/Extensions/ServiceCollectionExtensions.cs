using System;
using Microsoft.Extensions.DependencyInjection;
using EventNinja.Configuration;
using EventNinja.Services;

namespace EventNinja.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add EventNinja logger service to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configure">Configuration action</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddEventNinja(this IServiceCollection services, 
            Action<LoggerConfiguration> configure)
        {
            var config = new LoggerConfiguration();
            configure(config);
            
            services.AddSingleton<ILoggerService>(provider => new LoggerService(config));
            return services;
        }

        /// <summary>
        /// Add EventNinja logger service with default configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="appName">Application name for log files</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddEventNinja(this IServiceCollection services, 
            string appName = "Application")
        {
            return services.AddEventNinja(config => 
            {
                config.AppName = appName;
            });
        }
    }
}