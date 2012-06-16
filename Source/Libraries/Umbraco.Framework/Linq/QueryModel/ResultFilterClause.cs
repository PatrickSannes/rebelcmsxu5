namespace Umbraco.Framework.Linq.QueryModel
{
    using System;

    public class ResultFilterClause
    {
        public ResultFilterClause(Type resultType, ResultFilterType resultFilterType, int selectorArgument)
        {
            ResultType = resultType;
            ResultFilterType = resultFilterType;
            SelectorArgument = selectorArgument;
        }

        public ResultFilterClause()
            : this(typeof(object), ResultFilterType.Sequence, -1)
        {
        }

        public Type ResultType { get; set; }

        public ResultFilterType ResultFilterType { get; set; }

        // i.e. what gets passed into the Count selector
        public int SelectorArgument { get; set; }
    }
}