using System.Configuration;
using System.Globalization;
using System.Web.Configuration;

namespace Umbraco.Cms.Web.Configuration.Dashboards
{
    public class MatchFilterCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MatchFilterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MatchFilterElement) element).MatchFilterTypeName;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            var authorizationRule = new MatchFilterElement();
            switch (elementName.ToLower(CultureInfo.InvariantCulture))
            {
                case "allow":
                    authorizationRule.Action = AuthorizationRuleAction.Allow;
                    break;
                case "deny":
                    authorizationRule.Action = AuthorizationRuleAction.Deny;
                    break;
            }
            return authorizationRule;
        }

        protected override bool IsElementName(string elementname)
        {
            switch (elementname.ToLower(CultureInfo.InvariantCulture))
            {
                case "allow":
                case "deny":
                    return true;
            }
            return false;
        }


    }
}