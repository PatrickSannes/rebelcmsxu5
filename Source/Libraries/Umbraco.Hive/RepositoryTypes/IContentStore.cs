using Umbraco.Hive.ProviderGrouping;

namespace Umbraco.Hive.RepositoryTypes
{
    [RepositoryType("content://")]
    public interface IContentStore : IProviderTypeFilter
    {
        
    }
}