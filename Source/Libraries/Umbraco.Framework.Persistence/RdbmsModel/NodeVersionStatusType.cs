using System;
using System.Collections.Generic;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'NodeVersionStatusType'</summary>
    [Serializable]
    public partial class NodeVersionStatusType : AbstractEquatableObject<NodeVersionStatusType>, IReferenceByGuid, IReferenceByAlias
	{
		#region Class Member Declarations
		private ICollection<NodeVersionSchedule> _nodeVersionSchedules;
		private ICollection<NodeVersionStatusHistory> _nodeVersionStatuses;
		private System.Guid _id;
		private System.Boolean _isSystem;
        private System.String _name;
        private System.String _alias;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="NodeVersionStatusType"/> class.</summary>
		public NodeVersionStatusType() : base()
		{
			_nodeVersionSchedules = new HashSet<NodeVersionSchedule>();
			_nodeVersionStatuses = new HashSet<NodeVersionStatusHistory>();
			OnCreated();
		}

		/// <summary>Method called from the constructor</summary>
		partial void OnCreated();

		#region Class Property Declarations
		/// <summary>Gets or sets the Id field. </summary>	
		public virtual System.Guid Id
		{ 
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>Gets or sets the IsSystem field. </summary>	
		public virtual System.Boolean IsSystem
		{ 
			get { return _isSystem; }
			set { _isSystem = value; }
		}

        /// <summary>Gets or sets the Alias field. </summary>	
        public virtual System.String Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

		/// <summary>Gets or sets the Name field. </summary>	
		public virtual System.String Name
		{ 
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'NodeVersionSchedule.NodeVersionStatusType - NodeVersionStatusType.NodeVersionSchedules (m:1)'</summary>
		public virtual ICollection<NodeVersionSchedule> NodeVersionSchedules
		{
			get { return _nodeVersionSchedules; }
			set { _nodeVersionSchedules = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeVersionStatusHistory.NodeVersionStatusType - NodeVersionStatusType.NodeVersionStatuses (m:1)'</summary>
		public virtual ICollection<NodeVersionStatusHistory> NodeVersionStatuses
		{
			get { return _nodeVersionStatuses; }
			set { _nodeVersionStatuses = value; }
		}
		
		#endregion

        /// <summary>
        /// Gets the natural id members.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override IEnumerable<System.Reflection.PropertyInfo> GetMembersForEqualityComparison()
        {
            return new[] { this.GetPropertyInfo(x => x.Id), this.GetPropertyInfo(x => x.Alias) };
        }
	}
}
