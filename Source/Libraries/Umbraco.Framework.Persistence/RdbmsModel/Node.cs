using System;
using System.Collections.Generic;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'Node'</summary>
    [Serializable]
    public partial class Node : AbstractEquatableObject<Node>, IReferenceByGuid
	{
		#region Class Member Declarations
		private ICollection<NodeRelation> _incomingRelations;
		private ICollection<NodeRelation> _outgoingRelations;
		private ICollection<NodeRelationCache> _incomingRelationCaches;
		private ICollection<NodeRelationCache> _outgoingRelationCaches;
		private ICollection<NodeVersion> _nodeVersions;
		private System.DateTimeOffset _dateCreated;
		private System.Guid _id;
		private System.Boolean _isDisabled;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="Node"/> class.</summary>
		public Node() : base()
		{
			_incomingRelations = new HashSet<NodeRelation>();
			_outgoingRelations = new HashSet<NodeRelation>();
			_incomingRelationCaches = new HashSet<NodeRelationCache>();
			_outgoingRelationCaches = new HashSet<NodeRelationCache>();
			_nodeVersions = new HashSet<NodeVersion>();
			OnCreated();
		}

		/// <summary>Method called from the constructor</summary>
		partial void OnCreated();

        protected override IEnumerable<System.Reflection.PropertyInfo> GetMembersForEqualityComparison()
        {
            return new[] {this.GetPropertyInfo(x => x.Id)};
        }

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

		/// <summary>Gets or sets the IsDisabled field. </summary>	
		public virtual System.Boolean IsDisabled
		{ 
			get { return _isDisabled; }
			set { _isDisabled = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'NodeRelation.EndNode - Node.IncomingRelations (m:1)'</summary>
		public virtual ICollection<NodeRelation> IncomingRelations
		{
			get { return _incomingRelations; }
			set { _incomingRelations = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeRelation.StartNode - Node.OutgoingRelations (m:1)'</summary>
		public virtual ICollection<NodeRelation> OutgoingRelations
		{
			get { return _outgoingRelations; }
			set { _outgoingRelations = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeRelationCache.EndNode - Node.IncomingNodeRelationCaches (m:1)'</summary>
		public virtual ICollection<NodeRelationCache> IncomingRelationCaches
		{
			get { return _incomingRelationCaches; }
			set { _incomingRelationCaches = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeRelationCache.StartNode - Node.OutgoingRelationCaches (m:1)'</summary>
		public virtual ICollection<NodeRelationCache> OutgoingRelationCaches
		{
			get { return _outgoingRelationCaches; }
			set { _outgoingRelationCaches = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeVersion.Node - Node.NodeVersions (m:1)'</summary>
		public virtual ICollection<NodeVersion> NodeVersions
		{
			get { return _nodeVersions; }
			set { _nodeVersions = value; }
		}
		
		#endregion
	}
}
