using System.Linq;
using SieveFramework.Providers;

namespace SieveFramework.Predicates
{
    public interface IPredicate<TResource>
        where TResource : class
    {
        IQueryable<TResource> Apply(ModelProvider provider, IQueryable<TResource> query);
    }
}