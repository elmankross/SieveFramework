using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SieveFramework.Models
{
    public class SimpleFilter<TModel, TValue> : IFilter<TModel>
        where TModel : class
    {
        public string Property { get; }
        public SimpleFilterOperation Operation { get; }
        public TValue Value { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="operation"></param>
        /// <param name="value"></param>
        public SimpleFilter(Expression<Func<TModel, TValue>> property, SimpleFilterOperation operation, TValue value)
        {
            Property = ((MemberExpression)property.Body).Member.Name;
            Operation = operation;
            Value = value;
        }


        /// <inheritdoc />
        public Expression Apply(ParameterExpression target, Func<string, PropertyInfo> propertySelector)
        {
            var propertyType = propertySelector(Property).PropertyType;
            var property = Expression.Property(target, Property);
            var constant = Expression.Constant(propertyType != Value.GetType()
                                               ? Convert.ChangeType(Value, propertyType)
                                               : Value);
            return Operation switch
            {
                SimpleFilterOperation.Equal => Expression.Equal(property, constant),
                SimpleFilterOperation.NotEqual => Expression.NotEqual(property, constant),
                SimpleFilterOperation.Greater => Expression.GreaterThan(property, constant),
                SimpleFilterOperation.GreaterOrEqual => Expression.GreaterThanOrEqual(property, constant),
                SimpleFilterOperation.Less => Expression.LessThan(property, constant),
                SimpleFilterOperation.LessOrEqual => Expression.LessThanOrEqual(property, constant),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }


    public enum SimpleFilterOperation
    {
        Equal,
        NotEqual,
        Greater,
        GreaterOrEqual,
        Less,
        LessOrEqual
    }
}
