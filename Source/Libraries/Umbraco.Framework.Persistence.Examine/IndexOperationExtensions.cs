using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Framework.Persistence.Model.Versioning;

namespace Umbraco.Framework.Persistence.Examine
{
    internal static class LinearHiveOperationExtensions
    {
        /// <summary>
        /// Checks if the Entity assigned to the operation is a Revision{T}
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        internal static bool IsRevision(this LinearHiveIndexOperation op)
        {
            return op.OperationType == IndexOperationType.Add
                   && op.Entity != null
                   && op.Entity.GetType().IsOfGenericType(typeof (Revision<>));
        }

        /// <summary>
        /// Checks if the operation is for a "Relation"
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        internal static bool IsRelation(this LinearHiveIndexOperation op)
        {
            return op.OperationType == IndexOperationType.Add                  
                   && op.ItemCategory == "Relation";
        }

        /// <summary>
        /// Checks if the entities are IReferenceByHiveId and of the same type if so returns true if the Ids match, otherwise false.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="objectToCompare"></param>
        /// <returns></returns>
        internal static bool EntitiesAreEqual(this LinearHiveIndexOperation op, object objectToCompare)
        {
            if (ReferenceEquals(op.Entity, objectToCompare)) return true;

            var entityToCompare = objectToCompare as IReferenceByHiveId;
            if (entityToCompare == null) return false;
            var entity = op.Entity as IReferenceByHiveId;
            if (entity == null) return false;
            if (entity.GetType() != objectToCompare.GetType()) return false;
            if (entityToCompare.Id.IsNullValueOrEmpty() || entity.Id.IsNullValueOrEmpty()) return false;
            return entity.Id.Value.Equals(entityToCompare.Id.Value);
        }

        /// <summary>
        /// Detects if the Entity of the operation already exists in the queue list, if it does and it has been changed
        /// then this removes that operation from the queue list and returns true so that the newer version is added to the
        /// end of the list. If the item already exists in the queue but the entity has not been changed then it leaves the 
        /// original in the queue list. If the Entity doesn't exist in the queue then returns true so that it's added.
        /// </summary>
        /// <param name="indexOps"></param>
        /// <param name="op"></param>
        /// <returns>
        /// Returns true if the operation is a Delete operation.
        ///  then Returns false if the Entity is null.
        ///  then Returns false if the item was found in the queue list.
        ///  then Returns false if the item was not found in the queue list but has not been changed (not IsDirty())
        ///  then Returns true if the item was not found in the queue list and either has been changed or does not implement ICanBeDirty.
        /// </returns>
        internal static bool ShouldAddNewQueueItem(this List<LinearHiveIndexOperation> indexOps, LinearHiveIndexOperation op)
        {
            //always add delete ops
            if (op.OperationType == IndexOperationType.Delete) 
                return true;

            //if its not a Delete, the Entity should never be null
            if (op.Entity == null) 
                return false;

            //if (op.IsRevision())
            //{
            //    return true;   
            //}
                

            //checks if the operation exists by reference or by id
            var existingOperation = indexOps.Where(x => op.OperationType == IndexOperationType.Add
                                                        && (op.EntitiesAreEqual(x.Entity))).SingleOrDefault();
            
            //the item already exists
            if (existingOperation != null)
            {
                var dirtyEntity = op.Entity as ICanBeDirty;
                
                //if the item has been updated then remove the original entry from the queue list so the new version can be
                //re-added .
                if (dirtyEntity != null && dirtyEntity.IsDirty())
                {
                    indexOps.Remove(existingOperation);                    
                    return true;
                }
                //the item hasn't changed so leave the original
                return false;
            }

            return true;
        }

    }
}