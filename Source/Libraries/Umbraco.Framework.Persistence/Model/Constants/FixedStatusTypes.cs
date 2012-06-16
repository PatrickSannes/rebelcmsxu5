using System;
using Umbraco.Framework.Persistence.Model.Versioning;

namespace Umbraco.Framework.Persistence.Model.Constants
{
    public static class FixedStatusTypes
    {
        public static readonly RevisionStatusType Draft = new RevisionStatusType(FixedHiveIds.DraftStatusType, "draft", "Draft", true);
        public static readonly RevisionStatusType Unpublished = new RevisionStatusType(FixedHiveIds.UnpublishedStatusType, "unpublished", "Unpublished", true);
        public static readonly RevisionStatusType Created = new RevisionStatusType(FixedHiveIds.CreatedStatusType, "created", "Created", true);
        public static readonly RevisionStatusType Saved = new RevisionStatusType(FixedHiveIds.SavedStatusType, "saved", "Saved", true);

        public static readonly RevisionStatusType Published = new RevisionStatusType(FixedHiveIds.PublishedStatusType, "published", "Published", true, Unpublished);
    }
}