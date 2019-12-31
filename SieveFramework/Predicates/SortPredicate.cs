using System;
using System.Linq;
using SieveFramework.Models;
using SieveFramework.Providers;

namespace SieveFramework.Predicates
{
    public class SortPredicate : IPredicate
    {
        private readonly Sort[] _sorts;

        public SortPredicate(params Sort[] sorts)
        {
            _sorts = sorts;
        }


        public IQueryable<TResource> Apply<TResource>(ModelProvider provider, IQueryable<TResource> query) where TResource : class
        {
            foreach (var sort in _sorts)
            {
                if (provider.TrySort(sort.Property, out var property))
                {
                    query = query.Provider.CreateQuery<TResource>(sort.Apply(provider.Target, query, property));
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
