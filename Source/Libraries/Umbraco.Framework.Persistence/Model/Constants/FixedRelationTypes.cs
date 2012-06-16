using System;
using System.Linq;
using System.Text;
using Umbraco.Framework.Persistence.Model.Associations;

namespace Umbraco.Framework.Persistence.Model.Constants
{
    public static class FixedRelationTypes
    {
        public static readonly RelationType DefaultRelationType = new RelationType("DefaultRelationType");
        public static readonly RelationType RecycledRelationType = new RelationType("RecycledRelationType");

        public static readonly RelationType ApplicationRelationType = new RelationType("ApplicationRelationType");
        public static readonly RelationType UserGroupRelationType = new RelationType("UserGroupRelationType");
        public static readonly RelationType PermissionRelationType = new RelationType("PermissionRelationType");

        public static readonly RelationType ThumbnailRelationType = new RelationType("ThumbnailRelationType");
        public static readonly RelationType HostnameRelationType = new RelationType("HostnameRelationType");
        public static readonly RelationType LanguageRelationType = new RelationType("LanguageRelationType");
    }
}
