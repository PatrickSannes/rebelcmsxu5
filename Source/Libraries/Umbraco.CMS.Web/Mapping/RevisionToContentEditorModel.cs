using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    internal class RevisionToContentEditorModel<TContentModel> : TypeMapper<Revision<TypedEntity>, TContentModel>
        where TContentModel : StandardContentEditorModel
    {
        public RevisionToContentEditorModel(AbstractFluentMappingEngine engine)
            : base(engine)
        {
            MappingContext
                .CreateUsing(x =>
                    {
                        var output = MappingContext.Engine.Map<TypedEntity, TContentModel>(x.Item);
                        output.RevisionId = x.MetaData.Id;
                        return output;
                    });
        }
    }
}