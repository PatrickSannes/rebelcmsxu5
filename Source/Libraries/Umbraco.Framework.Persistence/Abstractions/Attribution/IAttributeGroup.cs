using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;

namespace Umbraco.Framework.Persistence.Abstractions.Attribution
{
    /// <summary>
    /// Represents an instance of an <see cref="IAttributeGroupDefinition"/>
    /// </summary>
    public interface IAttributeGroup : IAttributeGroupDefinition
    {
        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <value>The attributes.</value>
        ITypedAttributeCollection Attributes { get; }
    }
}
