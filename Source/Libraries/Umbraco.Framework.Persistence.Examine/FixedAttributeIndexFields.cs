using System;
using Examine;
using Umbraco.Framework.Persistence.Model.Attribution;

namespace Umbraco.Framework.Persistence.Examine
{
    /// <summary>
    /// Used to name attribute fields in a TypedEntity document since we flatten all attributes into the same document
    /// </summary>
    public static class FixedAttributeIndexFields
    {
        public const string AttributePrefix = "Attr.";
        public const string AttributeAlias = "__Alias";
        //public const string AttributeName = "__Name";
        public const string AttributeId = "__Id";


        /// <summary>
        /// Creates the attribute alias field.
        /// </summary>
        /// <param name="attributeDefAlias">The attribute def alias.</param>
        /// <returns></returns>
        public static string CreateAttributeAliasField(string attributeDefAlias)
        {
            return AttributePrefix + attributeDefAlias + "." + AttributeAlias;
        }

        /// <summary>
        /// Creates the attribute id field.
        /// </summary>
        /// <param name="attributeDefAlias">The attribute def alias.</param>
        /// <returns></returns>
        public static string CreateAttributeIdField(string attributeDefAlias)
        {
            return AttributePrefix + attributeDefAlias + "." + AttributeId;
        }

        /// <summary>
        /// Adds the attribute alias.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="ta">The ta.</param>
        public static void AddAttributeAlias(LazyDictionary<string, ItemField> d, TypedAttribute ta)
        {
            d.Add(CreateAttributeAliasField(ta.AttributeDefinition.Alias),
                new ItemField(ta.AttributeDefinition.Alias));
        }

        //public static void AddAttributeName(LazyDictionary<string, ItemField> d, TypedAttribute ta)
        //{
        //    d.Add(
        //        AttributePrefix + ta.AttributeDefinition.Alias + "." + AttributeName,
        //        new ItemField(ta.AttributeDefinition.Name));
        //}

        public static void AddAttributeId(LazyDictionary<string, ItemField> d, TypedAttribute ta)
        {
            d.Add(CreateAttributeIdField(ta.AttributeDefinition.Alias),
                new Lazy<ItemField>(() => new ItemField(ta.Id.Value.ToString()))); //lazy load this id as it might not be set
        }
    }
}