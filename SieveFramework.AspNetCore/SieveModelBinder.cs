using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SieveFramework.AspNetCore.Providers;
using System.Threading.Tasks;

namespace SieveFramework.AspNetCore
{
    public class SieveModelBinder : IModelBinder
    {
        private readonly QueryProvider _provider;
        private readonly MethodInfo _parserMethod;

        public SieveModelBinder()
        {
            _provider = new QueryProvider();
            _parserMethod = _provider.GetType().GetMethod(nameof(QueryProvider.Parse));
        }


        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var query = bindingContext.HttpContext.Request.QueryString;
            if (query.HasValue)
            {
                var underlying = bindingContext.ModelMetadata.UnderlyingOrModelType
                                               .GenericTypeArguments[0];
                var value = _parserMethod.MakeGenericMethod(underlying)
                                         .Invoke(_provider, new object[] { query.Value.Substring(1) });
                bindingContext.Result = ModelBindingResult.Success(value);
            }

            return Task.CompletedTask;
        }
    }
}
