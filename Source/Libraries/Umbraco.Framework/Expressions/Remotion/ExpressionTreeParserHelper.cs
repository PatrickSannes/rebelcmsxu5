using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Umbraco.Framework.Expressions.Remotion
{
    public static class ExpressionTreeParserHelper
    {
        public static CompoundNodeTypeProvider CreateDefaultNodeTypeProvider()
        {
            var types = typeof (MethodInfoBasedNodeTypeRegistry).Assembly.GetTypes();
            return new CompoundNodeTypeProvider(
                new INodeTypeProvider[]
                    {
                        MethodInfoBasedNodeTypeRegistry.CreateFromTypes(types),
                        MethodNameBasedNodeTypeRegistry.CreateFromTypes(types)
                    });
        }

        public static CompoundExpressionTreeProcessor CreateDefaultProcessor(
            IExpressionTranformationProvider tranformationProvider)
        {
            Mandate.ParameterNotNull(tranformationProvider, "tranformationProvider");
            return new CompoundExpressionTreeProcessor(
                new IExpressionTreeProcessor[]
                    {
                        new ModifiedPartialEvaluatingExpressionTreeProcessor(),
                        new TransformingExpressionTreeProcessor(tranformationProvider)
                    });
        }
    }
}