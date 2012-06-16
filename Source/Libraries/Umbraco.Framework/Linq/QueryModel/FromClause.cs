namespace Umbraco.Framework.Linq.QueryModel
{
    using System.Collections.Generic;
    using System.Linq;
    using Umbraco.Framework.Data;

    public class FromClause
    {
        public static readonly RevisionStatusType RevisionStatusNotSpecifiedType = new RevisionStatusType("not-specified", "Not Specified");
        // This is left here for backwards compilation compatibility with Hive providers that only take into account the alias
        public static readonly string RevisionStatusNotSpecified = RevisionStatusNotSpecifiedType.Alias;

        public FromClause(string startId, HierarchyScope hierarchyScope, RevisionStatusType revisionStatus, IEnumerable<HiveId> requiredEntityIds = null)
        {
            StartId = startId;
            HierarchyScope = hierarchyScope;
            RevisionStatusType = revisionStatus;
            RequiredEntityIds = requiredEntityIds ?? Enumerable.Empty<HiveId>();
        }

        public FromClause()
            : this(string.Empty, HierarchyScope.AllOrNone, RevisionStatusNotSpecifiedType)
        { }

        public string StartId { get; set; }
        public HierarchyScope HierarchyScope { get; set; }

        public IEnumerable<HiveId> RequiredEntityIds { get; set; }

        public string RevisionStatus
        {
            get
            {
                return RevisionStatusType.Alias;
            }
        }

        public RevisionStatusType RevisionStatusType { get; set; }
    }
}