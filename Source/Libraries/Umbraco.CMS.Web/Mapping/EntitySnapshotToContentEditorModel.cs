using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;

namespace Umbraco.Cms.Web.Mapping
{
    internal class EntitySnapshotToContentEditorModel<TContentModel> : TypeMapper<EntitySnapshot<TypedEntity>, TContentModel>
        where TContentModel : StandardContentEditorModel
    {
        public EntitySnapshotToContentEditorModel(AbstractFluentMappingEngine engine)
            : base(engine)
        {

            MappingContext
                .CreateUsing(x => MappingContext.Engine.Map<Revision<TypedEntity>, TContentModel>(x.Revision))
                .AfterMap(
                    (source, dest) =>
                    {
                        dest.UtcPublishedDate = source.PublishedDate();
                    });

        }


    }
}