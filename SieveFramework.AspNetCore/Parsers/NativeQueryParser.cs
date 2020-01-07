using System;
using System.Collections.Generic;
using System.Linq;
using SieveFramework.AspNetCore.Models;
using SieveFramework.Models;

namespace SieveFramework.AspNetCore.Parsers
{
    /// <summary>
    /// 
    /// </summary>
    public class NativeQueryParser : IParser
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


        /// <inheritdoc />
        public Dictionary<string, SimpleFilterOperation> SimpleOperationsMapping { get; } =
            new Dictionary<string, SimpleFilterOperation>
            {
                [EQUAL] = SimpleFilterOperation.Equal,
                [NOT_EQUAL] = SimpleFilterOperation.NotEqual,
                [GREATER_THAN] = SimpleFilterOperation.Greater,
                [GREATER_THAN_OR_EQUAL] = SimpleFilterOperation.GreaterOrEqual,
                [LESS_THAN] = SimpleFilterOperation.Less,
                [LESS_THAN_OR_EQUAL] = SimpleFilterOperation.LessOrEqual
            };

        /// <inheritdoc />
        public Dictionary<string, ComplexFilterOperation> ComplexOperationsMapping { get; } =
            new Dictionary<string, ComplexFilterOperation>
            {
                [AND] = ComplexFilterOperation.And,
                [OR] = ComplexFilterOperation.Or
            };


        /// <inheritdoc />
        public Dictionary<string, SortDirection> SortDirectionsMapping { get; } = new Dictionary<string, SortDirection>
        {
            [ASC] = SortDirection.Ascending,
            [DESC] = SortDirection.Descending
        };


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
            foreach (var node in query.Split(OPERATION_DELIMITER))
            {
                if (node.StartsWith(FILTER, StringComparison.OrdinalIgnoreCase))
                {
                    var result = TryParseFilter<TResource>(node.Substring(FILTER.Length));
                    if (result.Successful)
                    {
                        model.Filter = result.Result;
                    }
                    else
                    {
                        return new ParseResult<Sieve<TResource>>(result.Errors);
                    }
                }
                else if (node.StartsWith(SORT, StringComparison.OrdinalIgnoreCase))
                {
                    var result = TryParseSort<TResource>(node.Substring(SORT.Length));
                    if (result.Successful)
                    {
                        model.Sort = result.Result;
                    }
                    else
                    {
                        return new ParseResult<Sieve<TResource>>(result.Errors);
                    }
                }
                else if (node.StartsWith(SKIP, StringComparison.OrdinalIgnoreCase))
                {
                    var result = TryParseConst(node.Substring(SKIP.Length));
                    if (result.Successful)
                    {
                        model.Skip = result.Result ?? model.Skip;
                    }
                    else
                    {
                        return new ParseResult<Sieve<TResource>>(result.Errors);
                    }
                }
                else if (node.StartsWith(TAKE, StringComparison.OrdinalIgnoreCase))
                {
                    var result = TryParseConst(node.Substring(TAKE.Length));
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

            foreach (var operation in ComplexOperationsMapping)
            {
                var nodes = node.Split(operation.Key);
                if (nodes.Length > 1)
                {
                    var result = nodes.Select(TryParseFilter<TResource>).ToArray();
                    return result.All(r => r.Successful)
                        ? new ParseResult<IFilterPipeline<TResource>>(new ComplexFilterPipeline<TResource>(
                            operation.Value,
                            result.Select(r => r.Result).ToArray()))
                        : new ParseResult<IFilterPipeline<TResource>>(result.SelectMany(r => r.Errors).ToArray());
                }
            }

            var values = node.Split(NODE_DELIMITER);
            if (values.Length == 3)
            {
                var property = values[0];
                var operation = values[1];
                var value = values[2];
                if (!SimpleOperationsMapping.ContainsKey(operation))
                {
                    return new ParseResult<IFilterPipeline<TResource>>(new ParseError(
                        "Not supported filter's operation", node));
                }
                return new ParseResult<IFilterPipeline<TResource>>(
                    new SimpleFilterPipeline<TResource>(property, SimpleOperationsMapping[operation], value));
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

            var parsed = node.Split(AND).Select(n =>
            {
                var values = n.Split(NODE_DELIMITER);
                if (values.Length == 2)
                {
                    var property = values[0];
                    var direction = values[1];
                    if (SortDirectionsMapping.ContainsKey(direction))
                    {
                        return new ParseResult<ISortPipeline<TResource>>(
                            new SortPipeline<TResource>(property, SortDirectionsMapping[direction]));
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
