using Umbraco.Framework.Persistence.Model.Versioning;

namespace Umbraco.Hive.Configuration
{
    public class RevisionTypeLoaderElement<T> : PersistenceTypeLoaderElement
        where T : IVersionableEntity
    {
        
    }
}