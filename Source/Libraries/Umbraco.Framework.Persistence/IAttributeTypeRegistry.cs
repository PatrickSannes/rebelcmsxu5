using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;

namespace Umbraco.Framework.Persistence
{
    /// <summary>
    /// Defines at an AttributeType registry to resolve AttributeTypes by alias
    /// </summary>
    /// <remarks>
    /// This is used in order to ensure that pre-made AttributeType's are setup correctly 
    /// for the current environment such as the CMS environment since each environment may
    /// require that an AttributeType behave slightly differently like rendering a different
    /// RenderTypeProvider
    /// </remarks>
    public interface IAttributeTypeRegistry
    {

        /// <summary>
        /// Gets the AttributeType by alias
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        AttributeType GetAttributeType(string alias);

        /// <summary>
        /// Registers or updates an AttributeType in the registry
        /// </summary>
        /// <param name="attributeType">the AttributeType.</param>
        void RegisterAttributeType(Func<AttributeType> attributeType);

        /// <summary>
        /// Tries to get an <see cref="AttributeType" /> by alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        AttemptTuple<AttributeType> TryGetAttributeType(string alias);
    }
}
