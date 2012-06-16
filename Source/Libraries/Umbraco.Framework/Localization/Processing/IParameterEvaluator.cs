namespace Umbraco.Framework.Localization.Processing
{
    public interface IParameterEvaluator
    {
        ParameterValue GetValue(EvaluationContext context);
    }
      
}
