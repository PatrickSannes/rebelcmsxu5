using System;
using System.Collections.Generic;
using System.Diagnostics;
using Examine;

namespace Umbraco.Framework.Persistence.Examine
{
    /// <summary>
    /// Represents a single index operation
    /// </summary>
    [DebuggerDisplay("LinearHiveIndexOperation Id: {Id} - {OperationType} - {ItemCategory}")]
    public class LinearHiveIndexOperation
    {
        public LinearHiveIndexOperation()
        {
            Fields = new LazyDictionary<string, ItemField>();
        }

        /// <summary>
        /// Associates the entity to the index operation
        /// </summary>
        public object Entity { get; set; }

        /// <summary>
        /// Adds or updates a string field value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void AddOrUpdateField(string key, string val)
        {
            Fields.AddOrUpdate(key, 
                new Lazy<ItemField>(() => new ItemField(val)),
                (k, orig) => new Lazy<ItemField>(() => new ItemField(val)));
        }

        /// <summary>
        /// Adds or updates a string field value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void AddOrUpdateField(string key, Lazy<string> val)
        {
            Fields.AddOrUpdate(key,
                new Lazy<ItemField>(() => new ItemField(val.Value)),
                (k, orig) => new Lazy<ItemField>(() => new ItemField(val.Value)));
        }

        /// <summary>
        /// The fields to store in the index
        /// </summary>
        public LazyDictionary<string, ItemField> Fields { get; set; }
        
        /// <summary>
        /// Lazy loaded id
        /// </summary>
        public Lazy<string> Id { get; set; }
        
        public string ItemCategory { get; set; }
        public IndexOperationType OperationType { get; set; }
    }

    /// <summary>
    /// Represents a hierarichal index operation
    /// </summary>
    public class NestedHiveIndexOperation : LinearHiveIndexOperation
    {
        public NestedHiveIndexOperation()
        {
            SubIndexOperations = new List<NestedHiveIndexOperation>();
        }

        public IList<NestedHiveIndexOperation> SubIndexOperations { get; set; }
    }
}