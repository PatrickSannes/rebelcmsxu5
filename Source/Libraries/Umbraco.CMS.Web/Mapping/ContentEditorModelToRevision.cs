using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    internal class ContentEditorModelToRevision<TContentModel> : TypeMapper<TContentModel, Revision<TypedEntity>>
        where TContentModel : StandardContentEditorModel
    {
        public ContentEditorModelToRevision(AbstractFluentMappingEngine engine)
            : base(engine)
        {
            MappingContext
                .CreateUsing(x => new Revision<TypedEntity>()
                    {
                        Item = MappingContext.Engine.Map<TContentModel, TypedEntity>(x),
                        //TODO: Is this correctly mapped ??
                        MetaData = new RevisionData(x.RevisionId, FixedStatusTypes.Draft)
                    });

        }


    }
}