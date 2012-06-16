using System;
using System.Collections.Generic;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'NodeRelationCache'</summary>
    [Serializable]
    public partial class NodeRelationCache : AbstractEquatableObject<NodeRelationCache>, IReferenceByGuid
	{
		#region Class Member Declarations
		private Node _endNode;
		private Node _startNode;
		private NodeRelation _nodeRelation;
		private System.Int32 _distanceFromOriginal;
		private System.Guid _id;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="NodeRelationCache"/> class.</summary>
		public NodeRelationCache() : base()
		{
			OnCreated();
		}

		/// <summary>Method called from the constructor</summary>
		partial void OnCreated();

		#region Class Property Declarations
		/// <summary>Gets or sets the DistanceFromOriginal field. </summary>	
		public virtual System.Int32 DistanceFromOriginal
		{ 
			get { return _distanceFromOriginal; }
			set { _distanceFromOriginal = value; }
		}

		/// <summary>Gets or sets the Id field. </summary>	
		public virtual System.Guid Id
		{ 
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'NodeRelationCache.EndNode - Node.IncomingNodeRelationCaches (m:1)'</summary>
		public virtual Node EndNode
		{
			get { return _endNode; }
			set { _endNode = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeRelationCache.StartNode - Node.OutgoingRelationCaches (m:1)'</summary>
		public virtual Node StartNode
		{
			get { return _startNode; }
			set { _startNode = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'NodeRelationCache.NodeRelation - NodeRelation.NodeRelationCaches (m:1)'</summary>
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
