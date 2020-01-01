using System.Linq;
using SieveFramework.Providers;

namespace SieveFramework.Predicates
{
    public class SkipPredicate<TResource> : IPredicate<TResource>
        where TResource : class
    {
        private readonly int _skip;

        public SkipPredicate(int skip)
        {
            _skip = skip;
        }

        public IQueryable<TResource> Apply(ModelProvider _, IQueryable<TResource> query)
        {
            return query.Skip(_skip);
        }
    }
}
