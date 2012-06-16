using System;
using System.Collections.Generic;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'NodeRelationTag'</summary>
    [Serializable]
    public partial class NodeRelationTag : AbstractEquatableObject<NodeRelationTag>, IReferenceByGuid
	{
		#region Class Member Declarations
		private NodeRelation _nodeRelation;
		private System.Guid _id;
		private System.String _name;
		private System.String _value;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="NodeRelationTag"/> class.</summary>
		public NodeRelationTag() : base()
		{
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

		/// <summary>Gets or sets the Name field. </summary>	
		public virtual System.String Name
		{ 
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>Gets or sets the Value field. </summary>	
		public virtual System.String Value
		{ 
			get { return _value; }
			set { _value = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'NodeRelationTag.NodeRelation - NodeRelation.NodeRelationTags (m:1)'</summary>
		public virtual NodeRelation NodeRelation
		{
			get { return _nodeRelation; }
			set { _nodeRelation = value; }
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
