using System.Collections.Generic;
using SieveFramework.Predicates;

namespace SieveFramework.Models
{
    public class Sieve<TResource>
        where TResource : class
    {
        public List<IFilter<TResource>> Filters { get; set; }
        public List<Sort> Sorts { get; set; }
        public int Take { get; set; } = 100;
        public int Skip { get; set; }


        public Sieve()
        {
            Filters = new List<IFilter<TResource>>(0);
            Sorts = new List<Sort>(0);
        }


        internal IPredicate[] GetPredicates()
        {
            return new IPredicate[]
            {
                new FilterPredicate(Filters.ToArray()),
                new SortPredicate(Sorts.ToArray()),
                new SkipPredicate(Skip),
                new TakePredicate(Take)
            };
        }
    }
}
