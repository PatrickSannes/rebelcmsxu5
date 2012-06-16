namespace Umbraco.Framework.Dynamics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public abstract class EnumeratorWrapper<T> : IEnumerator<T>
    {
        protected IEnumerator<T> InnerEnumerator;

        public EnumeratorWrapper(IEnumerator<T> innerEnumerator)
        {
            InnerEnumerator = innerEnumerator;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            InnerEnumerator.Dispose();
        }

        #endregion

        #region Implementation of IEnumerator

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public bool MoveNext()
        {
            return InnerEnumerator.MoveNext();
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
        public void Reset()
        {
            InnerEnumerator.Reset();
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        /// <returns>
        /// The element in the collection at the current position of the enumerator.
        /// </returns>
        public abstract T Current { get; }

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        /// <returns>
        /// The current element in the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception><filterpriority>2</filterpriority>
        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        #endregion
    }

    public abstract class QueryWrapperForDynamics<T, TOriginalType> : IQueryable<T>
        where T : class
        where TOriginalType : T
    {

        protected readonly IQueryable<T> InnerSet = new HashSet<T>().AsQueryable();

        protected QueryWrapperForDynamics(IQueryable<T> source)
        {
            InnerSet = source;
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public virtual IEnumerator<T> GetEnumerator()
        {
            return InnerSet.GetEnumerator();
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

        public int Count()
        {
            return Queryable.Count(this);
        }

        public bool Any()
        {
            return Queryable.Any(this);
        }

        public virtual T First()
        {
            return (T)Queryable.First<TOriginalType>(this.OfType<TOriginalType>());
        }

        public virtual T FirstOrDefault()
        {
            return (T)Queryable.FirstOrDefault<TOriginalType>(this.OfType<TOriginalType>());
        }

        public T Single()
        {
            return Queryable.Single(this);
        }

        public T SingleOrDefault()
        {
            return Queryable.SingleOrDefault(this);
        }

        public T Last()
        {
            return Queryable.Last(this);
        }

        public T LastOrDefault()
        {
            return Queryable.LastOrDefault(this);
        }

        public T ElementAt(int index)
        {
            return Queryable.ElementAt(this, index);
        }

        public T ElementAtOrDefault(int index)
        {
            return Queryable.ElementAtOrDefault(this, index);
        }

        public IList<T> ToList()
        {
            return Enumerable.ToList(this);
        }

        public int IndexOf(T item)
        {
            if (item == null) return -1;
            // This necessarily enumerates the inner set at the moment until there's a way for BendyObject
            // to reliably implement IComparable<T>
            int index = -1;
            foreach (var element in InnerSet)
            {
                index++;
                if (ReferenceEquals(element, item) || element == item) return index;
            }
            return -1;
        }

        #region Implementation of IQueryable

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.Expressions.Expression"/> that is associated with this instance of <see cref="T:System.Linq.IQueryable"/>.
        /// </returns>
        public Expression Expression
        {
            get
            {
                return InnerSet.Expression;
            }
        }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type"/> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.
        /// </returns>
        public Type ElementType
        {
            get
            {
                return InnerSet.ElementType;
            }
        }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Linq.IQueryProvider"/> that is associated with this data source.
        /// </returns>
        public IQueryProvider Provider
        {
            get
            {
                return InnerSet.Provider;
            }
        }

        #endregion
    }
}