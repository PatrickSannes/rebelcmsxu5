using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;

namespace Umbraco.Hive
{
    public interface ICoreRelationsRepository 
        : ICoreReadonlyRelationsRepository
    {
        void AddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item);
        void RemoveRelation(IRelationById item);
    }
}