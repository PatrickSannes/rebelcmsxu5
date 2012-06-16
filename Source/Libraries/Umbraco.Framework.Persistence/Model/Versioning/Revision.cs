using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Framework.Persistence.Model.Versioning
{
    

    [DebuggerDisplay("Revision Id: {MetaData.Id} of Id: {Item.Id}")]
    public class Revision<T> : ICanBeDirty, INotifyPropertyChanged
        where T : class, IVersionableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public Revision()
        {
            MetaData = new RevisionData(FixedStatusTypes.Created);
        }

        public Revision(T item) : this()
        {
            Item = item;
        }

        private RevisionData _metaData;
        public RevisionData MetaData
        {
            get { return _metaData; }
            set
            {
                OnPropertyChanged<Revision<T>, RevisionData>(x => x.MetaData);
                _metaData = value;
            }
        }

        private T _item;
        public T Item
        {
            get { return _item; }
            set
            {
                OnPropertyChanged<Revision<T>, T>(x => x.Item);
                _item = value;
            }
        }

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
        protected virtual void OnPropertyChanged<TSource, TProperty>(Expression<Func<TSource, TProperty>> propSelector)
            where TSource : class
        {
            var source = this as TSource;
            if (source == null)
                throw new InvalidCastException("Could not cast type " + this.GetType() + " to " + typeof(TSource));
            var propInfo = source.GetPropertyInfo(propSelector);
            _propertyChangedInfo[propInfo.Name] = true;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propInfo.Name));
            }
        }
    }
}
