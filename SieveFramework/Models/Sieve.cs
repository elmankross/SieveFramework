using System.Collections.Generic;
using SieveFramework.Predicates;

namespace SieveFramework.Models
{
    public class Sieve<TResource>
        where TResource : class
    {
        public List<IFilterPipeline<TResource>> Filters { get; set; }
        public List<ISortPipeline<TResource>> Sorts { get; set; }
        public int Take { get; set; } = 100;
        public int Skip { get; set; }


        public Sieve()
        {
            Filters = new List<IFilterPipeline<TResource>>(0);
            Sorts = new List<ISortPipeline<TResource>>(0);
        }


        internal IPredicate<TResource>[] GetPredicates()
        {
            return new IPredicate<TResource>[]
            {
                new FilterPredicate<TResource>(Filters.ToArray()),
                new SortPredicate<TResource>(Sorts.ToArray()),
                new SkipPredicate<TResource>(Skip),
                new TakePredicate<TResource>(Take)
            };
        }
    }
}
