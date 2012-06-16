using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Umbraco.Framework.Dynamics.Expressions;

namespace Umbraco.Framework.Expressions.Remotion
{
    using System.Linq;
    using Umbraco.Framework.Diagnostics;
    using global::Remotion.Linq.Parsing.Structure.NodeTypeProviders;

    /// <summary>
    /// Heavily based on the default <see cref="QueryParser"/> normally used in Relinq, but primary difference is that we
    /// need to establish a different <see cref="ExpressionTreeParser"/> with a custom <see cref="DynamicTransformingExpressionTreeProcessor"/>
    /// in place of the default <see cref="TransformingExpressionTreeProcessor"/> in order to avoid it from precompiling certain expressions
    /// such as those referencing <see cref="DynamicMemberMetadata.GetMemberMethod"/> in order to reference dynamic operations inside
    /// regular .NET 3.5 expression trees. Relinq doesn't support <see cref="DynamicExpression"/> from .NET 4 at time of writing.
    /// </summary>
    public class CustomQueryParser : IQueryParser
    {
        private readonly ExpressionTreeParser _expressionTreeParser;

        public CustomQueryParser(ExpressionTreeParser expressionTreeParser)
        {
            Mandate.ParameterNotNull(expressionTreeParser, "expressionTreeParser");
            _expressionTreeParser = expressionTreeParser;
        }

        public ExpressionTreeParser ExpressionTreeParser { get { return _expressionTreeParser; } }

        public INodeTypeProvider NodeTypeProvider { get { return _expressionTreeParser.NodeTypeProvider; } }

        public IExpressionTreeProcessor Processor { get { return _expressionTreeParser.Processor; } }

        #region IQueryParser Members

        public QueryModel GetParsedQuery(Expression expressionTreeRoot)
        {
            Mandate.ParameterNotNull(expressionTreeRoot, "expressionTreeRoot");

            EnsureCustomModifiersRegisteredWithRelinq();

            return ApplyAllNodes(_expressionTreeParser.ParseTree(expressionTreeRoot), new ClauseGenerationContext(_expressionTreeParser.NodeTypeProvider));
        }

        /// <summary>
        /// Ensures the custom modifiers are registered with Relinq.
        /// </summary>
        protected void EnsureCustomModifiersRegisteredWithRelinq()
        {
            var nodeTypeProvider = NodeTypeProvider as CompoundNodeTypeProvider;
            if (nodeTypeProvider == null) return;

            var firstRegistry = nodeTypeProvider.InnerProviders.OfType<MethodInfoBasedNodeTypeRegistry>().FirstOrDefault();
            if (firstRegistry == null) return;

            foreach (var registration in ExpressionNodeModifierRegistry.Current.Registrations.Where(x => !firstRegistry.IsRegistered(x.MethodInfo)))
            {
                firstRegistry.Register(registration.MethodInfo.AsEnumerableOfOne(), registration.Type);
            }
        }

        #endregion

        public static CustomQueryParser CreateDefault()
        {
            var compoundNodeTypeProvider = ExpressionTreeParser.CreateDefaultNodeTypeProvider();

            return
                new CustomQueryParser(
                    new ExpressionTreeParser(
                        compoundNodeTypeProvider,
                        ExpressionTreeParserHelper.CreateDefaultProcessor(
                            ExpressionTransformerRegistry.CreateDefault())));
        }

        private QueryModel ApplyAllNodes(IExpressionNode node, ClauseGenerationContext clauseGenerationContext)
        {
            QueryModel queryModel = null;
            if (node.Source != null)
                queryModel = ApplyAllNodes(node.Source, clauseGenerationContext);
            return node.Apply(queryModel, clauseGenerationContext);
        }
    }
}