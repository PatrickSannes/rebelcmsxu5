using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Mvc.Controllers.BackOffice
{
    /// <summary>
    /// Ensures that the user is authorized to access any actions on the controller
    /// </summary>
    [UmbracoAuthorize(Permissions = new[]{ FixedPermissionIds.BackOfficeAccess }, Order = 10)]
    public abstract class SecuredBackOfficeController : BackOfficeController
    {
        protected SecuredBackOfficeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }
    }
}