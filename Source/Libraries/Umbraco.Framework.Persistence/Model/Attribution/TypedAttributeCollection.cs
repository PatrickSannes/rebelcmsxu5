using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;


namespace Umbraco.Framework.Persistence.Model.Attribution
{
    using Umbraco.Framework.Persistence.Model.Attribution.MetaData;

    /// <summary>
    /// Represents a collection of <see cref="TypedAttribute"/> instances.
    /// </summary>
    /// <remarks></remarks>
    public class TypedAttributeCollection : KeyedCollection<string, TypedAttribute>, INotifyCollectionChanged
    {
        public Func<TypedAttribute, bool> ValidateAdd { get; set; }

        protected override void SetItem(int index, TypedAttribute item)
        {
            base.SetItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, index));
        }

        protected override void RemoveItem(int index)
        {
            var removed = this[index];
            base.RemoveItem(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
        }

        protected override void InsertItem(int index, TypedAttribute item)
        {            
            base.InsertItem(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public new void Add(TypedAttribute item)
        {
            Add(item, true);
        }

        public new void Add(TypedAttribute item, bool validate)
        {
            Mandate.ParameterNotNull(item, "item");

            bool? performAdd = null;
            if (validate && ValidateAdd != null) performAdd = ValidateAdd.Invoke(item);
            if (performAdd == null || performAdd == true) 
            {
                
                if (base.Contains(GetKeyForItem(item)))
                    throw new InvalidOperationException("This TypedAttributeCollection already contains an item with key {0}".InvariantFormat(GetKeyForItem(item)));

                base.Add(item);

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));

                return;
            }

            throw new InvalidOperationException(
                string.Format("Cannot add a '{0}' attribute to this collection as it does not exist in the schema",
                              item.AttributeDefinition.Alias));
        }

        protected TypedAttributeCollection(IEnumerable<TypedAttribute> attributes)
        {
            Reset(attributes);
        }

        /// <summary>
        /// Resets the collection to only contain the <see cref="TypedAttribute"/> instances referenced in the <paramref name="attributes"/> parameter, whilst maintaining
        /// any validation delegates such as <see cref="ValidateAdd"/>
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="validate">Validate the attributes against the known schema.</param>
        /// <remarks></remarks>
        public void Reset(IEnumerable<TypedAttribute> attributes)
        {
            Reset(attributes, true);
        }

        /// <summary>
        /// Resets the collection to only contain the <see cref="TypedAttribute"/> instances referenced in the <paramref name="attributes"/> parameter, whilst maintaining
        /// any validation delegates such as <see cref="ValidateAdd"/>
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="validate">Validate the attributes against the known schema.</param>
        /// <remarks></remarks>
        public void Reset(IEnumerable<TypedAttribute> attributes, bool validate)
        {
            Clear();
            attributes.ForEach(x => Add(x, validate));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedAttributeCollection"/> class with a delegate responsible for validating the addition of <see cref="TypedAttribute"/> instances.
        /// </summary>
        /// <param name="validationCallback">The validation callback.</param>
        /// <remarks></remarks>
        public TypedAttributeCollection(Func<TypedAttribute, bool> validationCallback)
        {
            ValidateAdd = validationCallback;
        }

        /// <summary>
        /// Determines whether this collection contains a <see cref="TypedAttribute"/> whose alias matches the specified attribute name.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns><c>true</c> if the collection contains the specified attribute name; otherwise, <c>false</c>.</returns>
        /// <remarks></remarks>
        public new bool Contains(string attributeName)
        {
            return this.Any(x => x.AttributeDefinition.Alias == attributeName);
        }

        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <returns>
        /// The key for the specified element.
        /// </returns>
        /// <param name="item">The element from which to extract the key.</param>
        protected override string GetKeyForItem(TypedAttribute item)
        {
            Mandate.ParameterNotNull(item, "item");
            Mandate.ParameterNotNull(item.AttributeDefinition, "item.AttributeDefinition");

            return item.AttributeDefinition.Alias;
        }

        public TypedAttribute this[AttributeDefinition attributeDefinition]
        {
            get
            {
                return this.FirstOrDefault(x => x.AttributeDefinition.Alias == attributeDefinition.Alias);
            }
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