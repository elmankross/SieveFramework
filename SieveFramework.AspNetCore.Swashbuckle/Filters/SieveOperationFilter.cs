using Microsoft.OpenApi.Models;
using SieveFramework.Providers;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace SieveFramework.AspNetCore.Swashbuckle.Filters
{
    public class SieveOperationFilter : IOperationFilter
    {
        private readonly ISieveProvider _provider;

        public SieveOperationFilter(ISieveProvider provider)
        {
            _provider = provider;
        }


        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var parameters = context.SchemaRepository.Schemas
                                    .Where(p => p.Key.Contains("IPredicate"))
                                    .ToDictionary(x => x.Key, x => x.Value);
            foreach (var parameter in parameters)
            {
                context.SchemaRepository.Schemas.Remove(parameter.Key);
                foreach (var sieveReference in SieveSchema.References)
                {
                    context.SchemaRepository.Schemas.TryAdd(sieveReference.Key, sieveReference.Value);
                }
            }

            foreach (var parameter in operation.Parameters)
            {
                if (parameters.ContainsKey(parameter.Schema.Items.Reference.Id))
                {
                    parameter.Schema.Reference = SieveSchema.InstanceReference;
                    parameter.Schema.Items = null;

                    var model = context.ApiDescription.ParameterDescriptions.Single(d => d.Name == parameter.Name);
                    var provider = _provider.SingleOrDefault(p => p.Value.Target.Type == model.Type.GetGenericArguments()[0]);

                    parameter.Description += "<pre><p>FILTERABLE</p><ul>";
                    foreach (var filterable in provider.Value.GetFilterableProperties())
                    {
                        parameter.Description += "<li>" + filterable + "</li>";
                    }

                    parameter.Description += "</ul>";
                    parameter.Description += "<p>SORTABLE</p><ul>";
                    foreach (var sortable in provider.Value.GetSortableProperties())
                    {
                        parameter.Description += "<li>" + sortable + "</li>";
                    }
                    parameter.Description += "</ul></pre>";
                }
            }
        }
    }
}
