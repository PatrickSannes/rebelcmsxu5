using System;
using Umbraco.Framework.Localization.Processing.ParameterEvaluators;

namespace Umbraco.Framework.Localization.Web.JavaScript.ParameterEvaluators
{
    public class SimpleParameterGenerator : PatternProcessorGenerator<SimpleParameterEvaluator>
    {        

        public override void WriteEvaluator(SimpleParameterEvaluator proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {            
            WriteGetParameter(writer, writer.Json.Serialize(proc.ParameterName));            
        }
    }
}
