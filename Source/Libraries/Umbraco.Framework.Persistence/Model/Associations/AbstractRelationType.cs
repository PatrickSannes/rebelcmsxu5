using System;

namespace Umbraco.Framework.Persistence.Model.Associations
{
    // TODO: Create immutable version of IReferenceByName and apply here
    public abstract class AbstractRelationType
    {
        public abstract string RelationName { get; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return typeof(AbstractRelationType).IsAssignableFrom(obj.GetType()) 
                ? RelationName.Equals(((AbstractRelationType) obj).RelationName) 
                : base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return RelationName == null ? 0 : RelationName.GetHashCode();
        }
    }
}