namespace Umbraco.Cms.Web.Configuration.ApplicationSettings
{
    public interface IApplication
    {
        string Name { get; }
        string Alias { get; }
        string Icon { get; }
        int Ordinal { get; }
    }
}