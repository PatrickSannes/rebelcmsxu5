using System;

namespace Umbraco.Cms.Web.Model.BackOffice.Trees
{
    /// <summary>
    /// Identifies a default Umbraco tree that is shipped with the framework
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class UmbracoTreeAttribute : Attribute
    {  

    }
}
