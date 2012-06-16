using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Mapping
{
    internal class ContentEditorModelToEntitySchema<TInput> : StandardMemberMapper<TInput, EntitySchema>
         where TInput : BasicContentEditorModel
    {
        public ContentEditorModelToEntitySchema(AbstractFluentMappingEngine currentEngine, MapResolverContext context)
            : base(currentEngine, context)
        {
        }

        public override EntitySchema GetValue(TInput source)
        {
            if (source.DocumentTypeId.IsNullValueOrEmpty()) return null;

            using (var uow = ResolverContext.Hive.OpenReader<IContentStore>())
            {
                var entity = uow.Repositories.Schemas.GetComposite<EntitySchema>(source.DocumentTypeId);
                return entity;
            }
        }
    }
}