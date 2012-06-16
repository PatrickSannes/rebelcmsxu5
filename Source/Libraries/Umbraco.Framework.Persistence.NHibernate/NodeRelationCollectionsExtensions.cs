using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate
{
    public static class NodeRelationCollectionsExtensions
    {
        /// <summary>
        /// This will check if the relation passed in already exists in the source, if so it updates it Ordinal value
        /// </summary>
        /// <param name="relations"></param>
        /// <param name="r"></param>
        public static void UpdateOrdinal(this ICollection<NodeRelation> relations, NodeRelation r)
        {
            //check by id, if not (since the r.id is empty), then match by the composite key of a relation
            var found = relations.SingleOrDefault(x => x.Id == r.Id) 
                ?? relations.SingleOrDefault(x => x.StartNode.Id == r.StartNode.Id && x.EndNode.Id == r.EndNode.Id && x.NodeRelationType.Name == r.NodeRelationType.Name);
            if (found != null)
            {                
                found.Ordinal = r.Ordinal;
            }
        }

    }
}
