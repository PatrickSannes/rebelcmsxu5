using System;

namespace Umbraco.Cms.Web.Model.BackOffice.PropertyEditors
{
    /// <summary>
    /// Identifies an internal property editor that is not to be shown in the property editor drop down list
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class UmbracoPropertyEditorAttribute : Attribute
    {  

    }
}
