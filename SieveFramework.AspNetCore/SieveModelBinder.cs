using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SieveFramework.AspNetCore.Providers;
using System.Threading.Tasks;
using SieveFramework.AspNetCore.Models;

namespace SieveFramework.AspNetCore
{
    public class SieveModelBinder : IModelBinder
    {
        private readonly QueryProvider _provider;
        private readonly MethodInfo _parserMethod;

        public SieveModelBinder()
        {
            _provider = new QueryProvider();
            _parserMethod = _provider.GetType().GetMethod(nameof(QueryProvider.TryParse));
        }


        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var query = bindingContext.HttpContext.Request.QueryString;
            if (query.HasValue)
            {
                var underlying = bindingContext.ModelMetadata.UnderlyingOrModelType
                                               .GenericTypeArguments[0];
                var result = (ParseResult)_parserMethod.MakeGenericMethod(underlying)
                                         .Invoke(_provider, new object[] { query.Value.Substring(1) });
                if (result.Successful)
                {
                    bindingContext.Result = ModelBindingResult.Success(result.Result);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        bindingContext.ModelState.AddModelError(error.Context, error.Message);
                    }

                    bindingContext.Result = ModelBindingResult.Failed();
                }
            }

            return Task.CompletedTask;
        }
    }
}
