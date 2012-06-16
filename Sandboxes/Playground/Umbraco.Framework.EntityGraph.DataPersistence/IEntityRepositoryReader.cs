using System.Collections.Generic;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.Data.PersistenceSupport;
using Umbraco.Framework.EntityGraph.Domain;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph;

namespace Umbraco.Framework.EntityGraph.DataPersistence
{
    public interface IEntityRepositoryReader<out TEnumeratorType, out TDeepEnumeratorType, TEntityType, out TDeepEntityType>
        : IRepositoryReader<TEnumeratorType, TDeepEnumeratorType, TEntityType, TDeepEntityType>,
        ISupportsProviderInjection
        where TEnumeratorType : IEnumerable<TEntityType>
        where TDeepEnumeratorType : IEnumerable<TDeepEntityType>
        where TEntityType : class, ITypedEntity
        where TDeepEntityType : class, ITypedEntityVertex
    {
        
    }
}