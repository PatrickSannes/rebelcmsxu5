using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;

namespace Umbraco.Framework.EntityGraph.Domain.Entity.Attribution
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
