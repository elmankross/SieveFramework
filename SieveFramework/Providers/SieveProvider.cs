using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SieveFramework.Models;
using SieveFramework.Predicates;

namespace SieveFramework.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISieveProvider
    {
        /// <summary>
        /// Apply filter to resource
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="query"></param>
        /// <param name="sieve"></param>
        /// <returns></returns>
        IQueryable<TResource> Apply<TResource>(IQueryable<TResource> query, Sieve<TResource> sieve)
            where TResource : class;
    }


    /// <summary>
    /// 
    /// </summary>
    public class SieveProvider : ISieveProvider
    {
        internal ConcurrentDictionary<string, ModelProvider> Providers { get; }

        public SieveProvider()
        {
            Providers = new ConcurrentDictionary<string, ModelProvider>();
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
        /// <param name="sieve"></param>
        /// <returns></returns>
        public IQueryable<TResource> Apply<TResource>(IQueryable<TResource> query, Sieve<TResource> sieve)
            where TResource : class
        {
            return Apply(query, sieve.GetPredicates());
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
                throw new Exception("Can't add new provider. It seems this provider already added");
            }

            return this;
        }
    }
}