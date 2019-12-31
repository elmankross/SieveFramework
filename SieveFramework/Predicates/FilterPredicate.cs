using System;
using System.Linq;
using System.Linq.Expressions;
using SieveFramework.Models;
using SieveFramework.Providers;

namespace SieveFramework.Predicates
{
    public class FilterPredicate : IPredicate
    {
        private readonly IFilter[] _filters;

        public FilterPredicate(params IFilter[] filters)
        {
            _filters = filters;
        }


        /// <inheritdoc />
        public IQueryable<TResource> Apply<TResource>(ModelProvider provider, IQueryable<TResource> query)
            where TResource : class
        {
            foreach (var filter in _filters)
            {
                var expression = filter.Apply(provider.Target, property =>
                    provider.TryFilter(property, out var info)
                        ? info
                        : throw new ArgumentException("Cannot filter selected property"));
                // complex filter with nothing
                if (expression is DefaultExpression)
                {
                    continue;
                }
                var lambda = Expression.Lambda<Func<TResource, bool>>(expression, provider.Target);
                query = query.Where(lambda);
            }
            return query;
        }
    }
}
