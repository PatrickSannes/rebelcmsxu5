using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("A98F9C68-9A68-426E-AFCC-AD21F3511241",
        "Import document type",
        "Umbraco.Controls.MenuItems.ImportDocumentType",
        "menu-import-doc-type")]
    public class ImportDocumentType : MenuItem { }
}