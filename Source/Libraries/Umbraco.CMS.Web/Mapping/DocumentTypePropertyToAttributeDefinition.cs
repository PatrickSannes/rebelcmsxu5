using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.Metadata;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    internal class DocumentTypePropertyToAttributeDefinition : TypeMapper<DocumentTypeProperty, AttributeDefinition>
    {
        private readonly MapResolverContext _resolverContext;

        public DocumentTypePropertyToAttributeDefinition(
            AbstractFluentMappingEngine engine, 
            MapResolverContext resolverContext,
            bool mapAttributeGroup = true)
            : base(engine)
        {
            _resolverContext = resolverContext;

            MappingContext
                .CreateUsing(x => new AttributeDefinition(x.Alias, x.Name))
                .MapMemberFrom(x => x.Ordinal, x => x.SortOrder)
                .MapMemberUsing(x => x.AttributeType, new DocumentTypePropertyToAttributeType(engine, _resolverContext))
                .MapMemberUsing(x => x.AttributeGroup, new DocumentTypePropertyToAttributeGroup(engine, _resolverContext, !mapAttributeGroup))
                .ForMember(x => x.UtcModified, opt => opt.MapUsing<UtcModifiedMapper>())
                .ForMember(x => x.UtcCreated, opt => opt.MapUsing<UtcCreatedMapper>())
                .AfterMap((source, dest) =>
                    {
                        if (dest.AttributeType == null)
                        {
                            throw new InvalidOperationException("The DataType property of the DocumentTypeProperty object cannot be null when mapping from DocumentTypeProperty -> AttributeDefinition");
                        }

                        //This will check if the pre-value editor has overriding field capabilities. 
                        //If it does, it will persist these values to the RenderTypeProviderConfigOverride instead of the underlying 
                        //data type.
                        if (source.PreValueEditor != null)
                        {
                            var preValueMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => source.PreValueEditor, source.GetType());
                            //checks if the model contains overrides
                            var hasOverrides = preValueMetadata
                                .Properties
                                .Any(metaProp =>
                                     metaProp.AdditionalValues.ContainsKey(UmbracoMetadataAdditionalInfo.AllowDocumentTypePropertyOverride.ToString()));

                            //if this pre-value model has overrides, then persist them
                            if (hasOverrides)
                            {
                                dest.RenderTypeProviderConfigOverride = source.PreValueEditor.GetSerializedValue();
                            }
                        }

                    });
        }
    }
}
