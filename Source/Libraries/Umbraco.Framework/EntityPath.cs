using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Framework
{
    public class StringCastableEnumerable<T> : IEnumerable<T>
    {
        protected IEnumerable<T> Inner;

        public StringCastableEnumerable(IEnumerable<T> inner)
        {
            Inner = inner;
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
            return Inner.GetEnumerator();
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

        public override string ToString()
        {
            return string.Join(", ", this.Select(x => x.ToString()));
        }

        public static explicit operator string(StringCastableEnumerable<T> other)
        {
            return other == null ? string.Empty : other.ToString();
        }
    }

    public class EntityPath : StringCastableEnumerable<HiveId>
    {
        public EntityPath(IEnumerable<HiveId> path)
            : base(path)
        { }

        /// <summary>
        /// Gets the <see cref="Umbraco.Framework.HiveId"/> with the specified level. Not that this is not 0-indexed. This indexer is intended
        /// to be passed a <see cref="Level"/> where 0 is typically the Content root
        /// </summary>
        /// <value></value>
        public HiveId this[int i]
        {
            get
            {
                return this[i, false];
            }
        }

        /// <summary>
        /// Gets the <see cref="Umbraco.Framework.HiveId"/> with the specified level. Not that this is not 0-indexed. This indexer is intended
        /// to be passed a <see cref="Level"/> where 0 is typically the Content root
        /// </summary>
        /// <value></value>
         public HiveId this[int i, bool includeSystemIds]
        {
            get
            {
                return includeSystemIds ? TruePath.ElementAtOrDefault(i + 1) : NaturalPath.ElementAtOrDefault(i);
            }
        }

        public override IEnumerator<HiveId> GetEnumerator()
        {
            return NaturalPath.GetEnumerator();
        }

        /// <summary>
        /// Gets the "level" of the item that this <see cref="EntityPath"/> represents in a hierarchy.
        /// This is a shortcut to NaturalPath.Count, excluding any <see cref="HiveId"/> that are marked "system" ids.
        /// For example, the default Content Root would return 0 because it's a system id. The first child of the content root, e.g.
        /// Homepage, would return 1, etc.
        /// </summary>
        /// <value>The level.</value>
        public int Level
        {
            get
            {
                // Level should start at -1 where -1 is the last system node
                // But 0 still needs to equal the first non-system node whether or not
                // any system ids are in the path, so just strip out all the remaining
                // system ids before counting
                return NaturalPath.Count(x => !x.IsSystem());
            }
        }

        /// <summary>
        /// Gets the "natural" path, i.e. the list of <see cref="HiveId"/> from the last system id up until the end of this collection.
        /// <see cref="TruePath"/> is stripped of all but the last system id (e.g. to exclude the system root, but include the content root)
        /// and the result is returned.
        /// For example, the natural path of a typical homepage would include the content root and the homepage, but not include the system root.
        /// </summary>
        /// <value>The natural path.</value>
        public IEnumerable<HiveId> NaturalPath
        {
            get
            {
                // Find the last id that is a system id, and return that plus all others
                var lastSystem = TruePath.LastOrDefault(x => x.IsSystem());
                var filtered = lastSystem != default(HiveId) ? TruePath.SkipWhile(x => (x.IsSystem() && x != lastSystem)) : TruePath;
                return filtered;
            }
        }

        /// <summary>
        /// A representation of the true Hive path without filtering any ancestors.
        /// </summary>
        /// <value>The true path.</value>
        public IEnumerable<HiveId> TruePath
        {
            get
            {
                return new StringCastableEnumerable<HiveId>(Inner);
            }
        }

        /// <summary>
        /// Similar to <see cref="Level"/> but without excluding any system ids at all. Equivalent to TruePath.Count.
        /// </summary>
        /// <value>The true level.</value>
        public int TrueLevel
        {
            get
            {
                return TruePath.Count();
            }
        }
    }
}
