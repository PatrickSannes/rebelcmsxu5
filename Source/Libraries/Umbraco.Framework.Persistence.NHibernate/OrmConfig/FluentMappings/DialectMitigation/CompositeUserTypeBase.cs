using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.UserTypes;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Expressions;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation
{
    public abstract class CompositeUserTypeBase<T> : ICompositeUserType
    {
        private readonly List<PropertyInfo> _properties = new List<PropertyInfo>();

        /// <summary>
        /// Maps a property for the composite user type.
        /// </summary>
        /// <param name="property">A expression representing the property to map.</param>
        protected virtual void MapProperty(Expression<Func<T, object>> property)
        {
            var visitor = new PropertyVisitor();
            visitor.Visit(property);

            _properties.Add(visitor.Property);
        }

        /// <summary>
        /// Inherits must build up the underlying type and return it.
        /// </summary>
        /// <param name="propertyValues">An array of objects that contain the values retrieved from the database.</param>
        /// <returns></returns>
        protected abstract T CreateInstance(object[] propertyValues);

        /// <summary>
        /// Performs a deep copy of a source entity.
        /// </summary>
        /// <param name="source">The source entity whose deep copy should be returned.</param>
        /// <returns>T</returns>
        /// <remarks>
        /// Inheritors must return a cloned or deep copied instance of the provided entity. If 
        /// </remarks>
        protected abstract T PerformDeepCopy(T source);

        /// <summary>
        /// Get the value of a property
        /// </summary>
        /// <param name="component">an instance of class mapped by this "type"</param>
        /// <param name="property"></param>
        /// <returns>
        /// the property value
        /// </returns>
        public object GetPropertyValue(object component, int property)
        {
            var propInfo = _properties[property];
            return propInfo.GetValue(component, null);
        }

        /// <summary>
        /// Set the value of a property
        /// </summary>
        /// <param name="component">an instance of class mapped by this "type"</param>
        /// <param name="property"></param>
        /// <param name="value">the value to set</param>
        public void SetPropertyValue(object component, int property, object value)
        {
            var propInfo = _properties[property];
            propInfo.SetValue(component, value, null);
        }

        /// <summary>
        /// Compare two instances of the class mapped by this type for persistence
        /// "equality", ie. equality of persistent state.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>
        /// </returns>
        bool ICompositeUserType.Equals(object x, object y)
        {
            if (x == null || y == null)
                return false;
            return ReferenceEquals(x, y) || x.Equals(y);
        }

        /// <summary>
        /// Get a hashcode for the instance, consistent with persistence "equality"
        /// </summary>
        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        /// <summary>
        /// Retrieve an instance of the mapped class from a IDataReader. Implementors
        /// should handle possibility of null values.
        /// </summary>
        /// <param name="dr">IDataReader</param>
        /// <param name="names">the column names</param>
        /// <param name="session"></param>
        /// <param name="owner">the containing entity</param>
        /// <returns>
        /// </returns>
        public object NullSafeGet(IDataReader dr, string[] names, ISessionImplementor session, object owner)
        {
            if (dr == null)
                return null;

            var values = new object[names.Length];
            for (var i = 0; i < names.Length; i++)
                values[i] = NHibernateUtil.GuessType(_properties[i].PropertyType)
                    .NullSafeGet(dr, names[i], session, owner);
            return CreateInstance(values);
        }

        /// <summary>
        /// Write an instance of the mapped class to a prepared statement.
        /// Implementors should handle possibility of null values.
        /// A multi-column type should be written to parameters starting from index.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="session"></param>
        public void NullSafeSet(IDbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session)
        {
            if (value == null)
                return;

            var propIndex = index;
            foreach (var t in _properties)
            {
                try
                {
                    var property = t;
                    var valueToSet = property.PropertyType == value.GetType() ? value : property.GetValue(value, null);
                    NHibernateUtil.GuessType(property.PropertyType).NullSafeSet(cmd, valueToSet, propIndex, settable,
                                                                                session);
                    propIndex++;
                }
                catch (Exception e)
                {
                    LogHelper.Warn<CompositeUserTypeBase<T>>("Error handling null setters.{0}{1}{0}{2}",
                                                             Environment.NewLine, e.Message, e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Return a deep copy of the persistent state, stopping at entities and at collections.
        /// </summary>
        /// <param name="value">generally a collection element or entity field</param>
        /// <returns>
        /// </returns>
        public object DeepCopy(object value)
        {
            return PerformDeepCopy((T)value);
        }

        /// <summary>
        /// Transform the object into its cacheable representation.
        /// At the very least this method should perform a deep copy.
        /// That may not be enough for some implementations, method should perform a deep copy. 
        /// That may not be enough for some implementations, however; for example, 
        /// associations must be cached as identifier values. (optional operation)
        /// </summary>
        /// <param name="value">the object to be cached</param>
        /// <param name="session"></param>
        /// <returns>
        /// </returns>
        public virtual object Disassemble(object value, ISessionImplementor session)
        {
            return DeepCopy(value);
        }

        /// <summary>
        /// Reconstruct an object from the cacheable representation.
        /// At the very least this method should perform a deep copy. (optional operation)
        /// </summary>
        /// <param name="cached">the object to be cached</param>
        /// <param name="session"></param>
        /// <param name="owner"></param>
        /// <returns>
        /// </returns>
        public virtual object Assemble(object cached, ISessionImplementor session, object owner)
        {
            return DeepCopy(cached);
        }

        /// <summary>
        /// During merge, replace the existing (target) value in the entity we are merging to
        /// with a new (original) value from the detached entity we are merging. For immutable
        /// objects, or null values, it is safe to simply return the first parameter. For
        /// mutable objects, it is safe to return a copy of the first parameter. However, since
        /// composite user types often define component values, it might make sense to recursively 
        /// replace component values in the target object.
        /// </summary>
        public virtual object Replace(object original, object target, ISessionImplementor session, object owner)
        {
            return !IsMutable ? original : DeepCopy(original);
        }

        /// <summary>
        /// Get the "property names" that may be used in a query. 
        /// </summary>
        public virtual string[] PropertyNames
        {
            get
            {
                var names = new string[_properties.Count];
                for (var i = 0; i < _properties.Count; i++)
                    names[i] = _properties[i].Name;
                return names;
            }
        }

        /// <summary>
        /// Get the corresponding "property types"
        /// </summary>
        public virtual IType[] PropertyTypes
        {
            get
            {
                var types = new IType[_properties.Count];
                for (var i = 0; i < _properties.Count; i++)
                    types[i] = NHibernateUtil.GuessType(_properties[i].PropertyType);
                return types;
            }
        }

        /// <summary>
        /// The class returned by NullSafeGet().
        /// </summary>
        public Type ReturnedClass
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Are objects of this type mutable?
        /// </summary>
        public abstract bool IsMutable
        {
            get;
        }
    }
}
#region Note
/* Based heavily on NCommon as a placeholder for a w-i-p commit */
#endregion