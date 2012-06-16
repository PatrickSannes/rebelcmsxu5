using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;

namespace Umbraco.Framework.Persistence.Model.Attribution.MetaData
{
    public class AttributeType : AbstractSchemaPart<AttributeType>, IReferenceByName, IReferenceByOrdinal, IEquatable<AttributeType>
    {
        private readonly static PropertyInfo RenderTypeProviderSelector = ExpressionHelper.GetPropertyInfo<AttributeType, string>(x => x.RenderTypeProvider);
        private readonly static PropertyInfo RenderTypeConfigSelector = ExpressionHelper.GetPropertyInfo<AttributeType, string>(x => x.RenderTypeProviderConfig);
        private readonly static PropertyInfo OrdinalSelector = ExpressionHelper.GetPropertyInfo<AttributeType, int>(x => x.Ordinal);
        private readonly static PropertyInfo DescriptionSelector = ExpressionHelper.GetPropertyInfo<AttributeType, LocalizedString>(x => x.Description);
        private readonly static PropertyInfo SerializationTypeSelector = ExpressionHelper.GetPropertyInfo<AttributeType, IAttributeSerializationDefinition>(x => x.SerializationType);

        public AttributeType(string @alias, LocalizedString name, LocalizedString description, IAttributeSerializationDefinition serializationType)
        {
            this.Setup(alias, name, description);
            SerializationType = serializationType;
        }

        public AttributeType()
        {
        }

        /// <summary>
        /// Gets or sets the render type provider.
        /// </summary>
        /// <value>The render type provider.</value>
        /// <remarks></remarks>
        public string RenderTypeProvider
        {
            get { return _renderTypeProvider; }
            set
            {
                _renderTypeProvider = value;
                OnPropertyChanged(RenderTypeProviderSelector);
            }
        }
        private string _renderTypeProvider;

        /// <summary>
        /// Gets or sets the render type provider config.
        /// </summary>
        /// <value>The render type provider config.</value>
        /// <remarks></remarks>
        public string RenderTypeProviderConfig
        {
            get { return _renderTypeProviderConfig; }
            set
            {
                _renderTypeProviderConfig = value;
                OnPropertyChanged(RenderTypeConfigSelector);
            }
        }
        private string _renderTypeProviderConfig;

        #region Implementation of IExposesUIData
        
        /// <summary>
        ///   Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public LocalizedString Description
        {
            get { return _description; }
            set
            {
                OnPropertyChanged(DescriptionSelector);
                _description = value;
            }
        }
        private LocalizedString _description;

        #endregion

        #region Implementation of IComparable<in int>

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(int other)
        {
            return Ordinal - other * -1;
        }

        #endregion

        #region Implementation of IReferenceByOrdinal

        

        /// <summary>
        /// Gets the ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        public int Ordinal
        {
            get { return _ordinal; }
            set
            {
                OnPropertyChanged(OrdinalSelector);
                _ordinal = value;
            }
        }
        private int _ordinal;

        #endregion
        
        public IAttributeSerializationDefinition SerializationType
        {
            get { return _serializationType; }
            set
            {
                OnPropertyChanged(SerializationTypeSelector);
                _serializationType = value;
            }
        }
        private IAttributeSerializationDefinition _serializationType;

        public override int GetHashCode()
        {
            return Id.IsNullValueOrEmpty() ? 42 : Id.GetHashCode();
        }

        public bool Equals(AttributeType other)
        {
            if (other == null) return false;
            return Id == other.Id && 
                string.Equals(Alias ?? string.Empty, other.Alias ?? string.Empty)
                && string.Equals(Name ?? string.Empty, other.Name ?? string.Empty);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (typeof(AttributeType).IsAssignableFrom(obj.GetType()))
                return Equals((AttributeType)obj);
            return base.Equals(obj);
        }

        public static bool operator ==(AttributeType left, AttributeType right)
        {
            if (object.ReferenceEquals(left, right))
                return true;

            if (object.Equals(left, null) || object.Equals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(AttributeType left, AttributeType right)
        {
            return !(left == right);
        }
    }
}