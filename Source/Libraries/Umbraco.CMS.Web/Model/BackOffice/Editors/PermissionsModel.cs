using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    public class PermissionsModel : DialogModel, IMetadataAware
    {
        public HiveId Id { get; set; }
        public IEnumerable<UserGroupPermissionsModel> UserGroupPermissions { get; set; }

        public void OnMetadataCreated(ModelMetadata metadata)
        {
            metadata.DisplayName = "User Group";
        }
    }
}
