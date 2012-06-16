using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

namespace Umbraco.Framework.Linq.QueryModel
{
    public class SortClause
    {
        public SortClause() // For Json deserialization
        {
            FieldSelector = FieldSelectorExpression.Empty;
            Priority = 0;
            Direction = SortDirection.Indeterminate;
        }

        public SortClause(FieldSelectorExpression fieldName, SortDirection direction, int priority)
        {
            FieldSelector = fieldName;
            Direction = direction;
            Priority = priority;
        }

        public SortClause(FieldSelectorExpression fieldName)
            : this(fieldName, SortDirection.Indeterminate, 0)
        {
        }

        public int Priority { get; set; }
        public FieldSelectorExpression FieldSelector { get; set; }
        public SortDirection Direction { get; set; }
    }
}