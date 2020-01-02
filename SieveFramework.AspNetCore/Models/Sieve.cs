using Microsoft.AspNetCore.Mvc;
using SieveFramework.Models;
using SieveFramework.Predicates;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SieveFramework.AspNetCore.Models
{
    [ModelBinder(BinderType = typeof(SieveModelBinder))]
    public class Sieve<TResource> : IReadOnlyList<IPredicate<TResource>>
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

        private IPredicate<TResource>[] Predicates => new IPredicate<TResource>[]
        {
            new FilterPredicate<TResource>(Filter),
            new SortPredicate<TResource>(Sort.ToArray()),
            new SkipPredicate<TResource>(Skip),
            new TakePredicate<TResource>(Take)
        };

        public IEnumerator<IPredicate<TResource>> GetEnumerator()
        {
            return Predicates.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => Predicates.Count();
        public IPredicate<TResource> this[int index] => Predicates[index];
    }
}
