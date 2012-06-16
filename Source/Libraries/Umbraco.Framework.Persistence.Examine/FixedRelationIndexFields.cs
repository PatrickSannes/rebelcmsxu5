namespace Umbraco.Framework.Persistence.Examine
{
    /// <summary>
    /// Fields for storing data about relations
    /// </summary>
    public static class FixedRelationIndexFields
    {
        /// <summary>
        /// Stores the destination id for a relation
        /// </summary>
        public const string DestinationId = "DestId";

        /// <summary>
        /// Stores the source id for a relation
        /// </summary>
        public const string SourceId = "SourceId";

        /// <summary>
        /// Stores the relation type for a relation
        /// </summary>
        public const string RelationType = "RelationType";

        ///// <summary>
        ///// Stores the Assembly qualified name of the source entity type in a relation
        ///// </summary>
        //public const string RelationSourceType = "SourceEntityType";

        /// <summary>
        /// Prefixes each meta data item of a relation
        /// </summary>
        public const string MetadatumPrefix = "M.";
    }
}