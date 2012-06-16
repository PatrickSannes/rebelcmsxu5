using System.Collections.Generic;
using Umbraco.Cms.Web.LinqSupport;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.ModelFirst;

namespace Umbraco.Cms.Web.Model
{
    using Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata;
    using Umbraco.Framework.Persistence.Model;

    [QueryStructureBinderOfType(typeof(ViewModelStructureBinder))]
    public class Media : CustomTypedEntity<Media>
    {
        public Media()
            : base()
        { }

        public Media(HiveId id, IEnumerable<TypedAttribute> attributes)
        {
            Id = id;
            Attributes.Reset(attributes);
        }

        /// <summary>
        /// Gets the type of the media. This is a alternative accessor for <see cref="EntitySchema"/>
        /// </summary>
        /// <value>The type of the media.</value>
        /// <remarks></remarks>
        public EntitySchema MediaType
        {
            get { return EntitySchema; }
            set { EntitySchema = value; }
        }

        public string Name
        {
            get
            {
                return this.Field<string>(NodeNameAttributeDefinition.AliasValue, "Name");
            }
        }

        public string UrlName
        {
            get
            {
                return this.Field<string>(NodeNameAttributeDefinition.AliasValue, "UrlName");
            }
        }
    }

    [QueryStructureBinderOfType(typeof(ViewModelStructureBinder))]
    public class Content : CustomTypedEntity<Content>, IRoutableItem
    {
        public Content()
            : base()
        { }

        public Content(TypedEntity entity)
            : this(entity.Id, entity.Attributes)
        {
            this.SetupFromEntity(entity);
        }

        public Content(HiveId id, IEnumerable<TypedAttribute> attributes)
        {
            Id = id;
            Attributes.Reset(attributes);
        }

        /// <summary>
        /// Gets the current template.
        /// </summary>
        /// <value>The current template.</value>
        /// <remarks></remarks>
        public Template CurrentTemplate { get; set; }

        /// <summary>
        /// Gets the sort order relative to the content's parent.
        /// </summary>
        /// <value>The sort order.</value>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets the type of the content. This is a alternative accessor for <see cref="EntitySchema"/>
        /// </summary>
        /// <value>The type of the content.</value>
        /// <remarks></remarks>
        public EntitySchema ContentType
        {
            get { return EntitySchema; }
            set { EntitySchema = value; }
        }

        public string Name
        {
            get
            {
                return this.Field<string>(NodeNameAttributeDefinition.AliasValue, "Name");
            }
        }

        public string UrlName
        {
            get
            {
                return this.Field<string>(NodeNameAttributeDefinition.AliasValue, "UrlName");
            }
        }

        /// <summary>
        /// Gets or sets the alternative templates.
        /// </summary>
        /// <value>The alternative templates.</value>
        /// <remarks></remarks>
        public IEnumerable<Template> AlternativeTemplates { get; set; }
    }
}