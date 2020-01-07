using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SieveFramework.Models
{
    public interface IPipeline<TResource> : IPipeline
        where TResource : class
    {
    }

    public interface IPipeline
    {
        /// <summary>
        /// Apply filter to expression
        /// </summary>
        /// <param name="target"></param>
        /// <param name="propertySelector"></param>
        /// <returns></returns>
        Expression Apply(ParameterExpression target, Func<string, PropertyInfo> propertySelector);
    }
}