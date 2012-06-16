using System;
using System.Collections.Generic;
using System.Linq;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'AttributeSchemaDefinition'</summary>
    [Serializable]
    public partial class AttributeSchemaDefinition : Node, IReferenceByAlias
	{
		#region Class Member Declarations
		private ICollection<AttributeDefinition> _attributeDefinitions;
        private ICollection<AttributeDefinitionGroup> _attributeDefinitionGroups;
        private ICollection<NodeVersion> _referencedNodes;
		private System.String _alias;
		private System.String _description;
        private System.String _name;
        private System.String _schemaType;
		private System.String _xmlConfiguration;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="AttributeSchemaDefinition"/> class.</summary>
		public AttributeSchemaDefinition() : base()
		{
			_attributeDefinitions = new HashSet<AttributeDefinition>();
		    _attributeDefinitionGroups = new HashSet<AttributeDefinitionGroup>();
		    _referencedNodes = new HashSet<NodeVersion>();
		}


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

		/// <summary>Gets or sets the Name field. </summary>	
		public virtual System.String Name
		{ 
			get { return _name; }
			set { _name = value; }
		}

        /// <summary>Gets or sets the Name field. </summary>	
        public virtual System.String SchemaType
        {
            get { return _schemaType; }
            set { _schemaType = value; }
        }

		/// <summary>Gets or sets the XmlConfiguration field. </summary>	
		public virtual System.String XmlConfiguration
		{ 
			get { return _xmlConfiguration; }
			set { _xmlConfiguration = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'AttributeDefinition.AttributeSchemaDefinition - AttributeSchemaDefinition.AttributeDefinitions (m:1)'</summary>
		public virtual ICollection<AttributeDefinition> AttributeDefinitions
		{
			get { return _attributeDefinitions; }
			set { _attributeDefinitions = value; }
		}
		
		#endregion

        /// <summary>
        /// Represents all of the nodes that are referencing/using the attribute schema definition
        /// </summary>
        public virtual ICollection<NodeVersion> ReferencedNodes
        {
            get { return _referencedNodes; }
            set { _referencedNodes = value; }
        }

        /// <summary>
        /// Represents the navigator which is mapped onto the association 'AttributeDefinitionGroup.AttributeSchemaDefinition - AttributeSchemaDefinition.AttributeDefinitionGroups (m:1)'
        /// </summary>
        public virtual ICollection<AttributeDefinitionGroup> AttributeDefinitionGroups
        {
            get { return _attributeDefinitionGroups; }
            set { _attributeDefinitionGroups = value; }
        }

        /// <summary>
        /// Gets the natural id members.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override IEnumerable<System.Reflection.PropertyInfo> GetMembersForEqualityComparison()
        {
            return base.GetMembersForEqualityComparison().Concat(new[] { this.GetPropertyInfo(y => y.Alias) });
        }
	}
}
