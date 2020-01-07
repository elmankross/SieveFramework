using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SieveFramework.AspNetCore.Models;
using SieveFramework.AspNetCore.Parsers;
using SieveFramework.Models;

namespace SieveFramework.AspNetCore.Swashbuckle
{
    /// <summary>
    /// 
    /// </summary>
    internal class SieveScheme<TParser> : OpenApiSchema
        where TParser : IParser
    {
        public Dictionary<string, OpenApiSchema> References { get; }
        public static OpenApiReference InstanceReference = new OpenApiReference
        {
            Id = "Sieve",
            Type = ReferenceType.Schema
        };

        internal SieveScheme(TParser parser)
        {
            Title = "Sieve";
            Type = "object";
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [nameof(Sieve<object>.Sort)] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Reference = SieveSortSchema<TParser>.InstanceReference }
                },
                [nameof(Sieve<object>.Filter)] = new OpenApiSchema
                {
                    Type = "object",
                    OneOf = new List<OpenApiSchema>
                    {
                        new OpenApiSchema { Reference = SieveSimpleFilterSchema<TParser>.InstanceReference },
                        new OpenApiSchema { Reference = SieveComplexFilterSchema<TParser>.InstanceReference }
                    }
                },
                [nameof(Sieve<object>.Skip)] = new OpenApiSchema
                {
                    Type = "integer"
                },
                [nameof(Sieve<object>.Take)] = new OpenApiSchema
                {
                    Type = "integer"
                }
            };

            References = new Dictionary<string, OpenApiSchema>
            {
                [SieveScheme<TParser>.InstanceReference.Id] = this,
                [SieveSortSchema<TParser>.InstanceReference.Id] = new SieveSortSchema<TParser>(parser),
                [SieveSimpleFilterSchema<TParser>.InstanceReference.Id] = new SieveSimpleFilterSchema<TParser>(parser),
                [SieveComplexFilterSchema<TParser>.InstanceReference.Id] = new SieveComplexFilterSchema<TParser>(parser)
            };
        }
    }


    /// <summary>
    /// 
    /// </summary>
    internal class SieveSortSchema<TParser> : OpenApiSchema
        where TParser : IParser
    {
        public static OpenApiReference InstanceReference = new OpenApiReference
        {
            Id = "SieveSort",
            Type = ReferenceType.Schema
        };

        internal SieveSortSchema(TParser parser)
        {
            Title = "SieveSort";
            Type = "object";
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [nameof(ISortPipeline<object>.Property)] = new OpenApiSchema
                {
                    Type = "string"
                },
                [nameof(ISortPipeline<object>.Direction)] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = parser.SortDirectionsMapping.Select(d => new OpenApiString(d.Key))
                                 .OfType<IOpenApiAny>()
                                 .ToList()
                }
            };
        }
    }


    /// <summary>
    /// 
    /// </summary>
    internal class SieveSimpleFilterSchema<TParser> : OpenApiSchema
        where TParser : IParser
    {
        public static OpenApiReference InstanceReference = new OpenApiReference
        {
            Id = "SieveSimpleFilter",
            Type = ReferenceType.Schema
        };

        internal SieveSimpleFilterSchema(TParser parser)
        {
            Title = "SieveSimpleFilter";
            Type = "object";
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [nameof(ISimpleFilterPipeline<object>.Property)] = new OpenApiSchema
                {
                    Type = "string"
                },
                [nameof(ISimpleFilterPipeline<object>.Operation)] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = parser.SimpleOperationsMapping.Select(o => new OpenApiString(o.Key))
                                 .OfType<IOpenApiAny>()
                                 .ToList()
                },
                [nameof(ISimpleFilterPipeline<object>.Value)] = new OpenApiSchema
                {
                    Type = "string"
                }
            };
        }
    }


    /// <summary>
    /// 
    /// </summary>
    internal class SieveComplexFilterSchema<TParser> : OpenApiSchema
        where TParser : IParser
    {
        public static OpenApiReference InstanceReference = new OpenApiReference
        {
            Id = "SieveComplexFilter",
            Type = ReferenceType.Schema
        };

        internal SieveComplexFilterSchema(IParser parser)
        {
            Title = "SieveComplexFilter";
            Type = "object";
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [nameof(IComplexFilterPipeline<object>.Filters)] = new OpenApiSchema
                {
                    Type = "array",
                    AnyOf = new List<OpenApiSchema>
                    {
                        new OpenApiSchema { Reference = SieveSimpleFilterSchema<TParser>.InstanceReference },
                        new OpenApiSchema { Reference = SieveComplexFilterSchema<TParser>.InstanceReference }
                    }
                },
                [nameof(IComplexFilterPipeline<object>.Operation)] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = parser.ComplexOperationsMapping.Select(o => new OpenApiString(o.Key))
                                 .OfType<IOpenApiAny>()
                                 .ToList()
                }
            };
        }
    }
}
