using System;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Framework.Persistence.Model.Versioning
{
    public class RevisionData : AbstractEntity
    {
        public RevisionData()
            : this(new Changeset(new Branch("default")), FixedStatusTypes.Draft)
        {
        }

        #region Implementation of IRevisionData

        public RevisionData(Changeset changeset, HiveId revisionId, RevisionStatusType revisionStatusType)
            : this(changeset, revisionStatusType)
        {
            Id = revisionId;
        }

        public RevisionData(HiveId revisionId, RevisionStatusType revisionStatusType)
            : this(revisionStatusType)
        {
            Id = revisionId;
        }

        public RevisionData(Changeset changeset, RevisionStatusType revisionStatusType)
        {
            Changeset = changeset;
            StatusType = revisionStatusType;
        }

        public RevisionData(RevisionStatusType revisionStatusType) : this()
        {
            StatusType = revisionStatusType;
        }

        /// <summary>
        /// Creates a RevisionData object based on a single changeset and a branch name of "default".
        /// This object model is included to later support changesets and branching (May 2011 - APN).
        /// </summary>
        public static RevisionData CreateDefault(HiveId id, RevisionStatusType revisionStatusType, DateTimeOffset utcCreated, DateTimeOffset utcModified, DateTimeOffset utcStatusChanged)
        {
            var output = new RevisionData
                             {
                                 Id = id,
                                 StatusType = revisionStatusType,
                                 UtcCreated = utcCreated,
                                 UtcModified = utcModified,
                                 UtcStatusChanged = utcStatusChanged
                             };
            return output;
        }

        /// <summary>
        /// Gets or sets the changeset.
        /// </summary>
        /// <value>The changeset.</value>
        public virtual Changeset Changeset { get; set; }

        /// <summary>
        ///   Gets or sets the status of the entity.
        /// </summary>
        /// <value>The status.</value>
        public virtual RevisionStatusType StatusType { get; set; }

        #endregion
    }
}