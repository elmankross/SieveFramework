using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace SieveFramework.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public interface ISimpleFilterPipeline<TResource> : IFilterPipeline<TResource>
        where TResource : class
    {
        string Property { get; }
        SimpleFilterOperation Operation { get; }
        string Value { get; }
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
    public class SimpleFilterPipeline<TResource, TValue> : SimpleFilterPipeline<TResource>
        where TResource : class
        where TValue: IConvertible
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="operation"></param>
        /// <param name="value"></param>
        public SimpleFilterPipeline(Expression<Func<TResource, TValue>> property,
                                    SimpleFilterOperation operation,
                                    TValue value
        ) : base(((MemberExpression)property.Body).Member.Name, operation, value.ToString()) { }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public class SimpleFilterPipeline<TResource> : ISimpleFilterPipeline<TResource>
        where TResource : class
    {
        public string Property { get; }
        public SimpleFilterOperation Operation { get; }
        public string Value { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="operation"></param>
        /// <param name="value"></param>
        public SimpleFilterPipeline(string property, SimpleFilterOperation operation, string value)
        {
            Property = property;
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



    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public class ComplexFilterPipeline<TResource> : IComplexFilterPipeline<TResource>
        where TResource : class
    {
        public IFilterPipeline<TResource>[] Filters { get; }
        public ComplexFilterOperation Operation { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="operation"></param>
        public ComplexFilterPipeline(ComplexFilterOperation operation, IFilterPipeline<TResource>[] filters)
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
