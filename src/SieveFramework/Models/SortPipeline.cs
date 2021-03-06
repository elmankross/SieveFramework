﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SieveFramework.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public interface ISortPipeline<TResource> : IPipeline<TResource>
        where TResource : class
    {
        string Property { get; }
        SortDirection Direction { get; }
    }


    public enum SortDirection
    {
        Ascending,
        Descending
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SortPipeline<TResource, TValue> : SortPipeline<TResource>
        where TResource : class
        where TValue : IConvertible
    {
        public SortPipeline(Expression<Func<TResource, TValue>> property, SortDirection direction)
            : base(((MemberExpression)property.Body).Member.Name, direction) { }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public class SortPipeline<TResource> : ISortPipeline<TResource>
        where TResource : class
    {
        public string Property { get; }
        public SortDirection Direction { get; }


        public SortPipeline(string property, SortDirection direction)
        {
            Property = property;
            Direction = direction;
        }

        /// <inheritdoc />
        public Expression Apply(ParameterExpression target, Func<string, PropertyInfo> _)
        {
            var prop = Expression.Property(target, Property);
            return Expression.Lambda(prop, target);
        }
    }
}