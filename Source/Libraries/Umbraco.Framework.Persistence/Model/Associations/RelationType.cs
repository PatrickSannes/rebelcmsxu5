using System;

namespace Umbraco.Framework.Persistence.Model.Associations
{
    public class RelationType : AbstractRelationType
    {
        private readonly string _relationName;

        public RelationType(string relationName)
        {
            _relationName = relationName;
        }

        #region Overrides of AbstractRelationType
        
        #endregion

        public override string RelationName
        {
            get { return _relationName; }
        }
    }
}