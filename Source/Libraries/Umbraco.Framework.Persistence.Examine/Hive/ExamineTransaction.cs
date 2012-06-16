using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Hive.ProviderSupport;

namespace Umbraco.Framework.Persistence.Examine.Hive
{

    /// <summary>
    /// A transaction class used to commit information into the Examine index
    /// </summary>
    /// <remarks>>
    /// This class is not accessed across threads, a new transaction is created for each unit of work.
    /// </remarks>
    public class ExamineTransaction : AbstractProviderTransaction
    {
        private readonly ProviderMetadata _providerMetadata;
        private readonly IFrameworkContext _frameworkContext;
        protected ExamineManager ExamineManager { get; private set; }

        /// <summary>
        /// Useful for debugging purposes
        /// </summary>
        public Guid TransactionId
        {
            get { return _transactionId; }
        }

        private readonly Guid _transactionId = Guid.NewGuid();

        public ExamineTransaction(ExamineManager examineManager, ProviderMetadata providerMetadata, IFrameworkContext frameworkContext)
        {
            _providerMetadata = providerMetadata;
            _frameworkContext = frameworkContext;
            ExamineManager = examineManager;
            //create a new queue
            _itemsToIndex = new List<LinearHiveIndexOperation>();
        }

        /// <summary>
        /// Finds all schema associations (AttributeGroups and AttributeDefinitions) that already exist in the queue for the 
        /// specified schema and removes them.
        /// </summary>
        /// <param name="schema"></param>
        internal void RemoveSchemaAssociations(EntitySchema schema)
        {
            //find all associations previously added to the transaction queue
            // look for any entity that is an AttributeGroup or AttributeDefinition that has a schema Id the same as the id of the schema passed in.
            var toRemove = _itemsToIndex
                .Where(x => ((x.Entity is AttributeGroup || x.Entity is AttributeDefinition)
                            && (x.Fields[FixedIndexedFields.SchemaId].FieldValue.ToString() == schema.Id.Value.ToString())))
                .ToArray();
            foreach (var r in toRemove)
            {
                _itemsToIndex.Remove(r);
            }
        }

        /// <summary>
        /// Adds an index operation to the Queue, if the item already exists it is ignored.
        /// </summary>
        /// <param name="op"></param>
        public void EnqueueIndexOperation(LinearHiveIndexOperation op)
        {
            if (!_itemsToIndex.ShouldAddNewQueueItem(op))
                return;

            //before we process each item, we need to update all of the item ids if they haven't been set
            if (op.OperationType == IndexOperationType.Add && op.Entity != null)
            {
                //check if we should be creating Ids
                if (!_providerMetadata.IsPassthroughProvider)
                {
                    ExamineHelper.EnsureIds(op);    
                }
            }
            
            _itemsToIndex.Add(op);
        }

        private readonly List<LinearHiveIndexOperation> _itemsToIndex;

