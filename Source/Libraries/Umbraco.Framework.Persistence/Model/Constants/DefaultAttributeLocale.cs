using Umbraco.Framework.Localization;

namespace Umbraco.Framework.Persistence.Model.Constants
{
    public class DefaultAttributeLocale : LanguageInfo
    {
        public DefaultAttributeLocale()
        {
            Key = Alias; // Need to refactor LanguageInfo to use Alias and not Key
        }

        public override LocalizedString Name
        {
            get { return "Default Language (Attribute localization coming in a later version)"; }
            set { return; }
        }

        public override string Alias
        {
            get { return "en"; }
            set { return; }
        }
    }
}