using System.Collections.Generic;

namespace Umbraco.Framework.Persistence.Model.Associations._Revised
{
    public class RelationProxyBucket
    {
        public RelationProxyBucket()
        {
            Parents = new HashSet<RelationById>();
            Children = new HashSet<RelationById>();
        }

        public RelationProxyBucket(IEnumerable<RelationById> parents, IEnumerable<RelationById> children)
        {
            Parents = parents;
            Children = children;
        }

        public IEnumerable<RelationById> Parents { get; protected set; } 
        public IEnumerable<RelationById> Children { get; protected set; } 
    }
}