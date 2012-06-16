using System;
using System.Linq;

namespace Umbraco.Hive.ProviderGrouping
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class RepositoryTypeAttribute : Attribute
    {
        public RepositoryTypeAttribute(string groupUri)
        {
            ProviderGroupRoot = new Uri(groupUri);
        }

        public Uri ProviderGroupRoot { get; private set; }

        public static RepositoryTypeAttribute GetFrom(Type type)
        {
            return type.GetCustomAttributes(typeof(RepositoryTypeAttribute), true)
                .Cast<RepositoryTypeAttribute>()
                .SingleOrDefault();
        }

        public static RepositoryTypeAttribute GetFrom<T>()
        {
            return GetFrom(typeof(T));
        }
    }

    //public interface ISchemaRepository : IReadonlySchemaRepository
    //{
    //    void AddOrUpdateSchemaPart<T>(T item) where T : AbstractSchemaPart;
    //}

    //public interface IReadonlySchemaRepository : ICoreReadonlyRepository<EntitySchema>
    //{
    //    T GetSchemaPart<T>(HiveId id) where T : AbstractSchemaPart;
    //}
}
