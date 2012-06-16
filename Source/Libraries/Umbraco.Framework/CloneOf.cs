namespace Umbraco.Framework
{
    /// <summary>
    /// Represents a clone of another object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CloneOf<T> where T : class
    {
        internal CloneOf(bool partialTrustCausedPartialClone, T value)
        {
            PartialTrustCausedPartialClone = partialTrustCausedPartialClone;
            Value = value;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Umbraco.Framework.CloneOf{T}"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="other">The original item.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator T(CloneOf<T> other)
        {
            return other.Value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether full cloning was not possible due to partial trust restricting access to private members.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if partial trust caused a partial clone; otherwise, <c>false</c>.
        /// </value>
        public bool PartialTrustCausedPartialClone { get; private set; }

        /// <summary>
        /// Gets the cloned value.
        /// </summary>
        /// <value>The value.</value>
        public T Value { get; private set; }
    }
}