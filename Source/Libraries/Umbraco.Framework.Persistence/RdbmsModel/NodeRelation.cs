using System;
using System.Collections.Generic;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'NodeRelation'</summary>
    [Serializable]
    public partial class NodeRelation : AbstractEquatableObject<NodeRelation>, IReferenceByGuid
	{
		#region Class Member Declarations
		private Node _endNode;
		private Node _startNode;
		private ICollection<NodeRelationCache> _nodeRelationCaches;
		private ICollection<NodeRelationTag> _nodeRelationTags;
		private NodeRelationType _nodeRelationType;
		private System.DateTimeOffset _dateCreated;
		private System.Guid _id;
		private System.Int32 _ordinal;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="NodeRelation"/> class.</summary>
		public NodeRelation() : base()
		{
			_nodeRelationCaches = new HashSet<NodeRelationCache>();
			_nodeRelationTags = new HashSet<NodeRelationTag>();
			OnCreated();
		}

		/// <summary>Method called from the constructor</summary>
		partial void OnCreated();

		#region Class Property Declarations
		/// <summary>Gets or sets the DateCreated field. </summary>	
		public virtual System.DateTimeOffset DateCreated
		{ 
			get { return _dateCreated; }
			set { _dateCreated = value; }
		}

		/// <summary>Gets or sets the Id field. </summary>	
		public virtual System.Guid Id
		{ 
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>Gets or sets the Ordinal field. </summary>	
		public virtual System.Int32 Ordinal
		{ 
			get { return _ordinal; }
			set { _ordinal = value; }
		}

        /// <summary>Represents the navigator which is mapped onto the association 'NodeRelation.EndNode - Node.IncomingRelations (m:1)'</summary>
        public virtual Node EndNode
        {
            get { return _endNode; }
            set { _endNode = value; }
        }

        /// <summary>Represents the navigator which is mapped onto the association 'NodeRelation.StartNode - Node.OutgoingRelations (m:1)'</summary>
        public virtual Node StartNode
        {
            get { return _startNode; }
            set { _startNode = value; }
        }

        //private Guid _endNodeId = Guid.Empty;
        //public virtual Guid EndNodeId
        //{
        //    get { return _endNodeId; }
        //    set { _endNodeId = value; }
        //}

        //private Guid _startNodeId = Guid.Empty;
        //public virtual Guid StartNodeId
        //{
        //    get { return _startNodeId; }
        //    set { _startNodeId = value; }
        //}

	    /// <summary>Represents the navigator which is mapped onto the association 'NodeRelationCache.NodeRelation - NodeRelation.NodeRelationCaches (m:1)'</summary>
		public virtual ICollection<NodeRelationCache> NodeRelationCaches
		{
			get { return _nodeRelationCaches; }
			set { _nodeRelationCaches = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeRelationTag.NodeRelation - NodeRelation.NodeRelationTags (m:1)'</summary>
		public virtual ICollection<NodeRelationTag> NodeRelationTags
		{
			get { return _nodeRelationTags; }
			set { _nodeRelationTags = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeRelation.NodeRelationType - NodeRelationType.NodeRelations (m:1)'</summary>
		public virtual NodeRelationType NodeRelationType
		{
			get { return _nodeRelationType; }
			set { _nodeRelationType = value; }
		}
		
		#endregion

        /// <summary>
        /// Gets the natural id members.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override IEnumerable<System.Reflection.PropertyInfo> GetMembersForEqualityComparison()
        {
            return new[] { this.GetPropertyInfo(x => x.Id) };
        }
	}
}
