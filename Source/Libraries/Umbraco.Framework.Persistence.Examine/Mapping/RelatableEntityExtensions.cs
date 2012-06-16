using System.Linq;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.Examine.Mapping
{
    internal static class RelatableEntityExtensions
    {
        /// <summary>
        /// Maps relations for an IRelatableEntity and all entities attached to those relations to index operations
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="operation"></param>
        /// <param name="engine"></param>
        internal static void MapRelations<T>(this T entity, NestedHiveIndexOperation operation, AbstractFluentMappingEngine engine)
            where T : IRelatableEntity
        {
            //get the relations
            foreach (var r in entity.RelationProxies.AllChildRelations().Union(entity.RelationProxies.AllParentRelations()))
            {
                var relation = engine.Map<IReadonlyRelation<IRelatableEntity, IRelatableEntity>, NestedHiveIndexOperation>(r.Item);

                //we need to process the entities attached to the relation and add them to the sub index ops:

                if (entity.Id != r.Item.SourceId)
                {
                    if (r.Item.Source != null)
                        operation.SubIndexOperations.Add(engine.Map<NestedHiveIndexOperation>(r.Item.Source));
                }
                if (entity.Id != r.Item.DestinationId)
                {
                    if (r.Item.Destination != null)
                        operation.SubIndexOperations.Add(engine.Map<NestedHiveIndexOperation>(r.Item.Destination));
                }

                //now, we need to add the relation itself to the sub index ops:

                operation.SubIndexOperations.Add(relation);
            }
        }

        ///// <summary>
        ///// Maps relations for an IRelatableEntity and all entities attached to those relations to index operations and ensures
        ///// that the relations are added as revisions.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="entity"></param>
        ///// <param name="operation"></param>
        ///// <param name="engine"></param>
        //internal static void MapRelationsForRevisions<T>(this T entity, NestedHiveIndexOperation operation, AbstractMappingEngine engine)
        //    where T : IVersionableEntity, IRelatableEntity
        //{
        //    //get the relations
        //    foreach (var r in entity.RelationProxies.AllChildRelations().Union(entity.RelationProxies.AllParentRelations()))
        //    {
        //        var relation = engine.Map<IReadonlyRelation<IRelatableEntity, IRelatableEntity>, NestedHiveIndexOperation>(r.Item);

        //        //we need to process the entities attached to the relation and add them to the sub index ops:

        //        if (entity.Id != r.Item.SourceId)
        //        {
        //            if (r.Item.Source != null)
        //                operation.SubIndexOperations.Add(engine.Map<NestedHiveIndexOperation>(new Revision<T>((T) r.Item.Source)));
        //        }
        //        if (entity.Id != r.Item.DestinationId)
        //        {
        //            if (r.Item.Destination != null)
        //                operation.SubIndexOperations.Add(engine.Map<NestedHiveIndexOperation>(new Revision<T>((T) r.Item.Destination)));
        //        }

        //        //now, we need to add the relation itself to the sub index ops:

        //        operation.SubIndexOperations.Add(relation);
        //    }
        //}
    }
}