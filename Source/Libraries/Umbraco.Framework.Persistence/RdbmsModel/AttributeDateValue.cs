using System;
using System.Collections.Generic;


namespace Umbraco.Framework.Persistence.RdbmsModel
{
	/// <summary>Class which represents the entity 'AttributeDateValue'</summary>
    [Serializable]
    public partial class AttributeDateValue : AbstractEquatableObject<AttributeDateValue>, IReferenceByGuid
	{
		#region Class Member Declarations
		private Attribute _attribute;
		private Locale _locale;
		private System.Guid _id;
		private System.DateTimeOffset _value;
		private System.String _valueKey;
		#endregion

		/// <summary>Initializes a new instance of the <see cref="AttributeDateValue"/> class.</summary>
		public AttributeDateValue() : base()
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

		/// <summary>Gets or sets the Value field. </summary>	
		public virtual System.DateTimeOffset Value
		{ 
			get { return _value; }
			set { _value = value; }
		}

		/// <summary>Gets or sets the ValueKey field. </summary>	
		public virtual System.String ValueKey
		{ 
			get { return _valueKey; }
			set { _valueKey = value; }
		}

		/// <summary>Represents the navigator which is mapped onto the association 'AttributeDateValue.Attribute - Attribute.AttributeDateValues (m:1)'</summary>
		public virtual Attribute Attribute
		{
			get { return _attribute; }
			set { _attribute = value; }
		}
		
		/// <summary>Represents the navigator which is mapped onto the association 'AttributeDateValue.Locale - Locale.AttributeDateValues (m:1)'</summary>
		public virtual Locale Locale
		{
			get { return _locale; }
			set { _locale = value; }
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
