using Umbraco.Framework.Localization.Configuration;

namespace Umbraco.Framework.Localization
{
    /// <summary>
    /// Inherit from this class to make a simple way to get texts for the current assembly.
    /// </summary>
    /// <typeparam name="TNamespaceRef"></typeparam>
    public class Localization<TNamespaceRef>
    {
        public static string Get(string key, object parameters = null, LanguageInfo language = null, string ns = null, bool? debug = null, bool returnNullOnMissing = false, bool encode = true)
        {
            return LocalizationConfig.TextManager.Get<TNamespaceRef>(key, parameters, language, ns, debug,
                                                                     returnNullOnMissing, encode);
        }        
    }
}
