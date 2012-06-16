#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;

#endregion

namespace Umbraco.Framework.Configuration
{
    public class ConfigurationElementEnumerator<TKey, TElement> : IEnumerator<TElement>
      where TElement : ConfigurationElement, new()
    {
        #region Constants and Fields

        private readonly ConfigurationElementCollection<TKey, TElement> _collection;

        private int _curIndex;

        #endregion

        #region Constructors and Destructors

        public ConfigurationElementEnumerator(ConfigurationElementCollection<TKey, TElement> collection)
        {
            _collection = collection;
            _curIndex = -1;
            Current = default(TElement);
        }

        #endregion

        #region Properties

        public TElement Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion

        #region Implemented Interfaces

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEnumerator

        public bool MoveNext()
        {
            // Avoids going beyond the end of the collection.
            if (++_curIndex >= _collection.Count)
            {
                return false;
            }

            // Set current box to next item in collection.
            Current = _collection[_curIndex];

            return true;
        }

        public void Reset()
        {
            _curIndex = -1;
        }

        #endregion

        #endregion

        #region Methods

        protected virtual void Dispose(bool resourcesOnly)
        {
        }

        #endregion
    }
}