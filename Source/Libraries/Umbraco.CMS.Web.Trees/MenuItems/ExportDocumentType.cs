using ClientDependency.Core;
using Umbraco.Cms.Web.Model.BackOffice.Trees;

namespace Umbraco.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("3B578040-B9B0-4B11-93A0-D8FC68D430B6",
        "Export document type",
        "Umbraco.Controls.MenuItems.ExportDocumentType",
        "menu-export-doc-type")]
    public class ExportDocumentType : MenuItem { }
}