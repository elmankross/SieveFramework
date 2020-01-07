using SieveFramework.Predicates;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SieveFramework.AspNetCore", AllInternalsVisible = true)]
[assembly: InternalsVisibleTo("SieveFrameworkTests", AllInternalsVisible = true)]
namespace SieveFramework.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISieveProvider : IEnumerable<KeyValuePair<string, ModelProvider>>
    {
        /// <summary>
        /// Apply filter to resource
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicates"></param>
        /// <returns></returns>
        IQueryable<TResource> Apply<TResource>(IQueryable<TResource> query, IReadOnlyList<IPredicate<TResource>> predicates)
            where TResource : class;
    }


    /// <summary>
    /// 
    /// </summary>
    public class SieveProvider : ISieveProvider
    {
        internal ConcurrentDictionary<string, ModelProvider> Providers { get; }

        internal SieveProvider()
        {
            Providers = new ConcurrentDictionary<string, ModelProvider>();
        }


        public IEnumerator<KeyValuePair<string, ModelProvider>> GetEnumerator()
        {
            return Providers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public SieveProvider AddModel<TModel>(Action<ModelProvider<TModel>> builder)
            where TModel : class
        {
            var provider = new ModelProvider<TModel>();
            builder(provider);
            return AddProvider(provider);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public SieveProvider AddModel(Type type, Action<ModelProvider> builder)
        {
            var provider = new ModelProvider(type);
            builder(provider);
            return AddProvider(provider);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="query"></param>
        /// <param name="predicates"></param>
        /// <returns></returns>
        public IQueryable<TResource> Apply<TResource>(IQueryable<TResource> query, IReadOnlyList<IPredicate<TResource>> predicates)
            where TResource : class
        {
            if (predicates == null)
            {
                return query;
            }

            if (!Providers.TryGetValue(typeof(TResource).Name, out var provider))
            {
                throw new ArgumentException("Cannot filter selected resource. Provider was not registered");
            }

            foreach (var predicate in predicates)
            {
                query = predicate.Apply(provider, query);
            }

            return query;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        private SieveProvider AddProvider(ModelProvider provider)
        {
            if (!Providers.TryAdd(provider.Target.Type.Name, provider))
            {
                throw new Exception("Can't add new provider. It seems this provider already exists. "
                                  + "It may be previous registration through model's attributes.");
            }

            return this;
        }
    }
}