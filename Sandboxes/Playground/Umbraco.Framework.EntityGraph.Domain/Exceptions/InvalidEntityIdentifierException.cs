using System;
using System.Runtime.Serialization;

namespace Umbraco.Framework.EntityGraph.Domain.Exceptions
{
    [Serializable]
    public class InvalidEntityIdentifierException : Exception
    {
        public InvalidEntityIdentifierException(IMappedIdentifier identifier)
            : this(identifier, string.Empty)
        {
        }

        public InvalidEntityIdentifierException(IMappedIdentifier identifier, string message)
            : this(identifier, message, null)
        {
        }

        public InvalidEntityIdentifierException(IMappedIdentifier identifier, string message, Exception inner)
            : base(message, inner)
        {
            Id = identifier;
        }

        protected InvalidEntityIdentifierException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }

        public IMappedIdentifier Id { get; set; }
    }
}