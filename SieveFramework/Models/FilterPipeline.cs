using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SieveFramework.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public interface ISimpleFilterPipeline<TResource, out TValue> : IFilterPipeline<TResource>
        where TResource : class
    {
        string Property { get; }
        SimpleFilterOperation Operation { get; }
        TValue Value { get; }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public interface IComplexFilterPipeline<TResource> : IFilterPipeline<TResource>
        where TResource : class
    {
        IFilterPipeline<TResource>[] Filters { get; }
        ComplexFilterOperation Operation { get; }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public interface IFilterPipeline<TResource> : IPipeline<TResource>
        where TResource : class
    {
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


    public enum ComplexFilterOperation
    {
        And,
        Or
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SimpleFilterPipeline<TResource, TValue> : ISimpleFilterPipeline<TResource, TValue>
        where TResource : class
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
        public SimpleFilterPipeline(Expression<Func<TResource, TValue>> property, SimpleFilterOperation operation, TValue value)
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


    public class ComplexFilter<TResource> : IComplexFilterPipeline<TResource>
        where TResource : class
    {
        public IFilterPipeline<TResource>[] Filters { get; }
        public ComplexFilterOperation Operation { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="operation"></param>
        public ComplexFilter(ComplexFilterOperation operation, IFilterPipeline<TResource>[] filters)
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
}