        protected override bool PerformExplicitRollback()
        {
            try
            {
                _itemsToIndex.Clear();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override bool PerformImplicitRollback()
        {
            try
            {
                _itemsToIndex.Clear();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal void ExecuteCommit()
        {
            //need to send Examine all of the stuff to index for this unit of work

            //we need to 'stagger' the operations as it will be much faster to send all of the "Adds" to Examine at once and then each "Delete" one at a time.
            //because of the way Examine works, we also need to 'stagger' the index categories as well

            using (DisposableTimer.TraceDuration<ExamineTransaction>("Start adding indexes to Examine queue", "End adding indexes to Examine queue"))
            {
                var examineOperations = new List<IndexOperation>();

                //we track the TypedEntity revisions committed for 2 reasons (we're tracking the TypedEntity item in the revision): 
                //  1: see below notes
                //  2: we need to update the status changed date if the status has in fact 'changed' since we need to reference in-memory objects as well
                var revisionsCommitted = new List<Tuple<TypedEntity, RevisionData>>();

                //this will get all of the lazy ids generated so we can use the values
                // to lookup some entities.
                var ids = _itemsToIndex.Where(x => x.OperationType == IndexOperationType.Add).Select(x => x.Id.Value)
                    .Concat(_itemsToIndex
                                .Where(x => x.Fields.ContainsKey(FixedIndexedFields.EntityId) && x.OperationType == IndexOperationType.Add)
                                .Select(x => x.Fields[FixedIndexedFields.EntityId].FieldValue.ToString()))
                    .Distinct()
                    .ToArray();

                while (_itemsToIndex.Any())
                {
                    var op = _itemsToIndex.Last();

                    examineOperations.Add(_frameworkContext.TypeMappers.Map<IndexOperation>(op));

                    //if its a typed entity revision:
                    // - track it so we can perform index cleanup on revisions
                    // - check status changed dates so we can ensure they are correct
                    if (op.IsRevision())
                    {
                        revisionsCommitted.Add(
                            EnsureCorrectStatusChangedDate(op, revisionsCommitted));
                    }

                    EnsureRelationEntities(op, ids);

                    //'dequeue' the item from the list
                    _itemsToIndex.RemoveAt(_itemsToIndex.Count - 1);
                }

                //now we have all of the indexing operations ready, lets do it
                ExamineManager.PerformIndexing(examineOperations.ToArray());

                //do some cleanup of the index data
                CleanupIndexedRevisions(revisionsCommitted);
            }
        }

        protected override bool PerformCommit()
        {
            ExecuteCommit();

            return true;
        }

        public override bool IsTransactional
        {
            get { return true; }
        }

        /// <summary>
        /// Looks up a TypedEntity or EntitySchema Id in the queue or the index based on the id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="idsinQueue">The ids already in the queue</param>
        /// <returns></returns>
        private string GetItem(string id, IEnumerable<string> idsinQueue)
        {
            var foundInQueue = idsinQueue.SingleOrDefault(x => x == id);
            if (foundInQueue != null)
                return foundInQueue;

            var typedEntity = ExamineManager.Search(
                ExamineManager.CreateSearchCriteria()
                    .Must().EntityType<TypedEntity>()
                    .Must().Id(id, FixedIndexedFields.EntityId).Compile());
            if (typedEntity.Any())
                return typedEntity.Last().Fields[FixedIndexedFields.EntityId];
            var schema = ExamineManager.Search(
                ExamineManager.CreateSearchCriteria()
                    .Must().EntityType<EntitySchema>()
                    .Must().Id(id).Compile());
            if (schema.Any())
                return schema.Last().Id;
            return null;
        }

        /// <summary>
        /// This makes sure that if the operatoin is for a Relation, that the source and destination items for the relation exist already 
        /// in either the queue or the physical index itself.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="idsinQueue"></param>
        private void EnsureRelationEntities(LinearHiveIndexOperation op, string[] idsinQueue)
        {
            if (op.IsRelation())
            {
                // Get the source and destination items from the Nh session
                var sourceNode = GetItem(op.Fields[FixedRelationIndexFields.SourceId].FieldValue.ToString(), idsinQueue);
                var destNode = GetItem(op.Fields[FixedRelationIndexFields.DestinationId].FieldValue.ToString(), idsinQueue);

                // Check the Nh session is already aware of the items
                if (sourceNode == null || destNode == null)
                {
                    string extraMessage = string.Empty;
                    if (sourceNode == null) extraMessage = "Source {0} cannot be found.\n".InvariantFormat(op.Fields[FixedRelationIndexFields.SourceId].FieldValue);
                    if (destNode == null) extraMessage += "Destination {0} cannot be found.".InvariantFormat(op.Fields[FixedRelationIndexFields.DestinationId].FieldValue);
                    throw new InvalidOperationException(
                        "Before adding a relation between source {0} and destination {1}, you must call AddOrUpdate with those items or they must already exist in the datastore.\n{2}"
                            .InvariantFormat(op.Fields[FixedRelationIndexFields.SourceId].FieldValue, op.Fields[FixedRelationIndexFields.DestinationId].FieldValue, extraMessage));
                }
            }
        }

        /// <summary>
        /// Need to do a bit of cleanup now for Revision entries to ensure that we keep the records flagged as IsLatest to a minimum.
        /// To do this we'll lookup all revisions in the index that are marked as IsLatest that have Ids of revisions that we've just committed
        /// that have a UtcModified date of less than the ones we've just committed... thankfully, Lucene has a Range query that we can 
        /// use to do this cleanup
        /// </summary>
        /// <param name="revisionsCommitted">The revisions committed during this transaction</param>
        private void CleanupIndexedRevisions(IEnumerable<Tuple<TypedEntity, RevisionData>> revisionsCommitted)
        {           
            foreach (var r in revisionsCommitted)
            {
                //TODO: Figure out how Lucene is dealing with DateTime and UTC... currently were putting it back a day
                // but if we can do an Hour that would be better, just need to figure out what it is storing.
                var criteria = ExamineManager.CreateSearchCriteria()
                    .Must().EntityType<TypedEntity>()
                    .Must().HiveId(r.Item1.Id, FixedIndexedFields.EntityId)
                    .Must().Field(FixedRevisionIndexFields.IsLatest, "1".Escape())
                    .Must().Range(FixedIndexedFields.UtcModified, DateTime.MinValue, r.Item1.UtcModified.UtcDateTime.AddDays(-1));

                foreach (var i in ExamineManager.Search(criteria.Compile()))
                {
                    //convert the fields returned
                    var fields = i.Fields.ToDictionary(f => f.Key, f => new ItemField(f.Value));

                    //remove the flag
                    fields[FixedRevisionIndexFields.IsLatest] = new ItemField(0) { DataType = FieldDataType.Int };

                    //now we need to update the index item to not be latest
                    var updateOp = new IndexOperation
                    {
                        Operation = IndexOperationType.Add,
                        Item = new IndexItem
                        {
                            Fields = fields,
                            Id = i.Id,
                            ItemCategory = i.Fields[LuceneIndexer.IndexCategoryFieldName]
                        }
                    };

                    //do the update
                    ExamineManager.PerformIndexing(updateOp);
                }

            }
        }

        /// <summary>
        /// When the Entity is a Revision, this checks the previous revisions committed during this transaction to see if the status has actually changed,
        /// if it determines that no previous entry exists in memory for this transaction, it will look up the entity in the index to see if the 
        /// status has changed. It then sets the status changed date accordingly on the TypedEntity and in the index fields.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="revisionsCommitted"></param>
        /// <returns>Returns a Tuple of the updated TypedEntity and RevisionData</returns>
        private Tuple<TypedEntity, RevisionData> EnsureCorrectStatusChangedDate(LinearHiveIndexOperation op, IEnumerable<Tuple<TypedEntity, RevisionData>> revisionsCommitted)
        {            
            dynamic r = op.Entity;
            TypedEntity te = r.Item;
            RevisionData rd = r.MetaData;

            //find all previous TypedEntities in the committed list matching this one
            var previous = revisionsCommitted.Where(x => x.Item1.Id.Value.ToString() == te.Id.Value.ToString())
                .OrderBy(x => x.Item1.UtcModified)
                .LastOrDefault();
            SearchResult latestEntry = GetLatestEntry(r);

            if (previous == null && latestEntry != null && latestEntry.Fields.ContainsKey(FixedRevisionIndexFields.RevisionStatusId) && latestEntry.Fields[FixedRevisionIndexFields.RevisionStatusId] != rd.StatusType.Id.Value.ToString())
            {
                //if there's nothing in memory but there's a previously saved entry with a different status id, then update the date
                te.UtcStatusChanged = rd.UtcCreated;
                op.Fields[FixedIndexedFields.UtcStatusChanged] = new ItemField(te.UtcStatusChanged.UtcDateTime) { DataType = FieldDataType.DateTime };
            }
            else if (previous != null && previous.Item2.StatusType.Id.Value.ToString() != rd.StatusType.Id.Value.ToString())
            {
                //its changed in memory so update the date
                te.UtcStatusChanged = rd.UtcCreated;
                op.Fields[FixedIndexedFields.UtcStatusChanged] = new ItemField(te.UtcStatusChanged.UtcDateTime) { DataType = FieldDataType.DateTime };
            }
            else if (latestEntry != null)
            {
                //the status hasn't changed and the entity is not new, set to latest entries status changed
                te.UtcStatusChanged = ExamineHelper.FromExamineDateTime(latestEntry.Fields, FixedIndexedFields.UtcStatusChanged).Value;
                op.Fields[FixedIndexedFields.UtcStatusChanged] = new ItemField(te.UtcStatusChanged.UtcDateTime) { DataType = FieldDataType.DateTime };
            }
            return new Tuple<TypedEntity, RevisionData>(te, rd);
        }

        /// <summary>
        /// Determines if the status is changing for the entity passed in
        /// </summary>
        /// <param name="rev"></param>
        /// <returns></returns>
        private SearchResult GetLatestEntry<T>(Revision<T> rev)
            where T : class, IVersionableEntity
        {
            //get all latest versions of the typed entity
            var found = ExamineManager.Search(
                ExamineManager.CreateSearchCriteria()
                    .Must().EntityType<TypedEntity>()
                    .Must().HiveId(rev.Item.Id, FixedIndexedFields.EntityId)
                    .Must().Range(FixedRevisionIndexFields.IsLatest, 1, 1)
                    .Compile());

            //if nothing found, then its brand new
            if (!found.Any())
                return null;

            //get the very latest version
            var latest = found.OrderBy(x => x.Fields[FixedIndexedFields.UtcModified]).Last();

            //return the latest entry
            return latest;
        }
    }
}