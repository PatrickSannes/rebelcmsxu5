using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.Model.LinqSupport;
using Umbraco.Framework.Persistence.ModelFirst;
using Umbraco.Framework.Persistence.ModelFirst.Annotations;

namespace Umbraco.Framework.Persistence.Model.Constants.Entities
{
    using Umbraco.Framework.Linq;

    using Umbraco.Framework.Linq.CriteriaGeneration.StructureMetadata;

    [DefaultSchemaForQuerying(SchemaAlias = UserGroupSchema.SchemaAlias)]
    [QueryStructureBinderOfType(typeof(AnnotationQueryStructureBinder))]
    public class UserGroup : CustomTypedEntity<UserGroup>
    {
        public UserGroup()
        {
            this.SetupFromSchema<UserGroupSchema>();

            //A user group is always under the User Group virtual root
            this.RelationProxies.EnlistParent(FixedEntities.UserGroupVirtualRoot, FixedRelationTypes.DefaultRelationType);
        }

        //public string Name
        //{
        //    get { return Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue; }
        //    set { Attributes[NodeNameAttributeDefinition.AliasValue].DynamicValue = value; }
        //}

        [AttributeAlias(Alias = NodeNameAttributeDefinition.AliasValue)]
        public string Name
        {
            get { return base.BaseAutoGet(x => x.Name); }
            set { base.BaseAutoSet(x => x.Name, value); }
        }

    }
}
