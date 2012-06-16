using System;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.ModelFirst.Annotations;

namespace Umbraco.Framework.Persistence.ModelFirst
{
    public class CustomTypedEntity<TImplementor> : CustomTypedEntity
        where TImplementor : TypedEntity
    {
        protected T BaseAutoGet<T>(Expression<Func<TImplementor, T>> fromProperty, T defaultValue = default(T))
        {
            var attrib = AnnotationQueryStructureBinder.GetAutoAttribute(fromProperty);
            return this.Attribute<T>(attrib.Alias, defaultValue);
        }

        protected void BaseAutoSet<T>(Expression<Func<TImplementor, T>> forProperty, T value)
        {
            var attrib = AnnotationQueryStructureBinder.GetAutoAttribute(forProperty);
            this.Attributes[attrib.Alias].DynamicValue = value;
        }
    }

    public class CustomTypedEntity : TypedEntity
    {
        public object this[string alias]
        {
            get { return Attributes[alias].DynamicValue; }
            set { Attributes[alias].DynamicValue = value; }
        }

        protected T BaseAutoGet<T>(string attributeAlias, T defaultValue = default(T))
        {
            return this.Attribute<T>(attributeAlias, defaultValue);
        }

        protected void BaseAutoSet<T>(string attributeAlias, T value)
        {
            this.Attributes[attributeAlias].DynamicValue = value;
        }
    }
}
