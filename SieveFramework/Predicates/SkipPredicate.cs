using System.Linq;
using SieveFramework.Providers;

namespace SieveFramework.Predicates
{
    public class SkipPredicate : IPredicate
    {
        private readonly int _skip;

        public SkipPredicate(int skip)
        {
            _skip = skip;
        }

        public IQueryable<TResource> Apply<TResource>(ModelProvider _, IQueryable<TResource> query) where TResource : class
        {
            return query.Skip(_skip);
        }
    }
}
