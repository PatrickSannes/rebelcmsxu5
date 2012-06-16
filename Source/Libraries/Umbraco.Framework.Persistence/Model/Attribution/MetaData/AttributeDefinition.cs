

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Umbraco.Framework.Persistence.Model.Attribution.MetaData
{
    public class AttributeDefinition : AbstractSchemaPart<AttributeDefinition>, IReferenceByName, IReferenceByOrdinal 
    {
        public AttributeDefinition(string @alias, LocalizedString name)
        {
            Alias = alias;
            Name = name;
        }

        public AttributeDefinition()
        {
        }

        private AttributeGroup _attributeGroup;
        private LocalizedString _description;
        private AttributeType _attributeType;
        private string _renderTypeProviderConfigOverride;
        private int _ordinal;
        private readonly static PropertyInfo RenderTypeProviderConfigOverrideSelector = ExpressionHelper.GetPropertyInfo<AttributeDefinition, string>(x => x.RenderTypeProviderConfigOverride);
        private readonly static PropertyInfo OrdinalSelector = ExpressionHelper.GetPropertyInfo<AttributeDefinition, int>(x => x.Ordinal);
        private readonly static PropertyInfo TypeSelector = ExpressionHelper.GetPropertyInfo<AttributeDefinition, AttributeType>(x => x.AttributeType);
        private readonly static PropertyInfo GroupSelector = ExpressionHelper.GetPropertyInfo<AttributeDefinition, AttributeGroup>(x => x.AttributeGroup);
        private readonly static PropertyInfo DescriptionSelector = ExpressionHelper.GetPropertyInfo<AttributeDefinition, LocalizedString>(x => x.Description);

        public AttributeGroup AttributeGroup
        {
            get { return _attributeGroup; }
            set
            {
                _attributeGroup = value;
                OnPropertyChanged(GroupSelector);
            }
        }
       
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks></remarks>
        public LocalizedString Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(DescriptionSelector);
            }
        }

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

        /// <summary>
        /// Gets or sets the type of the attribute.
        /// </summary>
        /// <value>The type of the attribute.</value>
        public AttributeType AttributeType
        {
            get { return _attributeType; }
            set
            {
                _attributeType = value;
                OnPropertyChanged(TypeSelector);
            }
        }
        
        public string RenderTypeProviderConfigOverride
        {
            get { return _renderTypeProviderConfigOverride; }
            set
            {
                _renderTypeProviderConfigOverride = value;
                OnPropertyChanged(RenderTypeProviderConfigOverrideSelector);
            }
        }

    }
}