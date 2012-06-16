using System;
using System.Collections.Generic;
using System.Reflection;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
    using System.Diagnostics;

    /// <summary>Class which represents the entity 'AttributeDefinition'</summary>
    [Serializable]
    [DebuggerDisplay("Alias: {Alias}, Type: {AttributeType.Alias}, Id: {Id}")]
    public partial class AttributeDefinition : AbstractEquatableObject<AttributeDefinition>, IReferenceByGuid, IReferenceByAlias
	{
		#region Class Member Declarations
		private ICollection<Attribute> _attributes;
		private AttributeDefinitionGroup _attributeDefinitionGroup;
		private AttributeSchemaDefinition _attributeSchemaDefinition;
		private AttributeType _attributeType;
		private System.String _alias;
		private System.String _description;
		private System.Guid _id;
		private System.String _name;
		private System.String _xmlConfiguration;
        private int _ordinal;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="AttributeDefinition"/> class.</summary>
		public AttributeDefinition() : base()
		{
			_attributes = new HashSet<Attribute>();
			OnCreated();
		}

		/// <summary>Method called from the constructor</summary>
		partial void OnCreated();	

		#region Class Property Declarations

        /// <summary>
        /// Gets or sets the ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        /// <remarks></remarks>
        public virtual int Ordinal
        {
            get { return _ordinal; }
            set { _ordinal = value; }
        }

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

		/// <summary>Gets or sets the XmlConfiguration field. </summary>	
		public virtual System.String XmlConfiguration
		{ 
			get { return _xmlConfiguration; }
			set { _xmlConfiguration = value; }
		}

        /// <summary>Represents the navigator which is mapped onto the association 'Attribute.AttributeDefinition - AttributeDefinition.Attributes (m:1)'</summary>
        public virtual ICollection<Attribute> Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }
		
		/// <summary>Represents the navigator which is mapped onto the association 'AttributeDefinition.AttributeDefinitionGroup - AttributeDefinitionGroup.AttributeDefinitions (m:1)'</summary>
		public virtual AttributeDefinitionGroup AttributeDefinitionGroup
		{
			get { return _attributeDefinitionGroup; }
			set { _attributeDefinitionGroup = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'AttributeDefinition.AttributeSchemaDefinition - AttributeSchemaDefinition.AttributeDefinitions (m:1)'</summary>
		public virtual AttributeSchemaDefinition AttributeSchemaDefinition
		{
			get { return _attributeSchemaDefinition; }
			set { _attributeSchemaDefinition = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'AttributeDefinition.AttributeType - AttributeType.AttributeDefinitions (m:1)'</summary>
		public virtual AttributeType AttributeType
		{
			get { return _attributeType; }
			set { _attributeType = value; }
		}
		
		#endregion

	    #region Overrides of AbstractEquatableObject<NodeVersion>

	    /// <summary>
	    /// Gets the natural id members.
	    /// </summary>
	    /// <returns></returns>
	    /// <remarks></remarks>
	    protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
	    {
	        return new[] {this.GetPropertyInfo(x => x.Id), this.GetPropertyInfo(x => x.Alias)};
	    }

	    #endregion
	}
}
