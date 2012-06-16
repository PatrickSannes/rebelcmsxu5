using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Umbraco.Framework
{
    /// <summary>The global extensions.</summary>
    public static class GlobalExtensions
    {
        #region Public Methods

        /// <summary>format the timespan</summary>
        /// <param name="timeSpan">The time span.</param>
        /// <param name="format">The format.</param>
        /// <returns>The formatted timespan.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "1#", Justification = "By Design")]
        public static string Format(this TimeSpan timeSpan, string format)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                format,
                timeSpan.Hours,
                timeSpan.Minutes,
                timeSpan.Seconds);
        }
        
        /// <summary>The if not null.</summary>
        /// <param name="item">The item.</param>
        /// <param name="action">The action.</param>
        /// <typeparam name="TItem">The type</typeparam>
        public static void IfNotNull<TItem>(this TItem item, Action<TItem> action) where TItem : class
        {
            if (item != null)
            {
                action(item);
            }
        }
        
        /// <summary>The if true.</summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="action">The action.</param>
        public static void IfTrue(this bool predicate, Action action)
        {
            if (predicate)
            {
                action();
            }
        }

        /// <summary>
        /// Checks if the item is not null, and if so returns an action on that item, or a default value
        /// </summary>
        /// <typeparam name="TResult">the result type</typeparam>
        /// <typeparam name="TItem">The type</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="action">The action.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static TResult IfNotNull<TResult, TItem>(this TItem item, Func<TItem, TResult> action, TResult defaultValue = default(TResult))
            where TItem : class
        {
            return item != null ? action(item) : defaultValue;
        }
        
        /// <summary>
        /// Checks if the value is null, if it is it returns the value specified, otherwise returns the non-null value
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="item"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TItem IfNull<TItem>(this TItem item, Func<TItem, TItem> action)
            where TItem : class
        {
            return item ?? action(item);
        }

        

        #endregion
    }
}