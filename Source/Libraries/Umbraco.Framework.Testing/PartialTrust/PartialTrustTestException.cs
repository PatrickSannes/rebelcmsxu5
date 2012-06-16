using System;
using System.Runtime.Serialization;

namespace Umbraco.Framework.Testing.PartialTrust
{
    [Serializable]
    public class PartialTrustTestException : Exception
    {
        protected PartialTrustTestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PartialTrustTestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}