using System.Linq;
using SieveFramework.Providers;

namespace SieveFramework.Predicates
{
    public interface IPredicate
    {
        IQueryable<TResource> Apply<TResource>(ModelProvider provider, IQueryable<TResource> query) where TResource : class;
    }
}