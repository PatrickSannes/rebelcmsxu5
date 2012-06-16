using System.Collections.Generic;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    public class UserGroupPermissionsModel
    {
        public HiveId UserGroupId { get; set; }
        public string UserGroupHtmlId { get; set; }
        public string UserGroupName { get; set; }
        public IEnumerable<PermissionStatusModel> Permissions { get; set; }
    }
}