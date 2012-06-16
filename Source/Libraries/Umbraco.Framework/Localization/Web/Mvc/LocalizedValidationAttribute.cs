using System;

namespace Umbraco.Framework.Localization.Web.Mvc
{
    [AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
    public class LocalizedValidationAttribute : Attribute
    {
        public Type ForAttribute { get; set; }
        public string Key { get; set; }
        public string Namespace { get; set; }

        public LocalizedValidationAttribute(Type forAttribute, string key)
        {
            ForAttribute = forAttribute;
            Key = key;            
        }
    }
}
