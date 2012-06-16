using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Versioning;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Dynamic
{
    /// <summary>
    /// A dynamic implementation of the <see cref="ITypedEntity"/> object
    /// </summary>
    public sealed class DynamicTypedEntity : DynamicEntity, ITypedEntity
    {
        private readonly ITypedEntity _entity;

        internal DynamicTypedEntity(ITypedEntity entity)
            : base(entity)
        {
            Contract.Requires(entity != null);
            _entity = entity;
        }

        #region ITypedEntity Members

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>The attributes.</value>
        public ITypedAttributeCollection Attributes
        {
            get { return _entity.Attributes; }
            set { _entity.Attributes = value; }
        }

        /// <summary>
        /// Gets or sets the attribute groups.
        /// </summary>
        /// <value>The attribute groups.</value>
        public IAttributeGroupCollection AttributeGroups
        {
            get { return _entity.AttributeGroups; }
            set { _entity.AttributeGroups = value; }
        }

        /// <summary>
        /// Gets the attribute schema.
        /// </summary>
        /// <value>The attribute schema.</value>
        public IAttributionSchemaDefinition AttributionSchema
        {
            get { return _entity.AttributionSchema; }
            set { _entity.AttributionSchema = value; }
        }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>The type of the entity.</value>
        public IEntityTypeDefinition EntityType
        {
            get { return _entity.EntityType; }
            set { _entity.EntityType = value; }
        }

        #endregion

        [ContractInvariantMethod]
        private void SpecificObjectInvariant()
        {
            Contract.Invariant(_entity != null);
        }

        /// <summary>
        /// Attempts to resolve the attribute from the binder information
        /// </summary>
        /// <param name="binderInfo">The binder info.</param>
        /// <returns></returns>
        private ITypedAttribute TryResolveAttribute(DynamicEntity.BinderInfo binderInfo)
        {
            //TODO: Az - Should we be returning a dynamic version of Attribute? Could have some fun there if we do

            //find all the properties in the object schema
            //TODO: handle binderInfo.IgnoreCase
            var properties = from tab in _entity.AttributionSchema.AttributeGroupDefinitions
                             from p in tab.AttributeDefinitions
                             where p.Alias == binderInfo.Name || p.Id.ValueAsString == binderInfo.Name
                             select p;

            var property = properties.SingleOrDefault();
            //if there is a property then find the attribute that implements it
            if (property != default(IAttributeDefinition))
                //Need to work out some better way to expose the values, x.Value.Value, err that's not good
                return Attributes.Where<ITypedAttribute>(x => x.Value.AttributeType == property).Select(x => x.Value.Value).SingleOrDefault();

            return null;
        }

        private IAttributeGroupDefinition TryResolveAttributeGroupDefinition(DynamicEntity.BinderInfo binderInfo)
        {
            //finds all the attribute groups that either have the Name or the ID
            //TODO: handle binderInfo.IgnoreCase
            var tabs = from tab in _entity.AttributionSchema.AttributeGroupDefinitions
                       where tab.Name == binderInfo.Name || tab.Id.ValueAsString == binderInfo.Name
                       select tab;

            return tabs.SingleOrDefault();
        }

        private IEnumerable<dynamic> TryResolveChildrenByTypeName(DynamicEntity.BinderInfo binderInfo)
        {
            //ensures that we're working with a vertexed entity
            if (typeof(IEntityVertex).IsAssignableFrom(_entity.GetType()))
            {
                var vertex = (IEntityVertex)_entity;
                var children = from c in (IDictionary<IMappedIdentifier, IEntityVertex>)vertex.DescendentEntities //we have to do the explicit cast as it implements IEnumerable twice, as we want the dictionary
                               where MakePluralName(c.Key.ValueAsString) == binderInfo.Name || MakePluralName(c.Value.Id.ValueAsString) == binderInfo.Name
                               select ((ITypedEntity)c.Value).AsDynamic(); //either we're returning too low a type from c.Value or we need a Dynamic which isn't Typed

                //we wont return null here, as the child may be allowed, there just aren't any
                //TODO: Should we return null or not? (refer to previous comment)
                return children;
            }
            else
            {
                //You're doing it wrong!
                throw new NotSupportedException(string.Format("Entity {0} is not an instance of {1} and hence it does not support the concept of children", _entity.GetType().FullName, typeof(IEntityVertex).FullName));
            }
        }

        private static string MakePluralName(string name)
        {
            if ((name.EndsWith("x", StringComparison.OrdinalIgnoreCase) || name.EndsWith("ch", StringComparison.OrdinalIgnoreCase)) || (name.EndsWith("ss", StringComparison.OrdinalIgnoreCase) || name.EndsWith("sh", StringComparison.OrdinalIgnoreCase)))
            {
                name = name + "es";
                return name;
            }
            if ((name.EndsWith("y", StringComparison.OrdinalIgnoreCase) && (name.Length > 1)) && !IsVowel(name[name.Length - 2]))
            {
                name = name.Remove(name.Length - 1, 1);
                name = name + "ies";
                return name;
            }
            if (!name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                name = name + "s";
            }
            return name;
        }

        private static bool IsVowel(char c)
        {
            switch (c)
            {
                case 'O':
                case 'U':
                case 'Y':
                case 'A':
                case 'E':
                case 'I':
                case 'o':
                case 'u':
                case 'y':
                case 'a':
                case 'e':
                case 'i':
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned. For example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, the <paramref name="value"/> is "Test".</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Contract.Requires(binder != null);

            var binderInfo = new DynamicEntity.BinderInfo(binder.IgnoreCase, binder.Name, binder.ReturnType);

            ITypedAttribute attr = TryResolveAttribute(binderInfo);
            if (attr != null)
            {
                attr.Value = value;
                return true;
            }

            //TODO: Support all the different things that the member may try to represent

            return base.TrySetMember(binder, value);
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject"/> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result"/>.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Contract.Requires(binder != null);
            var binderInfo = new DynamicEntity.BinderInfo(binder.IgnoreCase, binder.Name, binder.ReturnType);

            ITypedAttribute attr = TryResolveAttribute(binderInfo);
            if (attr != null)
            {
                result = attr;
                return true;
            }

            //TODO: Support all the different things that the member may try to represent

            return base.TryGetMember(binder, out result);
        }
    }
}