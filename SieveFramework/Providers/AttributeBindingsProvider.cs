using System;
using System.Linq;
using System.Reflection;
using SieveFramework.Attributes;

namespace SieveFramework.Providers
{
    public class AttributeBindingsProvider
    {
        private readonly Assembly[] _assemblies;

        public AttributeBindingsProvider()
            : this(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.GlobalAssemblyCache).ToArray())
        {
        }

        public AttributeBindingsProvider(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sieveProvider"></param>
        /// <returns></returns>
        public void FillSieveProvider(SieveProvider sieveProvider)
        {
            var types = _assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && t.IsPublic))
                                   .Distinct()
                                   .ToArray();

            foreach (var type in types)
            {
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .Select(p => new
                                     {
                                         info = p,
                                         canSort = p.GetCustomAttribute<CanSortAttribute>() != null,
                                         canFilter = p.GetCustomAttribute<CanFilterAttribute>() != null
                                     }).Where(p => p.canFilter || p.canSort)
                                     .ToArray();

                if (properties.Length > 0)
                {
                    sieveProvider.AddModel(type, builder =>
                    {
                        foreach (var property in properties)
                        {
                            if (property.canFilter)
                            {
                                builder.CanFilter(property.info);
                            }

                            if (property.canSort)
                            {
                                builder.CanSort(property.info);
                            }
                        }
                    });
                }
            }
        }
    }
}
