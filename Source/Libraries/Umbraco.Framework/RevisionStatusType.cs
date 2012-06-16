

namespace Umbraco.Framework
{
    using System.Collections.Generic;
    using System.Reflection;

    public class RevisionStatusType : AbstractEquatableObject<RevisionStatusType>, IReferenceByName
    {
        public RevisionStatusType()
        {
            _negatedByTypes = new HashSet<RevisionStatusType>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RevisionStatusType(HiveId systemId, string @alias, LocalizedString name, bool isSystem, params RevisionStatusType[] negatedBy) : this(alias, name)
        {
            Id = systemId;
            IsSystem = isSystem;
            negatedBy.ForEach(x => _negatedByTypes.Add(x));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RevisionStatusType(string @alias, string name) 
            : this()
        {
            Alias = alias;
            Name = name;
            Id = new HiveId(Alias.EncodeAsGuid());
        }

        public virtual HiveId Id { get; set; }

        private readonly HashSet<RevisionStatusType> _negatedByTypes;
        public IEnumerable<RevisionStatusType> NegatedByTypes
        {
            get
            {
                return _negatedByTypes;
            }
            set
            {
                _negatedByTypes.Clear();
                value.ForEach(x => _negatedByTypes.Add(x));
            }

        }

        #region Implementation of IReferenceByName

        /// <summary>
        /// Gets or sets the alias of the object. The alias is a string to which this object
        /// can be referred programmatically, and is often a normalised version of the <see cref="IReferenceByName.Name"/> property.
        /// </summary>
        /// <value>The alias.</value>
        public virtual string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the object. The name is a string intended to be human-readable, and
        /// is often a more descriptive version of the <see cref="IReferenceByName.Alias"/> property.
        /// </summary>
        /// <value>The name.</value>
        public virtual LocalizedString Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a built-in system value.
        /// </summary>
        /// <value><c>true</c> if this instance is system; otherwise, <c>false</c>.</value>
        /// <remarks></remarks>
        public virtual bool IsSystem { get; set; }

        #endregion

        public string GetNameOrAlias()
        {
            if (Name == null || Name.Value.IsNullOrWhiteSpace()) return Alias;
            return Name.Value;
        }

        #region Overrides of AbstractEquatableObject<RevisionStatusType>

        /// <summary>
        /// Gets the natural id members.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
        {
            yield return this.GetPropertyInfo(x => x.Id);
            yield return this.GetPropertyInfo(x => x.Alias);
        }

        #endregion
    }
}