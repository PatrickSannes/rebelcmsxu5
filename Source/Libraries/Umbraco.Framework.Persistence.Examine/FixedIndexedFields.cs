namespace Umbraco.Framework.Persistence.Examine
{
    /// <summary>
    /// Standard index fields
    /// </summary>
    public static class FixedIndexedFields
    {

        /// <summary>
        /// This isn't directly used to create the index but is the field name for the Alias of an AttributeDefinition and others.
        /// </summary>
        public const string Alias = "Alias";

        public const string DateTimeOffsetSuffix = "_o";

        public const string UtcCreated = "UtcCreated";
        public const string UtcModified = "UtcModified";
        public const string UtcStatusChanged = "UtcStatusChanged";

        public const string Ordinal = "Ordinal";
        public const string SchemaId = "SchemaId";
        public const string SchemaName = "SchemaName";
        public const string SchemaAlias = "SchemaAlias";
        public const string ParentId = "ParentId";
        public const string SchemaType = "SchemaType";

        /// <summary>
        /// Used to store the id of the TypedEntity
        /// </summary>
        /// <remarks>
        /// Because Examine requires that the __NodeId field is unique, a TypedEntity will create a __NodeId as a composite key = EntityId + RevisionId
        /// </remarks>
        public const string EntityId = "EntityId";
        
        /// <summary>
        /// Used to store the attribute type id
        /// </summary>
        public const string AttributeTypeId = "AttTypeId";

        /// <summary>
        /// Used to store the attribute group id
        /// </summary>
        public const string GroupId = "GroupId";

        /// <summary>
        /// Stores the serialization type for an attribute
        /// </summary>
        public const string SerializationType = "SerializationType";
    }
}