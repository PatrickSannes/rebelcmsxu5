using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Framework.Security
{
    public class PermissionResult
    {
        public PermissionResult(Permission permission, HiveId sourceId, PermissionStatus status)
        {
            Permission = permission;
            SourceId = sourceId;
            Status = status;
        }

        public Permission Permission { get; protected set; }
        public HiveId SourceId { get; protected set; }
        public PermissionStatus Status { get; protected set; }

        public bool IsAllowed()
        {
            return Status == PermissionStatus.Allow;
        }
    }
}
