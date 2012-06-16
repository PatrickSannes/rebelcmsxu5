using System;
using System.Collections.Generic;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'NodeRelationType'</summary>
    [Serializable]
    public partial class NodeRelationType : AbstractEquatableObject<NodeRelationType>, IReferenceByGuid, IReferenceByAlias
	{
		#region Class Member Declarations
		private ICollection<NodeRelation> _nodeRelations;
		private System.String _alias;
		private System.String _description;
		private System.Guid _id;
		private System.String _name;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="NodeRelationType"/> class.</summary>
		public NodeRelationType() : base()
		{
			_nodeRelations = new HashSet<NodeRelation>();
			OnCreated();
		}

		/// <summary>Method called from the constructor</summary>
		partial void OnCreated();		

		#region Class Property Declarations
		/// <summary>Gets or sets the Alias field. </summary>	
		public virtual System.String Alias
		{ 
			get { return _alias; }
			set { _alias = value; }
		}

		/// <summary>Gets or sets the Description field. </summary>	
		public virtual System.String Description
		{ 
			get { return _description; }
			set { _description = value; }
		}

		/// <summary>Gets or sets the Id field. </summary>	
		public virtual System.Guid Id
		{ 
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>Gets or sets the Name field. </summary>	
		public virtual System.String Name
		{ 
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'NodeRelation.NodeRelationType - NodeRelationType.NodeRelations (m:1)'</summary>
		public virtual ICollection<NodeRelation> NodeRelations
		{
			get { return _nodeRelations; }
			set { _nodeRelations = value; }
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
