using System.Linq;
using SieveFramework.Providers;

namespace SieveFramework.Predicates
{
    public class TakePredicate<TResource> : IPredicate<TResource>
        where TResource : class
    {
        private readonly int _take;

        public TakePredicate(int take)
        {
            _take = take;
        }

        public IQueryable<TResource> Apply(ModelProvider _, IQueryable<TResource> query)
        {
            return query.Take(_take);
        }
    }
}