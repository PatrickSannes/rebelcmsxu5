using System;
using Umbraco.Framework;
using Umbraco.Framework.Data.Common;

namespace Umbraco.Cms.Web.Model
{
    public abstract class Node
    {
        protected Node() : this(new BasicConcurrencyToken(), HiveId.Empty)
        {
            UtcCreated = UtcModified = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class. This constructor is designed to be used for helping an instance verify whether the related
        /// data entity has changed between population and re-saving, if ever attempted.
        /// </summary>
        /// <param name="concurrencyToken">The concurrency token.</param>
        /// <param name="id">The id.</param>
        protected Node(IConcurrencyToken concurrencyToken, HiveId id) : this(id)
        {
            ConcurrencyToken = concurrencyToken;
            Id = id;
        }

        protected Node(HiveId id)
        {
            Id = id;
        }

        protected IConcurrencyToken ConcurrencyToken { get; set; }
        public HiveId Id { get; set; }
        public DateTimeOffset UtcCreated { get; set; }
        public DateTimeOffset UtcModified { get; set; }
        public virtual string NodeType { get { return GetType().Name; } }
    }
}