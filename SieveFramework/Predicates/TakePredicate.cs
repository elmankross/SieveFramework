using System.Linq;
using SieveFramework.Providers;

namespace SieveFramework.Predicates
{
    public class TakePredicate : IPredicate
    {
        private readonly int _take;

        public TakePredicate(int take)
        {
            _take = take;
        }

        public IQueryable<TResource> Apply<TResource>(ModelProvider _, IQueryable<TResource> query) where TResource : class
        {
            return query.Take(_take);
        }
    }
}