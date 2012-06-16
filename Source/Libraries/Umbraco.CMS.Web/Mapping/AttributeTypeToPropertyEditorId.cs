using System;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    /// <summary>
    /// Resolves a Property editor Id from an AttributeType
    /// </summary>
    internal class AttributeTypeToPropertyEditorId : StandardMemberMapper<AttributeType, Guid>
    {
        public AttributeTypeToPropertyEditorId(AbstractFluentMappingEngine currentEngine, MapResolverContext context)
            : base(currentEngine, context)
        {
        }

        public override Guid GetValue(AttributeType source)
        {
            Guid output;
            var guid = Guid.TryParse(source.RenderTypeProvider, out output) ? output : Guid.Empty;

            if (guid != Guid.Empty)
            {
                var editor = ResolverContext.PropertyEditorFactory.GetPropertyEditor(guid);
                if (editor != null)
                {
                    return editor.Metadata.Id;
                }
            }
            return Guid.Empty;
        }
    }
}