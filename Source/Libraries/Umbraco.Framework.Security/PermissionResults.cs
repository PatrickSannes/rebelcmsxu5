using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Framework.Security
{
    public class PermissionResults : IEnumerable<PermissionResult>
    {
        private readonly HashSet<PermissionResult> _innerResults = new HashSet<PermissionResult>();

        public PermissionResults(IEnumerable<PermissionStatusResult> statusResults, IEnumerable<Lazy<Permission, PermissionMetadata>> allPermissions)
            : this(statusResults.Select(x => new PermissionResult(allPermissions.Get(x.PermissionId).Value, x.SourceId, x.Status)).ToArray())
        {}

        public PermissionResults(params PermissionResult[] results)
        {
            results.ForEach(x => _innerResults.Add(x));
        }

        /// <summary>
        /// Gets the statuses of all the results.
        /// </summary>
        /// <value>The statuses.</value>
        public IEnumerable<PermissionStatus> Statuses { get { return _innerResults.Select(x => x.Status); } }

        /// <summary>
        /// Gets the source id of this set of results.
        /// </summary>
        /// <value>The source id.</value>
        public HiveId SourceId { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether all of the permissions in this resultset are allowed.
        /// </summary>
        /// <returns></returns>
        public bool AreAllAllowed()
        {
            return !_innerResults.Any(x => !x.IsAllowed());
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<PermissionResult> GetEnumerator()
        {
            return _innerResults.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}