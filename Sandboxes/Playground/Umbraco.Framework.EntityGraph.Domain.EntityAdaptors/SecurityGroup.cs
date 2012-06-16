using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoFramework.Models.EntityHub;

namespace UmbracoFramework.Concepts
{
    public class SecurityGroup : IComplexEntity
    {
        #region Implementation of IEntity

        public IEntity Parent { get; set; }
        public IEnumerable<IEntity> Children { get; set; }
        public bool HasChildren { get; set; }
        public EntityStatus Status { get; set; }
        public DateTime UtcCreated { get; set; }
        public DateTime UtcModified { get; set; }
        public DateTime UtcStatusChanged { get; set; }
        public IEntityIdentifier Id { get; set; }
        public IEntityPath Path { get; set; }

        #endregion

        #region Implementation of IComplexEntity

        public IAttributeSchemaDefinition TypeDefinition { get; set; }
        public IEnumerable<ITypedAttribute> FieldValues { get; set; }

        #endregion
    }
}
