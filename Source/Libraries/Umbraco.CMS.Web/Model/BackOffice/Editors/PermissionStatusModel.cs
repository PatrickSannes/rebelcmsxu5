using System;
using Umbraco.Cms.Web.Security.Permissions;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    public class PermissionStatusModel
    {
        public PermissionStatusModel()
        {
            Status = PermissionStatus.Inherit;
        }

        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; }
        public PermissionStatus? Status { get; set; }
        public PermissionStatus? InheritedStatus { get; set; }
    }
}