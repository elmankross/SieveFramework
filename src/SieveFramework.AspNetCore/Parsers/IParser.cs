using SieveFramework.AspNetCore.Models;
using SieveFramework.Models;
using System.Collections.Generic;

namespace SieveFramework.AspNetCore.Parsers
{
    public interface IParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        ParseResult<Sieve<TResource>> TryParse<TResource>(string query) where TResource : class;

        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, SimpleFilterOperation> SimpleOperationsMapping { get; }

        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, ComplexFilterOperation> ComplexOperationsMapping { get; }

        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, SortDirection> SortDirectionsMapping { get; }
    }
}