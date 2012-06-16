using System;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Marks a Pre value property as overridable at the Document Type property level
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AllowDocumentTypePropertyOverrideAttribute : Attribute
    {
        
    }
}