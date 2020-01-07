using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SieveFramework.AspNetCore.Parsers;
using SieveFramework.AspNetCore.Swashbuckle.Filters;
using SieveFramework.Providers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SieveFramework.AspNetCore.Swashbuckle.Extensions
{
    public static class SwaggerGenExtensions
    {
        public static SwaggerGenOptions AddSieveDescription(this IServiceCollection services, SwaggerGenOptions builder)
        {
            var provider = services.Where(s => s.ServiceType == typeof(ISieveProvider))
                                   .Select(s => s.ImplementationInstance)
                                   .Single();
            var parser = services.Where(s => s.ServiceType == typeof(IParser))
                                 .Select(s => s.ImplementationInstance)
                                 .Single();
            builder.OperationFilter<SieveOperationFilter>(provider, parser);
            return builder;
        }
    }
}
