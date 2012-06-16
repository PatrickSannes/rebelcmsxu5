using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Packaging;

namespace Umbraco.Cms.Web.Context
{

    

    /// <summary>
    /// Encapsulates information specific to a request handled by Umbraco back-office
    /// </summary>
    public interface IBackOfficeRequestContext : IRoutableRequestContext
    {

        SpriteIconFileResolver DocumentTypeIconResolver { get; }
        SpriteIconFileResolver ApplicationIconResolver { get; }
        IResolver<Icon> DocumentTypeThumbnailResolver { get; }


        IPackageContext PackageContext { get; }

    }
}