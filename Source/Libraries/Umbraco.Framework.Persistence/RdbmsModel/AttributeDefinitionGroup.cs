using System;
using System.Collections.Generic;
using System.Linq;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
    using System.Diagnostics;

    /// <summary>Class which represents the entity 'AttributeDefinitionGroup'</summary>
    [Serializable]
    [DebuggerDisplay("Alias: {Alias}, Id: {Id}")]
    public partial class AttributeDefinitionGroup : Node, IReferenceByAlias
	{
		#region Class Member Declarations
		private ICollection<AttributeDefinition> _attributeDefinitions;
		private System.String _alias;
		private System.String _description;
		private System.String _name;
	    private int _ordinal;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="AttributeDefinitionGroup"/> class.</summary>
		public AttributeDefinitionGroup() : base()
		{
			_attributeDefinitions = new HashSet<AttributeDefinition>();
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

		/// <summary>Represents the navigator which is mapped onto the association 'AttributeDefinition.AttributeDefinitionGroup - AttributeDefinitionGroup.AttributeDefinitions (m:1)'</summary>
		public virtual ICollection<AttributeDefinition> AttributeDefinitions
		{
			get { return _attributeDefinitions; }
			set { _attributeDefinitions = value; }
		}
		
		#endregion

	    private AttributeSchemaDefinition _attributeSchemaDefinition;

	    /// <summary>
        /// Gets or sets the attribute schema definition.
        /// </summary>
        public virtual AttributeSchemaDefinition AttributeSchemaDefinition
	    {
	        get { return _attributeSchemaDefinition; }
	        set { _attributeSchemaDefinition = value; }
	    }

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

        /// <summary>
        /// Gets the natural id members.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override IEnumerable<System.Reflection.PropertyInfo> GetMembersForEqualityComparison()
        {
            return base.GetMembersForEqualityComparison().Concat(new[] {this.GetPropertyInfo(y => y.Alias)});
        }
	}
}
