namespace Umbraco.Framework.Linq.ResultBinding
{
    using System.Collections.Generic;

    public abstract class SourceFieldBinder
    {
        public abstract object Source { get; }
        public abstract object GetFieldValue(string fieldName);
        public abstract object SetFieldValue(string fieldName, object value);
        public abstract IEnumerable<string> GetFieldNames();
    }
}
