using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SieveFramework.Providers
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class ModelProvider<TModel> : ModelProvider
        where TModel : class
    {
        /// <summary>
        /// 
        /// </summary>
        internal ModelProvider() : base(typeof(TModel))
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector"></param>
        public void CanSort<TType>(Expression<Func<TModel, TType>> selector)
        {
            var property = ((MemberExpression)selector.Body).Member as PropertyInfo;
            CanSort(property);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="selector"></param>
        public void CanFilter<TType>(Expression<Func<TModel, TType>> selector)
        {
            var property = ((MemberExpression)selector.Body).Member as PropertyInfo;
            CanFilter(property);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class ModelProvider
    {
        public readonly ParameterExpression Target;
        internal readonly HashSet<string> Filterable;
        internal readonly HashSet<string> Sortable;
        internal readonly Dictionary<string, PropertyInfo> Properties;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelType"></param>
        internal ModelProvider(Type modelType)
        {
            Target = Expression.Parameter(modelType);
            Properties = new Dictionary<string, PropertyInfo>();
            Filterable = new HashSet<string>();
            Sortable = new HashSet<string>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetFilterableProperties()
        {
            return Filterable;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetSortableProperties()
        {
            return Sortable;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        internal bool TrySort(string property, out PropertyInfo info)
        {
            if (!Sortable.Contains(property))
            {
                info = null;
                return false;
            }

            info = Properties[property];
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        internal bool TryFilter(string property, out PropertyInfo info)
        {
            if (!Filterable.Contains(property))
            {
                info = null;
                return false;
            }

            info = Properties[property];
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        internal void CanSort(PropertyInfo property)
        {
            var name = property.Name;
            Sortable.Add(name);
            if (!Properties.ContainsKey(name))
            {
                Properties.Add(name, property);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        internal void CanFilter(PropertyInfo property)
        {
            var name = property.Name;
            Filterable.Add(name);
            if (!Properties.ContainsKey(name))
            {
                Properties.Add(name, property);
            }
        }
    }
}
