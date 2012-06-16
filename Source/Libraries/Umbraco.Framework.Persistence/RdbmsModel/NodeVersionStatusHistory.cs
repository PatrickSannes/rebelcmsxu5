using System;
using System.Collections.Generic;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'NodeVersionStatusHistory'</summary>
    [Serializable]
    public partial class NodeVersionStatusHistory : AbstractEquatableObject<NodeVersionStatusHistory>, IReferenceByGuid
	{
		#region Class Member Declarations
		private NodeVersion _nodeVersion;
		private NodeVersionStatusType _nodeVersionStatusType;
		private System.DateTimeOffset _date;
		private System.Guid _id;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="NodeVersionStatusHistory"/> class.</summary>
		public NodeVersionStatusHistory() : base()
		{
			OnCreated();
		}

		/// <summary>Method called from the constructor</summary>
		partial void OnCreated();

		#region Class Property Declarations
		/// <summary>Gets or sets the Date field. </summary>	
		public virtual System.DateTimeOffset Date
		{ 
			get { return _date; }
			set { _date = value; }
		}

		/// <summary>Gets or sets the Id field. </summary>	
		public virtual System.Guid Id
		{ 
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'NodeVersionStatusHistory.NodeVersion - NodeVersion.NodeVersionStatuses (m:1)'</summary>
		public virtual NodeVersion NodeVersion
		{
			get { return _nodeVersion; }
			set { _nodeVersion = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeVersionStatusHistory.NodeVersionStatusType - NodeVersionStatusType.NodeVersionStatuses (m:1)'</summary>
		public virtual NodeVersionStatusType NodeVersionStatusType
		{
			get { return _nodeVersionStatusType; }
			set { _nodeVersionStatusType = value; }
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
