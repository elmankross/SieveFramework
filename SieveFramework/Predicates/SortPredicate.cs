using System;
using System.Linq;
using System.Linq.Expressions;
using SieveFramework.Models;
using SieveFramework.Providers;

namespace SieveFramework.Predicates
{
    public class SortPredicate<TResource> : IPredicate<TResource>
        where TResource : class
    {
        private readonly ISortPipeline<TResource>[] _sorts;

        public SortPredicate(params ISortPipeline<TResource>[] sorts)
        {
            _sorts = sorts;
        }


        public IQueryable<TResource> Apply(ModelProvider provider, IQueryable<TResource> query)
        {
            foreach (var sort in _sorts)
            {
                if (provider.TrySort(sort.Property, out var property))
                {
                    var lambda = sort.Apply(provider.Target, p => null);
                    var method = sort.Direction == SortDirection.Ascending
                        ? nameof(Queryable.OrderBy)
                        : nameof(Queryable.OrderByDescending);
                    var exp = Expression.Call(typeof(Queryable), method, new[] { provider.Target.Type, property.PropertyType },
                        query.Expression, lambda);
                    query = query.Provider.CreateQuery<TResource>(exp);
                }
                else
                {
                    throw new ArgumentException("Cannot sort selected property");
                }
            }

            return query;
        }
    }
}
