using Umbraco.Framework.Persistence.Model.Versioning;

namespace Umbraco.Hive.ProviderSupport
{
    public interface IProviderRevisionRepository<in TBaseEntity>
        : IReadonlyProviderRevisionRepository<TBaseEntity>, ICoreRevisionRepository<TBaseEntity>
        where TBaseEntity : class, IVersionableEntity
    {
    }
}