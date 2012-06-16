using System;
using System.Linq;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using System.Collections.Generic;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    /// <summary>
    /// Used to map a *New* BasicContentEditorModel type from a DocumentTypeEditorModel
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    internal class DocumentTypeToNewContentModel<TTarget> : TypeMapper<DocumentTypeEditorModel, TTarget>
        where TTarget : BasicContentEditorModel
    {
        public DocumentTypeToNewContentModel(AbstractFluentMappingEngine engine,
                                             Action<DocumentTypeEditorModel, TTarget> additionalAfterMap = null)
            : base(engine)
        {
            //set all of the member expressions
            MappingContext
                .ForMember(x => x.DocumentTypeName, opt => opt.MapFrom(x => x.Name))
                .ForMember(x => x.DocumentTypeId, opt => opt.MapFrom(x => x.Id))
                .ForMember(x => x.Id, opt => opt.Ignore())
                .ForMember(x => x.ActiveTabIndex, opt => opt.Ignore())
                .ForMember(x => x.UpdatedBy, opt => opt.Ignore())
                .ForMember(x => x.CreatedBy, opt => opt.Ignore())
                .ForMember(x => x.UtcCreated, opt => opt.Ignore())
                .ForMember(x => x.UtcModified, opt => opt.Ignore())
                .ForMember(x => x.ParentId, opt => opt.Ignore())
                .ForMember(x => x.Name, opt => opt.Ignore())
                .ForMember(x => x.Properties, opt => opt.MapFrom(x => x.Properties))
                .ForMember(x => x.NoticeBoard, x => x.Ignore())
                .ForMember(x => x.UIElements, x => x.Ignore())
                .AfterMap((source, dest) =>
                    {
                        dest.DocumentTypeId = source.Id;
                        dest.DocumentTypeName = source.Name;
                        dest.DocumentTypeAlias = source.Alias;
                        dest.Tabs = MappingContext.Engine.Map<HashSet<Tab>, HashSet<Tab>>(source.DefinedTabs);

                        //now, update the PropertyEditor context for each property with this item
                        foreach (var p in dest.Properties.Where(p => p.DocTypeProperty.DataType.InternalPropertyEditor != null)
                            .Where(p => p.DocTypeProperty.DataType.InternalPropertyEditor is IContentAwarePropertyEditor))
                        {
                            var contentAwarePropEditor = (IContentAwarePropertyEditor)p.DocTypeProperty.DataType.InternalPropertyEditor;
                            contentAwarePropEditor.SetDocumentType(source);
                        }

                        if (additionalAfterMap != null)
                        {
                            additionalAfterMap(source, dest);
                        }

                    });
        }

    }
}