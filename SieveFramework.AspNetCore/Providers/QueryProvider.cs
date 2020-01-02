using System;
using SieveFramework.AspNetCore.Models;
using SieveFramework.Models;
using System.Collections.Generic;
using System.Linq;

namespace SieveFramework.AspNetCore.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryAliases
    {
        public const string NODE_DELIMITER = "~";
        public const string OPERATION_DELIMITER = "&";
        public const string FILTER = "filter=";
        public const string SORT = "sort=";
        public const string TAKE = "take=";
        public const string SKIP = "skip=";

        public const string AND = "~and~";
        public const string OR = "~or~";

        public const string ASC = "asc";
        public const string DESC = "desc";

        public const string EQUAL = "eq";
        public const string NOT_EQUAL = "neq";
        public const string GREATER_THAN = "gt";
        public const string GREATER_THAN_OR_EQUAL = "gte";
        public const string LESS_THAN = "lt";
        public const string LESS_THAN_OR_EQUAL = "lte";


        public static Dictionary<string, SimpleFilterOperation> OperationMappings = new Dictionary<string, SimpleFilterOperation>
        {
            [EQUAL] = SimpleFilterOperation.Equal,
            [NOT_EQUAL] = SimpleFilterOperation.NotEqual,
            [GREATER_THAN] = SimpleFilterOperation.Greater,
            [GREATER_THAN_OR_EQUAL] = SimpleFilterOperation.GreaterOrEqual,
            [LESS_THAN] = SimpleFilterOperation.Less,
            [LESS_THAN_OR_EQUAL] = SimpleFilterOperation.LessOrEqual
        };

        public static Dictionary<string, SortDirection> SortDirectionMappings = new Dictionary<string, SortDirection>
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
        public ParseResult<Sieve<TResource>> TryParse<TResource>(string query)
            where TResource : class
        {
            var model = new Sieve<TResource>();
            foreach (var node in query.Split(QueryAliases.OPERATION_DELIMITER))
            {
                if (node.StartsWith(QueryAliases.FILTER, StringComparison.OrdinalIgnoreCase))
                {
                    var result = TryParseFilter<TResource>(node.Substring(QueryAliases.FILTER.Length));
                    if (result.Successful)
                    {
                        model.Filter = result.Result;
                    }
                    else
                    {
                        return new ParseResult<Sieve<TResource>>(result.Errors);
                    }
                }
                else if (node.StartsWith(QueryAliases.SORT, StringComparison.OrdinalIgnoreCase))
                {
                    var result = TryParseSort<TResource>(node.Substring(QueryAliases.SORT.Length));
                    if (result.Successful)
                    {
                        model.Sort = result.Result;
                    }
                    else
                    {
                        return new ParseResult<Sieve<TResource>>(result.Errors);
                    }
                }
                else if (node.StartsWith(QueryAliases.SKIP, StringComparison.OrdinalIgnoreCase))
                {
                    var result = TryParseConst(node.Substring(QueryAliases.SKIP.Length));
                    if (result.Successful)
                    {
                        model.Skip = result.Result ?? model.Skip;
                    }
                    else
                    {
                        return new ParseResult<Sieve<TResource>>(result.Errors);
                    }
                }
                else if (node.StartsWith(QueryAliases.TAKE, StringComparison.OrdinalIgnoreCase))
                {
                    var result = TryParseConst(node.Substring(QueryAliases.TAKE.Length));
                    if (result.Successful)
                    {
                        model.Take = result.Result ?? model.Take;
                    }
                    else
                    {
                        return new ParseResult<Sieve<TResource>>(result.Errors);
                    }
                }
            }

            return new ParseResult<Sieve<TResource>>(model);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        private ParseResult<IFilterPipeline<TResource>> TryParseFilter<TResource>(string node)
            where TResource : class
        {
            if (string.IsNullOrEmpty(node))
            {
                return new ParseResult<IFilterPipeline<TResource>>();
            }

            var complex = node.Split(QueryAliases.OR);
            if (complex.Length > 1)
            {
                var result = complex.Select(TryParseFilter<TResource>).ToArray();
                return result.All(r => r.Successful)
                    ? new ParseResult<IFilterPipeline<TResource>>(
                        new ComplexFilterPipeline<TResource>(ComplexFilterOperation.Or,
                            result.Select(r => r.Result).ToArray()))
                    : new ParseResult<IFilterPipeline<TResource>>(result.SelectMany(r => r.Errors).ToArray());
            }

            var simple = node.Split(QueryAliases.AND);
            if (simple.Length > 1)
            {
                var result = simple.Select(TryParseFilter<TResource>).ToArray();
                return result.All(r => r.Successful)
                    ? new ParseResult<IFilterPipeline<TResource>>(new ComplexFilterPipeline<TResource>(
                        ComplexFilterOperation.And,
                        result.Select(r => r.Result).ToArray()))
                    : new ParseResult<IFilterPipeline<TResource>>(result.SelectMany(r => r.Errors).ToArray());
            }

            var values = node.Split(QueryAliases.NODE_DELIMITER);
            if (values.Length == 3)
            {
                var property = values[0];
                var operation = values[1];
                var value = values[2];
                if (!QueryAliases.OperationMappings.ContainsKey(operation))
                {
                    return new ParseResult<IFilterPipeline<TResource>>(new ParseError(
                        "Not supported filter's operation", node));
                }
                return new ParseResult<IFilterPipeline<TResource>>(
                    new SimpleFilterPipeline<TResource>(property, QueryAliases.OperationMappings[operation], value));
            }

            return new ParseResult<IFilterPipeline<TResource>>(new ParseError("Invalid filter format", node));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        private ParseResult<List<ISortPipeline<TResource>>> TryParseSort<TResource>(string node)
            where TResource : class
        {
            if (string.IsNullOrEmpty(node))
            {
                return new ParseResult<List<ISortPipeline<TResource>>>(new List<ISortPipeline<TResource>>());
            }

            var parsed = node.Split(QueryAliases.AND).Select(n =>
            {
                var values = n.Split(QueryAliases.NODE_DELIMITER);
                if (values.Length == 2)
                {
                    var property = values[0];
                    var direction = values[1];
                    if (QueryAliases.SortDirectionMappings.ContainsKey(direction))
                    {
                        return new ParseResult<ISortPipeline<TResource>>(
                            new SortPipeline<TResource>(property, QueryAliases.SortDirectionMappings[direction]));
                    }
                    return new ParseResult<ISortPipeline<TResource>>(new ParseError("Invalid sorting direction", n));
                }
                return new ParseResult<ISortPipeline<TResource>>(new ParseError("Invalid sorting format", n));
            }).ToArray();

            return parsed.All(r => r.Successful)
                ? new ParseResult<List<ISortPipeline<TResource>>>(parsed.Select(r => r.Result).ToList())
                : new ParseResult<List<ISortPipeline<TResource>>>(parsed.SelectMany(r => r.Errors).ToArray());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private ParseResult<int?> TryParseConst(string node)
        {
            if (string.IsNullOrEmpty(node))
            {
                return new ParseResult<int?>((int?)null);
            }

            return int.TryParse(node, out var result)
                ? new ParseResult<int?>(result)
                : new ParseResult<int?>(new ParseError("Constant value must be a valid number", node));
        }
    }
}
