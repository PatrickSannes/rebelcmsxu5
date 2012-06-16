namespace Umbraco.Framework.DependencyManagement
{
    public static class DependencyRegistrarExtensions
    {
        public static IDependencyRegistrar<T> KnownAsSelf<T>(this IDependencyRegistrar<T> registrar)
        {
            return registrar.KnownAs(registrar.RawType);
        }

        public static IDependencyRegistrar<T> NamedForSelf<T>(this IDependencyRegistrar<T> registrar, string name)
        {
            return registrar.Named(name, registrar.RawType);
        }
    }
}