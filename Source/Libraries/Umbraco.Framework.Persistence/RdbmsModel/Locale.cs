using System;
using System.Collections.Generic;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'Locale'</summary>
    [Serializable]
    public partial class Locale : AbstractEquatableObject<Locale>, IReferenceByGuid, IReferenceByAlias
	{
		#region Class Member Declarations
		private ICollection<AttributeDateValue> _attributeDateValues;
		private ICollection<AttributeIntegerValue> _attributeIntegerValues;
		private ICollection<AttributeLongStringValue> _attributeLongStringValues;
		private ICollection<AttributeStringValue> _attributeStringValues;
		private System.String _alias;
		private System.Guid _id;
		private System.String _languageIso;
		private System.String _name;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="Locale"/> class.</summary>
		public Locale() : base()
		{
            _attributeDateValues = new HashSet<AttributeDateValue>();
            _attributeIntegerValues = new HashSet<AttributeIntegerValue>();
            _attributeLongStringValues = new HashSet<AttributeLongStringValue>();
            _attributeStringValues = new HashSet<AttributeStringValue>();
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

		/// <summary>Gets or sets the Id field. </summary>	
		public virtual System.Guid Id
		{ 
			get { return _id; }
			set { _id = value; }
		}

		/// <summary>Gets or sets the LanguageIso field. </summary>	
		public virtual System.String LanguageIso
		{ 
			get { return _languageIso; }
			set { _languageIso = value; }
		}

		/// <summary>Gets or sets the Name field. </summary>	
		public virtual System.String Name
		{ 
			get { return _name; }
			set { _name = value; }
		}

		#endregion

        /// <summary>Represents the navigator which is mapped onto the association 'AttributeDateValue.Locale - Locale.AttributeDateValues (m:1)'</summary>
		public virtual ICollection<AttributeDateValue> AttributeDateValues
		{
			get { return _attributeDateValues; }
			set { _attributeDateValues = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'AttributeIntegerValue.Locale - Locale.AttributeIntegerValues (m:1)'</summary>
		public virtual ICollection<AttributeIntegerValue> AttributeIntegerValues
		{
			get { return _attributeIntegerValues; }
			set { _attributeIntegerValues = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'AttributeLongStringValue.Locale - Locale.AttributeLongStringValues (m:1)'</summary>
		public virtual ICollection<AttributeLongStringValue> AttributeLongStringValues
		{
			get { return _attributeLongStringValues; }
			set { _attributeLongStringValues = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'AttributeStringValue.Locale - Locale.AttributeStringValues (m:1)'</summary>
		public virtual ICollection<AttributeStringValue> AttributeStringValues
		{
			get { return _attributeStringValues; }
			set { _attributeStringValues = value; }
		}

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
