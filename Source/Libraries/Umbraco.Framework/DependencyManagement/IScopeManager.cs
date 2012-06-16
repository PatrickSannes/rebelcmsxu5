namespace Umbraco.Framework.DependencyManagement
{
    public interface IScopeManager<T>
    {
        IDependencyRegistrar<T> Singleton();
        IDependencyRegistrar<T> ForLifetime(string lifetimeName);
        IDependencyRegistrar<T> NewInstanceEachTime();
        IDependencyRegistrar<T> HttpRequest();
    }
}