namespace Umbraco.Cms.Web.Configuration.Tasks
{
    public interface ITaskParameter
    {
        string Name { get; }
        string Value { get; }
    }
}