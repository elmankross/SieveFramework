using SieveFramework.Exceptions;
using SieveFramework.Models;
using System.Collections.Generic;
using System.Linq;

namespace SieveFramework.Providers
{
    /// <summary>
    /// 
    /// </summary>
    internal class QueryAliases
    {
        internal const string NODE_DELIMITER = "~";
        internal const string OPERATION_DELIMITER = "&";
        internal const string FILTER = "filter=";
        internal const string SORT = "sort=";
        internal const string TAKE = "take=";
        internal const string SKIP = "skip=";

        internal const string AND = "~and~";
        internal const string OR = "~or~";

        internal const string ASC = "asc";
        internal const string DESC = "desc";

        internal const string EQUAL = "eq";
        internal const string NOT_EQUAL = "neq";
        internal const string GREATER_THAN = "gt";
        internal const string GREATER_THAN_OR_EQUAL = "gte";
        internal const string LESS_THAN = "lt";
        internal const string LESS_THAN_OR_EQUAL = "lte";


        internal static Dictionary<string, SimpleFilterOperation> OperationMappings = new Dictionary<string, SimpleFilterOperation>
        {
            [EQUAL] = SimpleFilterOperation.Equal,
            [NOT_EQUAL] = SimpleFilterOperation.NotEqual,
            [GREATER_THAN] = SimpleFilterOperation.Greater,
            [GREATER_THAN_OR_EQUAL] = SimpleFilterOperation.GreaterOrEqual,
            [LESS_THAN] = SimpleFilterOperation.Less,
            [LESS_THAN_OR_EQUAL] = SimpleFilterOperation.LessOrEqual
        };

        internal static Dictionary<string, SortDirection> SortDirectionMappings = new Dictionary<string, SortDirection>
        {
            [ASC] = SortDirection.Ascending,
            [DESC] = SortDirection.Descending
        };
    }


    /// <summary>
    /// 
    /// </summary>
    public class QueryProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public Sieve<TResource> Parse<TResource>(string query)
            where TResource : class
        {
            var model = new Sieve<TResource>();
            foreach (var node in query.Split(QueryAliases.OPERATION_DELIMITER))
            {
                if (node.StartsWith(QueryAliases.FILTER))
                {
                    model.Filter = ParseFilter<TResource>(node.Substring(QueryAliases.FILTER.Length));
                }
                else if (node.StartsWith(QueryAliases.SORT))
                {
                    model.Sort = ParseSort<TResource>(node.Substring(QueryAliases.SORT.Length));
                }
                else if (node.StartsWith(QueryAliases.SKIP))
                {
                    model.Skip = ParseConst(node.Substring(QueryAliases.SKIP.Length)) ?? model.Skip;
                }
                else if (node.StartsWith(QueryAliases.TAKE))
                {
                    model.Take = ParseConst(node.Substring(QueryAliases.TAKE.Length)) ?? model.Take;
                }
                else
                {
                    throw new InvalidFilterFormatException("Invalid query alias");
                }
            }

            return model;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        private IFilterPipeline<TResource> ParseFilter<TResource>(string node)
            where TResource : class
        {
            if (string.IsNullOrEmpty(node))
            {
                return null;
            }

            var complex = node.Split(QueryAliases.OR);
            if (complex.Length > 1)
            {
                var subset = complex.Select(ParseFilter<TResource>).ToArray();
                return new ComplexFilterPipeline<TResource>(ComplexFilterOperation.Or, subset);
            }

            var simple = node.Split(QueryAliases.AND);
            if (simple.Length > 1)
            {
                var subset = simple.Select(ParseFilter<TResource>).ToArray();
                return new ComplexFilterPipeline<TResource>(ComplexFilterOperation.And, subset);
            }

            var values = node.Split(QueryAliases.NODE_DELIMITER);
            if (values.Length == 3)
            {
                var property = values[0];
                var operation = values[1];
                var value = values[2];
                if (!QueryAliases.OperationMappings.ContainsKey(operation))
                {
                    throw new InvalidFilterFormatException("Not supported filter's operation", node);
                }
                return new SimpleFilterPipeline<TResource>(property, QueryAliases.OperationMappings[operation], value);
            }

            throw new InvalidFilterFormatException("Invalid filter format", node);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        private List<ISortPipeline<TResource>> ParseSort<TResource>(string node)
            where TResource : class
        {
            if (string.IsNullOrEmpty(node))
            {
                return new List<ISortPipeline<TResource>>(0);
            }

            return node.Split(QueryAliases.AND).Select(n =>
            {
                var values = n.Split(QueryAliases.NODE_DELIMITER);
                if (values.Length == 2)
                {
                    var property = values[0];
                    var direction = values[1];
                    if (QueryAliases.SortDirectionMappings.ContainsKey(direction))
                    {
                        return new SortPipeline<TResource>(property, QueryAliases.SortDirectionMappings[direction]);
                    }

                    throw new InvalidFilterFormatException("Invalid sorting direction", n);
                }
                throw new InvalidFilterFormatException("Invalid sorting format", n);
            }).OfType<ISortPipeline<TResource>>().ToList();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private int? ParseConst(string node)
        {
            if (string.IsNullOrEmpty(node))
            {
                return null;
            }

            if (int.TryParse(node, out var result))
            {
                return result;
            }
            throw new InvalidFilterFormatException("Constant value must be a valid number", node);
        }
    }
}
