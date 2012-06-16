using System.Collections.Generic;
using Umbraco.Framework.Data.PersistenceSupport;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph;

namespace Umbraco.Framework.EntityGraph.DataPersistence
{
    public interface IEntityRepositoryWriter<TEnumeratorType, TDeepEnumeratorType, TEntityType, TDeepEntityType>
        : IRepositoryWriter<TEnumeratorType, TDeepEnumeratorType, TEntityType, TDeepEntityType>,
        ISupportsProviderInjection
        where TEnumeratorType : IEnumerable<TEntityType>
        where TDeepEnumeratorType : IEnumerable<TDeepEntityType>
        where TEntityType : class, ITypedEntity
        where TDeepEntityType : class, ITypedEntityVertex
    {
    }
}