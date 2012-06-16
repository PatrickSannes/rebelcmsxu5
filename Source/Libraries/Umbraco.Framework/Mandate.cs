using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework.Localization;

namespace Umbraco.Framework
{
    /// <summary>
    /// Helper class for mandating values, for example on method parameters.
    /// </summary>
    public static class Mandate
    {
        /// <summary>
        /// Mandates that the specified parameter is not null.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="paramName">Name of the param.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null.</exception>
        public static void ParameterNotNull<T>(T value, string paramName) where T : class 
        {
            That(value != null, x => new ArgumentNullException(paramName));
        }

        /// <summary>
        /// Mandates that the specified parameter is not empty.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="paramName">Name of the param.</param>
        /// <remarks></remarks>
        public static void ParameterNotEmpty(HiveId value, string paramName)
        {
            That(value != HiveId.Empty, x => new ArgumentNullException(paramName));
        }

        public static void ParameterNotEmpty(HiveId? value, string paramName)
        {
            That(value.HasValue && value != HiveId.Empty, x => new ArgumentNullException(paramName));
        }

        /// <summary>
        /// Mandates that the specified parameter is not empty
        /// </summary>
        /// <param name="value"></param>
        /// <param name="paramName"></param>
        public static void ParameterNotEmpty(HiveIdValue value, string paramName)
        {
            That(value != HiveIdValue.Empty, x => new ArgumentNullException(paramName));
        }

        /// <summary>
        /// Mandates that the specified parameter is not null.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="paramName">Name of the param.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is null or whitespace.</exception>
        public static void ParameterNotNullOrEmpty(string value, string paramName)
        {
            That(!string.IsNullOrWhiteSpace(value), x => new ArgumentNullException(paramName));
        }

        /// <summary>
        /// Mandates that the specified sequence is not null and has at least one element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="paramName">Name of the param.</param>
        public static void ParameterNotNullOrEmpty<T>(IEnumerable<T> sequence, string paramName)
        {
            ParameterNotNull(sequence, paramName);
            ParameterCondition(sequence.Any(), paramName);
        }

        /// <summary>
        /// Mandates that the specified parameter matches the condition.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="paramName">Name of the param.</param>
        /// <exception cref="ArgumentException">If the condition is false.</exception>
        public static void ParameterCondition(bool condition, string paramName)
        {
            That(condition, x => new ArgumentException(paramName));
        }

        /// <summary>
        /// Mandates that the specified condition is true, otherwise throws an exception specified in <typeparamref name="TException"/>.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="condition">if set to <c>true</c>, throws exception <typeparamref name="TException"/>.</param>
        /// <exception cref="Exception">An exception of type <typeparamref name="TException"/> is raised if the condition is false.</exception>
        public static void That<TException>(bool condition) where TException : Exception, new()
        {
            if (!condition)
                throw ActivatorHelper.CreateInstance<TException>();
        }

        /// <summary>
        /// Mandates that the specified condition is true, otherwise throws an exception specified in <typeparamref name="TException"/>.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="condition">if set to <c>true</c>, throws exception <typeparamref name="TException"/>.</param>
        /// <param name="defer">Deffered expression to call if the exception should be raised.</param>
        /// <exception cref="Exception">An exception of type <typeparamref name="TException"/> is raised if the condition is false.</exception>
        public static void That<TException>(bool condition, Func<TextManager, TException> defer) where TException : Exception, new()
        {
            if (!condition)
            {
                throw defer.Invoke(null); // TODO: Replace null with Localization<TException>.Current once Localization stable
            }

            // Here is an example of how this method is actually called
            //object myParam = null;
            //Mandate.That(myParam != null,
            //     textManager => new ArgumentNullException(textManager.Get("blah", new {User = "blah"})));
        }
    }
}
