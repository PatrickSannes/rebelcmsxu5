using System;
using System.Collections.Generic;
using System.Reflection;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'NodeVersion'</summary>
    [Serializable]
    public partial class NodeVersion : AbstractEquatableObject<NodeVersion>, IReferenceByGuid
	{
		#region Class Member Declarations
		private ICollection<Attribute> _attributes;
		private Node _node;
		private ICollection<NodeVersionSchedule> _nodeVersionSchedules;
		private ICollection<NodeVersionStatusHistory> _nodeVersionStatuses;
		private System.DateTimeOffset _dateCreated;
		private System.String _defaultName;
		private System.Guid _id;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="NodeVersion"/> class.</summary>
		public NodeVersion() : base()
		{
			_attributes = new HashSet<Attribute>();
			_nodeVersionSchedules = new HashSet<NodeVersionSchedule>();
			_nodeVersionStatuses = new HashSet<NodeVersionStatusHistory>();
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

		/// <summary>Gets or sets the DefaultName field. </summary>	
		public virtual System.String DefaultName
		{ 
			get { return _defaultName; }
			set { _defaultName = value; }
		}

		/// <summary>Gets or sets the Id field. </summary>	
		public virtual System.Guid Id
		{ 
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'Attribute.NodeVersion - NodeVersion.Attributes (m:1)'</summary>
		public virtual ICollection<Attribute> Attributes
		{
			get { return _attributes; }
			set { _attributes = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'NodeVersion.Node - Node.NodeVersions (m:1)'</summary>
		public virtual Node Node
		{
			get { return _node; }
			set { _node = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeVersionSchedule.NodeVersion - NodeVersion.NodeVersionSchedules (m:1)'</summary>
		public virtual ICollection<NodeVersionSchedule> NodeVersionSchedules
		{
			get { return _nodeVersionSchedules; }
			set { _nodeVersionSchedules = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeVersionStatusHistory.NodeVersion - NodeVersion.NodeVersionStatuses (m:1)'</summary>
		public virtual ICollection<NodeVersionStatusHistory> NodeVersionStatuses
		{
			get { return _nodeVersionStatuses; }
			set { _nodeVersionStatuses = value; }
		}
		
		#endregion

	    private AttributeSchemaDefinition _attributeSchemaDefinition;
        /// <summary>
        /// Represents the navigator which is mapped onto the association 'NodeVersion.AttributeSchemaDefinition - AttributeSchemaDefinition.NodeVersion (1:m)'
        /// </summary>
	    public virtual AttributeSchemaDefinition AttributeSchemaDefinition
	    {
	        get { return _attributeSchemaDefinition; }
	        set { _attributeSchemaDefinition = value; }
	    }

	    #region Overrides of AbstractEquatableObject<NodeVersion>

	    /// <summary>
	    /// Gets the natural id members.
	    /// </summary>
	    /// <returns></returns>
	    /// <remarks></remarks>
	    protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
	    {
	        return new[] {this.GetPropertyInfo(x => x.Id)};
	    }

	    #endregion
	}
}
