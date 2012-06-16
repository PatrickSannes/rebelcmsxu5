using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;

namespace Umbraco.Framework.Persistence.Examine
{
    /// <summary>
    /// Custom indexer for Umbraco
    /// </summary>
    public class UmbracoExamineIndexer : LuceneIndexer
    {
        /// <summary>
        /// Default constructor for use with provider implementation
        /// </summary>
        public UmbracoExamineIndexer()
        {
        }

        /// <summary>
        /// Default constructor for use with provider implementation
        /// </summary>
        /// <param name="indexSetConfig">
        /// All index sets specified to be queried against in order to setup the indexer
        /// </param>
        public UmbracoExamineIndexer(IEnumerable<IndexSet> indexSetConfig)
            : base(indexSetConfig)
        {
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        /// <param name="synchronizationType"></param>
        public UmbracoExamineIndexer(DirectoryInfo workingFolder, Analyzer analyzer, SynchronizationType synchronizationType)
            : base(workingFolder, analyzer, synchronizationType)
        {              
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime with a specified lucene directory
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="synchronizationType"></param>
        /// <param name="luceneDirectory"></param>
        public UmbracoExamineIndexer(Analyzer analyzer, SynchronizationType synchronizationType, Lucene.Net.Store.Directory luceneDirectory)
            : base(analyzer, synchronizationType, luceneDirectory)
        {
        }

        /// <summary>
        /// A type that defines the type of index for each Umbraco field (non user defined fields)
        /// Alot of standard umbraco fields shouldn't be tokenized or even indexed, just stored into lucene
        /// for retreival after searching.
        /// </summary>
        internal static readonly Dictionary<string, FieldIndexTypes> SystemEntityFieldPolicies
            = new Dictionary<string, FieldIndexTypes>()
            {                                
                { FixedIndexedFields.SerializationType, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedIndexedFields.GroupId, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedIndexedFields.AttributeTypeId, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedIndexedFields.SchemaId, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedIndexedFields.SchemaName, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedIndexedFields.SchemaType, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedIndexedFields.SchemaAlias, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedIndexedFields.ParentId, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedIndexedFields.Ordinal, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedIndexedFields.EntityId, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedIndexedFields.UtcCreated, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedIndexedFields.UtcModified, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedIndexedFields.UtcStatusChanged, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                
                { FixedRelationIndexFields.SourceId, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedRelationIndexFields.DestinationId, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedRelationIndexFields.RelationType, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                //{ FixedRelationIndexFields.RelationSourceType, FieldIndexTypes.NO}, //don't index, doesn't require searching and is big                
                
                { FixedRevisionIndexFields.RevisionStatusName, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedRevisionIndexFields.RevisionId, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedRevisionIndexFields.RevisionStatusAlias, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedRevisionIndexFields.RevisionStatusId, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                { FixedRevisionIndexFields.IsLatest, FieldIndexTypes.NOT_ANALYZED_NO_NORMS},
                
            };

        /// <summary>
        /// Determines the indexing policy for the field specified, by default unless this method is overridden, all fields will be "Analyzed"
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="indexCategory"></param>
        /// <returns></returns>
        protected override FieldIndexTypes GetPolicy(string fieldName, string indexCategory)
        {
            indexCategory = indexCategory.ToUpper();

            //NOTE: We pretty much want all fields NOT_ANALYZED so the raw values are stored, then we can setup specific search fields that are analyzed.

            var fieldNameParts = fieldName.Split('.');
            var prefixPart = fieldNameParts[0];
            var namePart = fieldNameParts[fieldNameParts.Length - 1];
            if (prefixPart.StartsWith(SpecialFieldPrefix)
                || prefixPart.StartsWith(FixedRelationIndexFields.MetadatumPrefix.TrimEnd('.'))
                || (prefixPart.StartsWith(FixedAttributeIndexFields.AttributePrefix.TrimEnd('.'))))
            {
                return FieldIndexTypes.NOT_ANALYZED_NO_NORMS;
            }
            
            //anything ending with Alias, is not analyzed but allow Norms so alias fields could be boosted, etc...
            if (namePart.Equals("Alias", StringComparison.InvariantCultureIgnoreCase))
            {
                return FieldIndexTypes.NOT_ANALYZED;
            }

            //any DateTimeOffset field is not indexed, only stored
            if (namePart.EndsWith(FixedIndexedFields.DateTimeOffsetSuffix, StringComparison.InvariantCultureIgnoreCase))
            {
                return FieldIndexTypes.NO;
            }

            var def = SystemEntityFieldPolicies.Where(x => x.Key == fieldName).ToArray();
            return def.Any()
                       ? def.Single().Value
                       : base.GetPolicy(fieldName, indexCategory);
        }

    }
}