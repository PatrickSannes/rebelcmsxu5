using System;
using System.Collections.Generic;
using System.Reflection;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
    using System.Diagnostics;

    /// <summary>Class which represents the entity 'Attribute'</summary>
	[Serializable]
    [DebuggerDisplay("Alias: {AttributeDefinition.Alias}, Type: {AttributeDefinition.AttributeType.Alias}")]
    public partial class Attribute : AbstractEquatableObject<Attribute>, IReferenceByGuid
	{
		#region Class Member Declarations
		private ICollection<AttributeDateValue> _attributeDateValues;
		private AttributeDefinition _attributeDefinition;
        private ICollection<AttributeIntegerValue> _attributeIntegerValues;
        private ICollection<AttributeDecimalValue> _attributeDecimalValues;
		private ICollection<AttributeLongStringValue> _attributeLongStringValues;
		private ICollection<AttributeStringValue> _attributeStringValues;
		private NodeVersion _nodeVersion;
		private System.Guid _id;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="Attribute"/> class.</summary>
		public Attribute() : base()
		{
			_attributeDateValues = new HashSet<AttributeDateValue>();
            _attributeIntegerValues = new HashSet<AttributeIntegerValue>();
            _attributeDecimalValues = new HashSet<AttributeDecimalValue>();
			_attributeLongStringValues = new HashSet<AttributeLongStringValue>();
			_attributeStringValues = new HashSet<AttributeStringValue>();
			OnCreated();
		}

		/// <summary>Method called from the constructor</summary>
		partial void OnCreated();

	    /// <summary>
	    /// Gets the natural id members.
	    /// </summary>
	    /// <returns></returns>
	    /// <remarks></remarks>
	    protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
	    {
	        return new[] {this.GetPropertyInfo(x => x.Id)};
	    }

	    #region Class Property Declarations
		/// <summary>Gets or sets the Id field. </summary>	
		public virtual System.Guid Id
		{ 
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'AttributeDateValue.Attribute - Attribute.AttributeDateValues (m:1)'</summary>
		public virtual ICollection<AttributeDateValue> AttributeDateValues
		{
			get { return _attributeDateValues; }
			set { _attributeDateValues = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'Attribute.AttributeDefinition - AttributeDefinition.Attributes (m:1)'</summary>
		public virtual AttributeDefinition AttributeDefinition
		{
			get { return _attributeDefinition; }
			set { _attributeDefinition = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'AttributeIntegerValue.Attribute - Attribute.AttributeIntegerValues (m:1)'</summary>
		public virtual ICollection<AttributeIntegerValue> AttributeIntegerValues
		{
			get { return _attributeIntegerValues; }
			set { _attributeIntegerValues = value; }
		}

        /// <summary>Represents the navigator which is mapped onto the association 'AttributeDecimalValue.Attribute - Attribute.AttributeDecimalValues (m:1)'</summary>
        public virtual ICollection<AttributeDecimalValue> AttributeDecimalValues
        {
            get { return _attributeDecimalValues; }
            set { _attributeDecimalValues = value; }
        }
		
		/// <summary>Represents the navigator which is mapped onto the association 'AttributeLongStringValue.Attribute - Attribute.AttributeLongStringValues (m:1)'</summary>
		public virtual ICollection<AttributeLongStringValue> AttributeLongStringValues
		{
			get { return _attributeLongStringValues; }
			set { _attributeLongStringValues = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'AttributeStringValue.Attribute - Attribute.AttributeStringValues (m:1)'</summary>
		public virtual ICollection<AttributeStringValue> AttributeStringValues
		{
			get { return _attributeStringValues; }
			set { _attributeStringValues = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'Attribute.NodeVersion - NodeVersion.Attributes (m:1)'</summary>
		public virtual NodeVersion NodeVersion
		{
			get { return _nodeVersion; }
			set { _nodeVersion = value; }
		}
		
		#endregion
	}
}
