using System.Collections.Generic;
using SieveFramework.Predicates;

namespace SieveFramework.Models
{
    public class Sieve<TResource>
        where TResource : class
    {
        public IFilterPipeline<TResource> Filter { get; set; }
        public List<ISortPipeline<TResource>> Sort { get; set; }
        public int Take { get; set; } = 100;
        public int Skip { get; set; }


        public Sieve()
        {
            Sort = new List<ISortPipeline<TResource>>(0);
        }


        internal IPredicate<TResource>[] GetPredicates()
        {
            return new IPredicate<TResource>[]
            {
                new FilterPredicate<TResource>(Filter),
                new SortPredicate<TResource>(Sort.ToArray()),
                new SkipPredicate<TResource>(Skip),
                new TakePredicate<TResource>(Take)
            };
        }
    }
}
