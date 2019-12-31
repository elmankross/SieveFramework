using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SieveFramework.Models
{
    public class Sort<TModel, TValue> : Sort
    {
        public Sort(Expression<Func<TModel, TValue>> property, SortDirection direction)
            : base(((MemberExpression)property.Body).Member.Name, direction)
        {
        }
    }


    public class Sort
    {
        public string Property { get; }
        public SortDirection Direction { get; }

        public Sort(string property, SortDirection direction)
        {
            Property = property;
            Direction = direction;
        }


        public Expression Apply(ParameterExpression target, IQueryable query, PropertyInfo property)
        {
            var prop = Expression.Property(target, Property);
            var exp = Expression.Lambda(prop, target);
            var method = Direction == SortDirection.Ascending
                ? nameof(Queryable.OrderBy)
                : nameof(Queryable.OrderByDescending);
            return Expression.Call(typeof(Queryable), method, new[] { target.Type, property.PropertyType },
                query.Expression, exp);
        }
    }


    public enum SortDirection
    {
        Ascending,
        Descending
    }
}