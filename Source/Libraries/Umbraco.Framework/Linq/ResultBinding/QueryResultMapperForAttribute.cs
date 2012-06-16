namespace Umbraco.Framework.Linq.ResultBinding
{
    using System;

    public class QueryResultMapperForAttribute : Attribute
    {
        public QueryResultMapperForAttribute(Type resultType)
        {
            ResultType = resultType;
        }

        public Type ResultType { get; set; }
    }
}