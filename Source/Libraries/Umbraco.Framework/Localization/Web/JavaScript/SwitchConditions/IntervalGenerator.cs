using System;
using Umbraco.Framework.Localization.Processing.SwitchConditions;

namespace Umbraco.Framework.Localization.Web.JavaScript.SwitchConditions
{
    public class IntervalGenerator<T> : PatternProcessorGenerator<IntervalCondition<T>> where T : struct, IComparable<T>
    {        

        public override void WriteEvaluator(IntervalCondition<T> proc, JavaScriptExpressionWriter writer, params Action[] argumentWriters)
        {
            if (proc.Min.HasValue)
            {
                argumentWriters[0]();                
                writer.Output.Write(">");
                if (proc.MinInclusive) writer.Output.Write("=");
                WriteLimit(proc.Min.Value, writer);
                if (proc.Max.HasValue) writer.Output.Write("&&");
            }
            if (proc.Max.HasValue)
            {
                argumentWriters[0]();                
                writer.Output.Write("<");
                if (proc.MaxInclusive) writer.Output.Write("=");
                WriteLimit(proc.Max.Value, writer);
            }
        }

        protected virtual void WriteLimit(T value, JavaScriptExpressionWriter writer)
        {
            WriteScriptConstant(writer, value);                
        }
    }

    public class TimespanIntervalWriter : IntervalGenerator<TimeSpan>
    {
        protected override void WriteLimit(TimeSpan value, JavaScriptExpressionWriter writer)
        {
            WriteScriptConstant(writer, (long) value.TotalMilliseconds);                
        }
    }
}
