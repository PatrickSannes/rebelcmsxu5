using System;

namespace Umbraco.Cms.Web.Editors
{
    /// <summary>
    /// Identifies a default Umbraco editor that is shipped with the framework
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class UmbracoEditorAttribute : Attribute
    {  

    }
}
