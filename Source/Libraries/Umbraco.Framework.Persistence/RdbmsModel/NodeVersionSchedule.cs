using System;
using System.Collections.Generic;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'NodeVersionSchedule'</summary>
    [Serializable]
    public partial class NodeVersionSchedule : AbstractEquatableObject<NodeVersionSchedule>, IReferenceByGuid
	{
		#region Class Member Declarations
		private NodeVersion _nodeVersion;
		private NodeVersionStatusType _nodeVersionStatusType;
		private System.DateTimeOffset _endDate;
		private System.Guid _id;
		private System.DateTimeOffset _startDate;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="NodeVersionSchedule"/> class.</summary>
		public NodeVersionSchedule() : base()
		{
			OnCreated();
		}

		/// <summary>Method called from the constructor</summary>
		partial void OnCreated();

		#region Class Property Declarations
		/// <summary>Gets or sets the EndDate field. </summary>	
		public virtual System.DateTimeOffset EndDate
		{ 
			get { return _endDate; }
			set { _endDate = value; }
		}

		/// <summary>Gets or sets the Id field. </summary>	
		public virtual System.Guid Id
		{ 
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>Gets or sets the StartDate field. </summary>	
		public virtual System.DateTimeOffset StartDate
		{ 
			get { return _startDate; }
			set { _startDate = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'NodeVersionSchedule.NodeVersion - NodeVersion.NodeVersionSchedules (m:1)'</summary>
		public virtual NodeVersion NodeVersion
		{
			get { return _nodeVersion; }
			set { _nodeVersion = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeVersionSchedule.NodeVersionStatusType - NodeVersionStatusType.NodeVersionSchedules (m:1)'</summary>
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
