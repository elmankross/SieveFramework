using Microsoft.Extensions.DependencyInjection;
using SieveFramework.Attributes;
using SieveFramework.Providers;
using System;
using System.Reflection;
using SieveFramework.AspNetCore.Parsers;

namespace SieveFramework.AspNetCore.Extensions
{
    public class SieveConfiguration
    {
        internal Assembly[] Assemblies { get; private set; }
        internal IParser Parser { get; private set; }
        internal Action<SieveProvider> Configurator { get; private set; }

        internal SieveConfiguration()
        {
            Parser = new NativeQueryParser();
            Assemblies = new Assembly[0];
        }


        public SieveConfiguration ConfigureProvider(Action<SieveProvider> configurator)
        {
            Configurator = configurator;
            return this;
        }


        public SieveConfiguration WithParser<TParser>()
            where TParser : class, IParser, new()
        {
            Parser = new TParser();
            return this;
        }


        public SieveConfiguration ForAssemblies(params Assembly[] assemblies)
        {
            Assemblies = assemblies;
            return this;
        }
    }


    public static class ConfigurationExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="invoker">What assemblies contain Entity/View models marked with <see cref="CanFilterAttribute"/> or
        /// <see cref="CanSortAttribute"/></param>
        /// <returns></returns>
        public static IServiceCollection AddSieveProvider(this IServiceCollection services, Action<SieveConfiguration> invoker = null)
        {
            var config = new SieveConfiguration();
            invoker?.Invoke(config);

            var configurator = config.Assemblies.Length > 0
                ? new SieveConfiguratorProvider(config.Assemblies)
                : new SieveConfiguratorProvider();

            var provider = new SieveProvider();
            configurator.Configure(provider);
            config.Configurator?.Invoke(provider);

            services.AddSingleton<ISieveProvider>(provider);
            services.AddSingleton<IParser>(config.Parser);

            return services;
        }
    }
}
