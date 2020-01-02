using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SieveFramework.AspNetCore.Models;
using SieveFramework.AspNetCore.Providers;
using SieveFramework.Models;

namespace SieveFramework.AspNetCore.Swashbuckle
{
    /// <summary>
    /// 
    /// </summary>
    internal class SieveSchema : OpenApiSchema
    {
        public static SieveSchema Instance = new SieveSchema();
        public static OpenApiReference InstanceReference = new OpenApiReference
        {
            Id = "Sieve",
            Type = ReferenceType.Schema
        };
        public static Dictionary<string, OpenApiSchema> References = new Dictionary<string, OpenApiSchema>
        {
            [SieveSchema.InstanceReference.Id] = SieveSchema.Instance,
            [SieveSortSchema.InstanceReference.Id] = SieveSortSchema.Instance,
            [SieveSimpleFilterSchema.InstanceReference.Id] = SieveSimpleFilterSchema.Instance,
            [SieveComplexFilterSchema.InstanceReference.Id] = SieveComplexFilterSchema.Instance,
        };

        private SieveSchema()
        {
            Title = "Sieve";
            Type = "object";
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [nameof(Sieve<object>.Sort)] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Reference = SieveSortSchema.InstanceReference }
                },
                [nameof(Sieve<object>.Filter)] = new OpenApiSchema
                {
                    Type = "object",
                    OneOf = new List<OpenApiSchema>
                    {
                        new OpenApiSchema { Reference = SieveSimpleFilterSchema.InstanceReference },
                        new OpenApiSchema { Reference = SieveComplexFilterSchema.InstanceReference }
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
        }
    }


    /// <summary>
    /// 
    /// </summary>
    internal class SieveSortSchema : OpenApiSchema
    {
        public static SieveSortSchema Instance = new SieveSortSchema();
        public static OpenApiReference InstanceReference = new OpenApiReference
        {
            Id = "SieveSort",
            Type = ReferenceType.Schema
        };

        private SieveSortSchema()
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
                    Enum = QueryAliases.SortDirectionMappings.Select(d => new OpenApiString(d.Key))
                                       .OfType<IOpenApiAny>()
                                       .ToList()
                }
            };
        }
    }


    /// <summary>
    /// 
    /// </summary>
    internal class SieveSimpleFilterSchema : OpenApiSchema
    {
        public static SieveSimpleFilterSchema Instance = new SieveSimpleFilterSchema();
        public static OpenApiReference InstanceReference = new OpenApiReference
        {
            Id = "SieveSimpleFilter",
            Type = ReferenceType.Schema
        };

        private SieveSimpleFilterSchema()
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
                    Enum = QueryAliases.OperationMappings.Select(o => new OpenApiString(o.Key))
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
    internal class SieveComplexFilterSchema : OpenApiSchema
    {
        public static SieveComplexFilterSchema Instance = new SieveComplexFilterSchema();
        public static OpenApiReference InstanceReference = new OpenApiReference
        {
            Id = "SieveComplexFilter",
            Type = ReferenceType.Schema
        };


        private SieveComplexFilterSchema()
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
                        new OpenApiSchema { Reference = SieveSimpleFilterSchema.InstanceReference },
                        new OpenApiSchema { Reference = SieveComplexFilterSchema.InstanceReference }
                    }
                },
                [nameof(IComplexFilterPipeline<object>.Operation)] = new OpenApiSchema
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny>
                    {
                        new OpenApiString(QueryAliases.AND),
                        new OpenApiString(QueryAliases.OR)
                    }
                }
            };
        }
    }
}
