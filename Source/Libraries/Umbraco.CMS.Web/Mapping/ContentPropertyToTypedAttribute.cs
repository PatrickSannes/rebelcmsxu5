using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.TypeMapping;
using EditorModel = Umbraco.Cms.Web.Model.BackOffice.PropertyEditors.EditorModel;

namespace Umbraco.Cms.Web.Mapping
{
    internal class ContentPropertyToTypedAttribute : TypeMapper<ContentProperty, TypedAttribute>
    {
        private readonly bool _ignoreAttributeDef;

        public ContentPropertyToTypedAttribute(AbstractFluentMappingEngine engine, bool ignoreAttributeDef = false)
            : base(engine)
        {
            _ignoreAttributeDef = ignoreAttributeDef;

            MappingContext
                .ForMember(x => x.UtcCreated, opt => opt.MapUsing<UtcCreatedMapper>())
                .ForMember(x => x.UtcModified, opt => opt.MapUsing<UtcModifiedMapper>())
                .ForMember(x => x.AttributeDefinition, opt => opt.MapFrom(x => _ignoreAttributeDef ? null : x.DocTypeProperty))
                .AfterMap((source, dest) =>
                    {
                        var propEditor = ((EditorModel)source.PropertyEditorModel);
                        if (propEditor != null)
                        {
                            dest.Values.Clear();
                            foreach (var i in propEditor.GetSerializedValue())
                            {
                                dest.Values.Add(i.Key, i.Value);
                            }
                        }
                        else
                        {
                            dest.DynamicValue = null;
                        }

                    });
        }
    }
}