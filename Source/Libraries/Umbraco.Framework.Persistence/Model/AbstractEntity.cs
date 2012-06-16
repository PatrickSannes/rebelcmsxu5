using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Umbraco.Framework.Data.Common;

namespace Umbraco.Framework.Persistence.Model
{



    [DebuggerDisplay("{DebugTypeName} with Id: {Id}")]
    public abstract class AbstractEntity : AbstractEquatableObject<AbstractEntity>, IEntity, INotifyPropertyChanged, ICanBeDirty, IReferenceByHiveId
    {
        protected AbstractEntity()
        {
            ConcurrencyToken = new RepositoryGeneratedToken();
            //entity.StatusType = FixedStatusTypes.Draft;
            UtcCreated = UtcModified = UtcStatusChanged = DateTimeOffset.UtcNow;
        }

        private string DebugTypeName
        {
            get { return GetType().Name; }
        }

        /// <summary>
        /// Gets the concurrency token.
        /// </summary>
        /// <value>The concurrency token.</value>
        public IConcurrencyToken ConcurrencyToken { get; set; }

        /// <summary>
        ///   Gets or sets the creation date of the entity (UTC).
        /// </summary>
        /// <value>The UTC created.</value>
        public DateTimeOffset UtcCreated { get; set; }

        /// <summary>
        ///   Gets or sets the modification date of the entity (UTC).
        /// </summary>
        /// <value>The UTC modified.</value>
        public DateTimeOffset UtcModified { get; set; }

        /// <summary>
        ///   Gets or sets the timestamp when the status of this entity was last changed (in UTC).
        /// </summary>
        /// <value>The UTC status changed.</value>
        public DateTimeOffset UtcStatusChanged { get; set; }

        /// <summary>
        ///   Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [TypeConverter(typeof(HiveIdTypeConverter))]
        public HiveId Id { get; set; }

        /// <summary>
        /// Tracks the properties that have changed
        /// </summary>
        private readonly IDictionary<string, bool> _propertyChangedInfo = new Dictionary<string, bool>();
        
        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;     

        /// <summary>
        /// Returns true if the property referenced by the name has been changed on the class
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool IsPropertyDirty(string propertyName)
        {
            return _propertyChangedInfo.Any(x => x.Key == propertyName);
        }

        /// <summary>
        /// Returns true if any properties have been changed on the class
        /// </summary>
        /// <returns></returns>
        public bool IsDirty()
        {
            return _propertyChangedInfo.Any();
        }

        /// <summary>
        /// Method to call on a property setter
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propSelector"></param>
        [Obsolete("Use overload that takes a PropertyInfo instead", true)]
        protected virtual void OnPropertyChanged<TSource, TProperty>(Expression<Func<TSource, TProperty>> propSelector)
            where TSource : class
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method to call on a property setter.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        protected virtual void OnPropertyChanged(PropertyInfo propertyInfo)
        {
            _propertyChangedInfo[propertyInfo.Name] = true;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyInfo.Name));
            }
        }

        protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
        {
            yield return this.GetPropertyInfo(x => x.Id);
            yield return this.GetPropertyInfo(x => x.UtcCreated);
            yield return this.GetPropertyInfo(x => x.UtcModified);
            yield return this.GetPropertyInfo(x => x.UtcStatusChanged);
        }
    }
}
