using System;

namespace Umbraco.Framework.DependencyManagement
{
    public class NamedResolutionAttempt<T> : ResolutionAttempt<T> where T : class
    {
        public NamedResolutionAttempt(string key, bool success, T item) : base(success, item)
        {
            Key = key;
        }

        public NamedResolutionAttempt(string key, Exception error) : base(error)
        {
            Key = key;
        }

        public string Key { get; protected set; }
    }

    /// <summary>
    /// Represnets an IoC resolution attempt
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResolutionAttempt<T> where T : class
    {
        private readonly bool _success;
        private readonly T _item;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolutionAttempt&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="item">The item.</param>
        public ResolutionAttempt(bool success, T item)
        {
            _success = success;
            _item = item;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolutionAttempt&lt;T&gt;"/> class when an error occurs and was not-successful
        /// </summary>
        /// <param name="error">The error.</param>
        public ResolutionAttempt(Exception error)
        {
            _success = false;
            _item = null;
            Error = error;
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Updates the error on this resolution attempt.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void UpdateError(Exception exception)
        {
            Error = exception;
        }

        /// <summary>
        /// The success of the resolution attempt.
        /// </summary>
        public bool Success { get { return _success; } }

        /// <summary>
        /// The resulting value of the resolution attempt, if any.
        /// </summary>
        public T Value
        {
            get { return _item; }
        }

        public override string ToString()
        {
            return _success + "," + _item;
        }
    }
}