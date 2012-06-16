using System.Collections;
using System.Linq;
using System.Reflection;


namespace Umbraco.Framework.Persistence.Model.Attribution.MetaData
{
    public class AttributeGroup : AbstractSchemaPart<AttributeGroup>, IReferenceByName, IReferenceByOrdinal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeGroup"/> class.
        /// </summary>
        public AttributeGroup()
        {
        }

        public AttributeGroup(string @alias, LocalizedString name, int ordinal)
        {
            Alias = alias;
            Name = name;
            Ordinal = ordinal;
        }

        private int _ordinal;

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(int other)
        {
            return new CaseInsensitiveComparer().Compare(this.Ordinal, other);
        }

        /// <summary>
        /// Gets the ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        public int Ordinal
        {
            get { return _ordinal; }
            set
            {
                _ordinal = value;
                OnPropertyChanged(OrdinalSelector);
            }
        }
        private readonly static PropertyInfo OrdinalSelector = ExpressionHelper.GetPropertyInfo<AttributeGroup, int>(x => x.Ordinal);

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Id.ToString();
        }

        protected override System.Collections.Generic.IEnumerable<System.Reflection.PropertyInfo> GetMembersForEqualityComparison()
        {
            var members = new[]
                {
                    this.GetPropertyInfo(x => x.Alias)
                };
            return base.GetMembersForEqualityComparison().Concat(members);
        }
    }
}