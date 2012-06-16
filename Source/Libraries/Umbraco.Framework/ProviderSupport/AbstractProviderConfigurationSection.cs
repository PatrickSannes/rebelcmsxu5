using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Framework.ProviderSupport
{
    ///<summary>
    /// Abstract configuration section to for provider sections
    ///</summary>
    public abstract class AbstractProviderConfigurationSection : ConfigurationSection
    {
        /////<summary>
        ///// Exposes the child elements
        /////</summary>
        //public abstract IEnumerable<ConfigurationElement> Elements { get; }
    }
}