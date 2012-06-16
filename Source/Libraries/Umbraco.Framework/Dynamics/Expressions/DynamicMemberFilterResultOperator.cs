namespace Umbraco.Framework.Dynamics.Expressions
{
    using System.Linq.Expressions;
    using Remotion.Linq.Clauses;
    using Umbraco.Framework.Expressions.Remotion;
    using Umbraco.Framework.Linq.QueryModel;

    public class DynamicMemberFilterResultOperator : AbstractExtensionResultOperator
    {
        public DynamicMemberFilterResultOperator(Expression parameter)
            : base(parameter)
        {
        }

        #region Overrides of AbstractExtensionResultOperator

        public override void ModifyQueryDescription(QueryDescription queryDescription)
        {
            var constant = Parameter as ConstantExpression;
            //queryDescription.From.RequiredEntityIds = (IEnumerable<HiveId>)(constant.Value);
        }

        public override string Name
        {
            get
            {
                return "DynamicMemberFilter";
            }
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new DynamicMemberFilterResultOperator(Parameter);
        }

        #endregion
    }
}