using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;

namespace Umbraco.Framework.Persistence.Model.Attribution
{
    [DebuggerDisplay("{ToDebugString()}")]
    public class TypedAttributeValueCollection : LazyConcurrentDictionary<string, object>, INotifyCollectionChanged //ConcurrentDictionary<string, object>
    {
        /// <summary>
        /// The key is "Value" by default because this is the default property name of PropertyEditors and serialization
        /// will just use the property names.
        /// </summary>
        public const string DefaultAttributeValueKey = "Value";

        public override void Add(string key, Lazy<object> valueLoader)
        {
            base.Add(key, valueLoader);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, valueLoader));
        }
        public override void AddOrUpdate(string key, Lazy<object> value, Func<string, Lazy<object>, Lazy<object>> updator)
        {
            base.AddOrUpdate(key, value, updator);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, updator));
        }
        public override void Clear()
        {
            base.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public override bool Remove(string key)
        {            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, base[key]));
            return base.Remove(key);
        }

        public virtual object GetDefaultValue()
        {
            object retrieve = null;
            return TryGetValue(DefaultAttributeValueKey, out retrieve) ? retrieve : null;
        }

        public virtual void SetDefaultValue(object value)
        {
            Add(DefaultAttributeValueKey, value);
        }

        public virtual void SetLazyDefaultValue(Lazy<object> value)
        {
            Add(DefaultAttributeValueKey, value);
        }

        protected string ToDebugString()
        {
            var sb = new StringBuilder();
            foreach (var pair in this)
            {
                sb.AppendLine(pair.ToDebugString(2));
            }
            return sb.ToString();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, args);
            }
        }
    }
}