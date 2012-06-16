using System;
using System.Collections.Generic;
using System.Linq;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'AttributeType'</summary>
    [Serializable]
    public partial class AttributeType : AbstractEquatableObject<AttributeType>, IReferenceByGuid, IReferenceByAlias
	{
		#region Class Member Declarations
		private ICollection<AttributeDefinition> _attributeDefinitions;
		private System.String _alias;
		private System.String _description;
		private System.Guid _id;
		private System.String _name;
		private System.String _persistenceTypeProvider;
		private System.String _renderTypeProvider;
		private System.String _xmlConfiguration;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="AttributeType"/> class.</summary>
		public AttributeType() : base()
		{
			_attributeDefinitions = new HashSet<AttributeDefinition>();
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

		/// <summary>Gets or sets the PersistenceTypeProvider field. </summary>	
		public virtual System.String PersistenceTypeProvider
		{ 
			get { return _persistenceTypeProvider; }
			set { _persistenceTypeProvider = value; }
		}

		/// <summary>Gets or sets the RenderTypeProvider field. </summary>	
		public virtual System.String RenderTypeProvider
		{ 
			get { return _renderTypeProvider; }
			set { _renderTypeProvider = value; }
		}

		/// <summary>Gets or sets the XmlConfiguration field. </summary>	
		public virtual System.String XmlConfiguration
		{ 
			get { return _xmlConfiguration; }
			set { _xmlConfiguration = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'AttributeDefinition.AttributeType - AttributeType.AttributeDefinitions (m:1)'</summary>
		public virtual ICollection<AttributeDefinition> AttributeDefinitions
		{
			get { return _attributeDefinitions; }
			set { _attributeDefinitions = value; }
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
