using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using System.Linq;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Framework;
using Umbraco.Cms.Web.Model;

namespace Umbraco.Cms.Web
{
    using Umbraco.Hive;

    public static class CmsHiveManagerExtensions
    {
        public static IRenderViewHiveManagerWrapper Cms(this IHiveManager hiveManager)
        {
            return new RenderViewHiveManagerWrapper(hiveManager);
        }

        public static ISchemaBuilderStep<T, TProviderFilter> NewContentType<T, TProviderFilter>(
            this IRenderViewHiveManagerWrapper hiveManager,
            string alias)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            var schema = hiveManager.NewSchema<T, TProviderFilter>(alias);
            schema.Item.RelationProxies.EnlistParentById(FixedHiveIds.ContentRootSchema, FixedRelationTypes.DefaultRelationType);
            return schema;
        }

        public static ISchemaBuilderStep<T, TProviderFilter> NewMediaType<T, TProviderFilter>(
            this IRenderViewHiveManagerWrapper hiveManager,
            string alias)
            where TProviderFilter : class, IProviderTypeFilter
            where T : EntitySchema, new()
        {
            var schema = hiveManager.NewSchema<T, TProviderFilter>(alias);
            schema.Item.RelationProxies.EnlistParentById(FixedHiveIds.MediaRootSchema, FixedRelationTypes.DefaultRelationType);
            return schema;
        }


        public static IQueryable<TResult> Query<TResult, TProviderFilter>(this IHiveManager hiveManager)
            where TResult : class, IReferenceByHiveId
            where TProviderFilter : class, IProviderTypeFilter
        {
            using (var uow = hiveManager.OpenReader<TProviderFilter>())
            {
                return uow.Repositories.Select(x => hiveManager.FrameworkContext.TypeMappers.Map<TResult>(x));
            }
        }

        public static IQueryable<T> QueryMedia<T>(this IHiveManager hiveManager)
            where T : class, IReferenceByHiveId
        {
            return hiveManager.Query<T, IMediaStore>();
        }

        public static IQueryable<Media> QueryMedia(this IHiveManager hiveManager)
        {
            return QueryMedia<Media>(hiveManager);
        }

        public static IQueryable<T> QueryContent<T>(this IHiveManager hiveManager)
            where T : class, IReferenceByHiveId
        {
            return hiveManager.Query<T, IContentStore>();
        }

        public static IQueryable<Content> QueryContent(this IHiveManager hiveManager)
        {
            return QueryContent<Content>(hiveManager);
        }

        public static T GetById<T>(this IQueryable<T> source, HiveId id)
            where T : class, IReferenceByHiveId
        {
            return source.FirstOrDefault(x => x.Id == id);
        }

        public static T GetById<T>(this IQueryable<T> source, string idAsString)
            where T : class, IReferenceByHiveId
        {
            var parsed = HiveId.TryParse(idAsString);
            if (!parsed.Success) return null;
            return source.FirstOrDefault(x => x.Id == parsed.Result);
        }
    }
}
