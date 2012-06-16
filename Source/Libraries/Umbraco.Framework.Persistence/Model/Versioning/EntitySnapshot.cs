using System.Collections.Generic;

namespace Umbraco.Framework.Persistence.Model.Versioning
{
    public class EntitySnapshot<T> where T : class, IVersionableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public EntitySnapshot(Revision<T> revision, IEnumerable<RevisionData> entityRevisionStatusList)
        {
            Revision = revision;
            EntityRevisionStatusList = entityRevisionStatusList;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public EntitySnapshot(Revision<T> revision) : this(revision, new HashSet<RevisionData>())
        {}

        public Revision<T> Revision { get; protected set; }

        public IEnumerable<RevisionData> EntityRevisionStatusList { get; protected set; }
    }
}