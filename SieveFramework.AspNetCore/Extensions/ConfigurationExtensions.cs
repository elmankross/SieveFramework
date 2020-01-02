using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SieveFramework.Attributes;
using SieveFramework.Providers;

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
            services.AddSingleton(_ => assemblies.Length > 0
                ? new SieveConfiguratorProvider(assemblies)
                : new SieveConfiguratorProvider());
            services.AddSingleton<ISieveProvider, SieveProvider>(s =>
            {
                var provider = new SieveProvider();
                var configurator = s.GetService<SieveConfiguratorProvider>();
                configurator.Configure(provider);
                return provider;
            });
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
            services.AddSingleton<ISieveProvider, SieveProvider>(_ =>
            {
                var provider = new SieveProvider();
                config(provider);
                return provider;
            });
            return services;
        }
    }
}
