namespace Umbraco.Framework.Configuration
{
    public interface IConfigurationResolver
    {
        object GetConfigSection(string name);
    }
}