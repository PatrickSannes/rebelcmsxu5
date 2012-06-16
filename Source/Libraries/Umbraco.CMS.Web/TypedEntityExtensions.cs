using System.Linq;

using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Hive;
namespace Umbraco.Cms.Web
{
    public static class TypedEntityExtensions
    {
        /// <summary>
        /// Determines whether the specified entity is content.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="uow"></param>
        /// <returns>
        ///   <c>true</c> if the specified entity is content; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsContent(this TypedEntity entity, IReadonlyGroupUnit<IContentStore> uow)
        {
            var ancestorRelations = uow.Repositories.GetAncestorRelations(entity.Id, FixedRelationTypes.DefaultRelationType).ToArray();
            return ancestorRelations.Any(x => x.SourceId.Value == FixedHiveIds.ContentVirtualRoot.Value);
        }

        /// <summary>
        /// Determines whether the specified entity is media.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="uow"></param>
        /// <returns>
        ///   <c>true</c> if the specified entity is media; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMedia(this TypedEntity entity, IReadonlyGroupUnit<IContentStore> uow)
        {
            return uow.Repositories.GetAncestorRelations(entity.Id, FixedRelationTypes.DefaultRelationType).Any(x => x.SourceId.Value == FixedHiveIds.MediaVirtualRoot.Value);
        }
    }
}
