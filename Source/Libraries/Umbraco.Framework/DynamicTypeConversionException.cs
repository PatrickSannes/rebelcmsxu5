using System;

namespace Umbraco.Framework
{
    public class DynamicTypeConversionException : Exception
    {
        public DynamicTypeConversionException(string message) : base(message)
        {
        }
    }
}