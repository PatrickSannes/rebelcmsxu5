using System;

namespace Umbraco.Framework
{
    public class UnreachableException : NotImplementedException
    {
        public UnreachableException()
            : base("This method is used for expression creation only")
        {
        }
    }
}