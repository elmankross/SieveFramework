using Microsoft.AspNetCore.Mvc.ModelBinding;
using SieveFramework.AspNetCore.Models;
using SieveFramework.AspNetCore.Parsers;
using System.Reflection;
using System.Threading.Tasks;

namespace SieveFramework.AspNetCore
{
    public class SieveModelBinder : IModelBinder
    {
        private readonly IParser _parser;
        private readonly MethodInfo _parserMethod;

        public SieveModelBinder(IParser parser)
        {
            _parser = parser;
            _parserMethod = parser.GetType().GetMethod(nameof(IParser.TryParse));
        }


        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var query = bindingContext.HttpContext.Request.QueryString;
            if (query.HasValue)
            {
                var underlying = bindingContext.ModelMetadata.UnderlyingOrModelType
                                               .GenericTypeArguments[0];
                var result = (ParseResult)_parserMethod.MakeGenericMethod(underlying)
                                         .Invoke(_parser, new object[] { query.Value.Substring(1) });
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
