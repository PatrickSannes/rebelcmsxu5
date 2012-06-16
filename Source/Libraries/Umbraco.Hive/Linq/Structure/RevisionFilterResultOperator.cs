namespace Umbraco.Hive.Linq.Structure
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Remotion.Linq.Clauses;
    using Umbraco.Framework;
    using Umbraco.Framework.Expressions.Remotion;
    using Umbraco.Framework.Linq.QueryModel;

    public class RevisionFilterResultOperator : AbstractExtensionResultOperator
    {
        public RevisionFilterResultOperator(Expression parameter)
            : base(parameter)
        {
        }

        #region Overrides of AbstractExtensionResultOperator

        public override void ModifyQueryDescription(QueryDescription queryDescription)
        {
            var constant = Parameter as ConstantExpression;
            queryDescription.From.RevisionStatusType = (RevisionStatusType)(constant.Value);
        }

        public override string Name
        {
            get
            {
                return "RevisionFilter";
            }
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new RevisionFilterResultOperator(Parameter);
        }

        #endregion
    }

    public class IdFilterResultOperator : AbstractExtensionResultOperator
    {
        public IdFilterResultOperator(Expression parameter)
            : base(parameter)
        {
        }

        #region Overrides of AbstractExtensionResultOperator

        public override void ModifyQueryDescription(QueryDescription queryDescription)
        {
            var constant = Parameter as ConstantExpression;
            queryDescription.From.RequiredEntityIds = (IEnumerable<HiveId>)(constant.Value);
        }

        public override string Name
        {
            get
            {
                return "IdFilter";
            }
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new IdFilterResultOperator(Parameter);
        }

        #endregion
    }
}