using Umbraco.Framework.Data.PersistenceSupport;
using Umbraco.Framework.EntityGraph.Domain.EntityAdaptors.ObjectModel;

namespace Umbraco.Framework.EntityGraph.Domain.EntityAdaptors
{
    public interface IContentResolver
        : IRepositoryReader<TypedEntityAdaptorCollection<IContentEntity>, TypedEntityAdaptorGraph<IContentVertex>, IContentEntity, IContentVertex>, ISupportsProviderInjection
    {
        //EntityRepositoryReader RepositoryReader { get; set; }
        //IEntityRepositoryWriter RepositoryWriter { get; set; }

        //IEntityAdaptorVertex<IContentVertex> Get(IMappedIdentifier identifier);
        //EntityAdaptorGraph<IContentVertex> Get(IEnumerable<IMappedIdentifier> identifiers);
        //EntityAdaptorGraph<IContentVertex> GetAll();

        //IEntityAdaptorVertex<IContentVertex> Get(IMappedIdentifier identifier, int traversalDepth);
        //EntityAdaptorGraph<IContentVertex> Get(IEnumerable<IMappedIdentifier> identifiers, int traversalDepth);
        //EntityAdaptorGraph<IContentVertex> GetAll(int traversalDepth);
    }
}