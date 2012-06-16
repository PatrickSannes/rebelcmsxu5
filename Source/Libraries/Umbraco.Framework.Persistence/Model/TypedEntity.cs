using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.LinqSupport;
using Umbraco.Framework.Persistence.Model.Versioning;

namespace Umbraco.Framework.Persistence.Model
{
    using Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata;

    [QueryStructureBinderOfType(typeof(PersistenceModelStructureBinder))]
    [DebuggerDisplay("Id: {Id}")]
    public class TypedEntity : AbstractEntity, IRelatableEntity, IVersionableEntity
    {
        public TypedEntity()
        {
            //v2: create the relation proxy collection
            RelationProxies = new RelationProxyCollection(this);

            //create the attributes
            Func<TypedAttribute, bool> validateAdd = x =>
            {
                //if (FixedAttributeDefinitionAliases.AllAliases.Contains(x.AttributeDefinition.Alias))
                //    return true;
                // TODO: Better validation here
                if (EntitySchema == null) return true;

                var composite = EntitySchema as CompositeEntitySchema;
                if (composite != null)
                {
                    return composite.AllAttributeDefinitions.Any(y => y.Alias == x.AttributeDefinition.Alias);
                }

                return EntitySchema.AttributeDefinitions.Any(y => y.Alias == x.AttributeDefinition.Alias);
            };
            Attributes = new TypedAttributeCollection(validateAdd);
            Attributes.CollectionChanged += AttributesCollectionChanged;
        }


        private readonly static PropertyInfo AttributesSelector = ExpressionHelper.GetPropertyInfo<TypedEntity, TypedAttributeCollection>(x => x.Attributes);
        void AttributesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(AttributesSelector);
        }

        public TypedAttributeCollection Attributes { get; private set; }
    
        /// <summary>
        /// Gets the attribute groups.
        /// </summary>
        /// <value>The attribute groups.</value>
        public IEnumerable<AttributeGroup> AttributeGroups
        {
            get
            {
                return EntitySchema == null
                           ? Enumerable.Empty<AttributeGroup>()
                           : EntitySchema.AttributeGroups;
            }
        }

        /// <summary>
        /// Gets the attribute schema.
        /// </summary>
        /// <value>The attribute schema.</value>
        public virtual EntitySchema EntitySchema
        {
            get { return _entitySchema; }
            set
            {
                _entitySchema = value;
                OnPropertyChanged(EntitySchemaSelector);
            }
        }
        private EntitySchema _entitySchema;
        private readonly static PropertyInfo EntitySchemaSelector = ExpressionHelper.GetPropertyInfo<TypedEntity, EntitySchema>(x => x.EntitySchema);

        /// <summary>
        /// A store of relation proxies for this entity, to support enlisting relations to this entity.
        /// The relations will not be persisted until the entity is passed to a repository for saving.
        /// If <see cref="RelationProxyCollection.IsConnected"/> is <code>true</code>, this sequence may have
        /// <see cref="RelationProxy"/> objects lazily loaded by enumerating the results of calling <see cref="RelationProxyCollection.LazyLoadDelegate"/>.
        /// </summary>
        /// <value>The relation proxies.</value>
        public RelationProxyCollection RelationProxies { get; private set; }
    }
}