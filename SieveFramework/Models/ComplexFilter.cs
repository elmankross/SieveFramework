using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SieveFramework.Models
{
    public class ComplexFilter : IFilter
    {
        public IFilter[] Filters { get; }
        public ComplexFilterOperation Operation { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="operation"></param>
        public ComplexFilter(ComplexFilterOperation operation, IFilter[] filters)
        {
            Filters = filters;
            Operation = operation;
        }


        /// <inheritdoc />
        public Expression Apply(ParameterExpression target, Func<string, PropertyInfo> propertySelector)
        {
            Expression root = Expression.Empty();
            for (var i = 0; i < Filters.Length; i++)
            {
                var lambda = Filters[i].Apply(target, propertySelector);
                if (i == 0)
                {
                    root = lambda;
                }
                else
                {
                    root = Operation switch
                    {
                        ComplexFilterOperation.And => Expression.And(root, lambda),
                        ComplexFilterOperation.Or => Expression.Or(root, lambda),
                        _ => root
                    };
                }
            }
            return root;
        }
    }

    public enum ComplexFilterOperation
    {
        And,
        Or
    }
}