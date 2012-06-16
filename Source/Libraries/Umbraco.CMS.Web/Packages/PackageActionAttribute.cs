using Umbraco.Cms.Domain.BackOffice;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Packages
{
    /// <summary>
    /// Attribute that defines a Package Action
    /// </summary>
    public class PackageActionAttribute : PluginAttribute
    {
        public string Name { get; private set; }

        public PackageActionAttribute(string id, string name) 
            : base(id)
        {
            Name = name;
        }
    }
}