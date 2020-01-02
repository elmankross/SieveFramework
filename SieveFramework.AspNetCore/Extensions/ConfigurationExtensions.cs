using Microsoft.Extensions.DependencyInjection;
using SieveFramework.Attributes;
using SieveFramework.Providers;
using System;
using System.Reflection;

namespace SieveFramework.AspNetCore.Extensions
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies">What assemblies contain Entity/View models marked with <see cref="CanFilterAttribute"/> or
        /// <see cref="CanSortAttribute"/></param>
        /// <returns></returns>
        public static IServiceCollection AddSieveProvider(this IServiceCollection services, params Assembly[] assemblies)
        {
            var configurator = assemblies.Length > 0
                ? new SieveConfiguratorProvider(assemblies)
                : new SieveConfiguratorProvider();
            var provider = new SieveProvider();
            configurator.Configure(provider);
            services.AddSingleton<ISieveProvider>(provider);
            return services;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection AddSieveProvider(this IServiceCollection services, Action<SieveProvider> config)
        {
            var provider = new SieveProvider();
            config(provider);
            services.AddSingleton<ISieveProvider>(provider);
            return services;
        }
    }
}
