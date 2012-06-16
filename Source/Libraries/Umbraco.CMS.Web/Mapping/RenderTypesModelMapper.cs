using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Mapping
{
    using Umbraco.Framework.Persistence.Model.Constants;

    public sealed class RenderTypesModelMapper : AbstractFluentMappingEngine
    {
        private readonly MapResolverContext _resolverContext;

        public RenderTypesModelMapper(MapResolverContext resolverContext)
            : base(resolverContext.FrameworkContext)
        {
            _resolverContext = resolverContext;
        }

        public override void ConfigureMappings()
        {
            #region TypedEntity -> Content

            this.CreateMap<TypedEntity, Content>()
                .CreateUsing(x => new Content(x.Id, x.Attributes))
                //.ForMember(x => x.Attributes, opt => opt.MapFrom(x => x.Attributes))
                //.ForMember(x => x.ContentType, opt => opt.MapFrom(x => x.EntitySchema))
                .AfterMap((source, dest) =>
                    {
                        var contentHive = _resolverContext.Hive.GetReader<IContentStore>();
                        using (var uow = contentHive.CreateReadonly())
                        {
                            var siblings = uow.Repositories.GetBranchRelations(dest.Id, FixedRelationTypes.DefaultRelationType);
                            var currentPlace = siblings.FirstOrDefault(x => x.DestinationId == dest.Id);
                            if (currentPlace != null) dest.SortOrder = currentPlace.Ordinal;
                        }

                    var fileHive = _resolverContext.Hive.TryGetReader<IFileStore>(new Uri("storage://templates"));
                    if (fileHive == null) return;
                    using (var uow = fileHive.CreateReadonly())
                    {
                        //selected template
                        var templateVal = source.GetAttributeValueAsString(SelectedTemplateAttributeDefinition.AliasValue);
                        if (!templateVal.IsNullOrWhiteSpace())
                        {
                            var selectedTemplateId = new HiveId(source.GetAttributeValueAsString(SelectedTemplateAttributeDefinition.AliasValue));
                            if (!selectedTemplateId.IsNullValueOrEmpty())
                            {
                                var selectedTemplate = uow.Repositories.Get<File>(selectedTemplateId);
                                if (selectedTemplate != null)
                                {
                                    dest.CurrentTemplate = FrameworkContext.TypeMappers.Map<Template>(selectedTemplate);
                                }
                            }
                        }
                        
                        //alternative templates
                        var altTemplateIds = source.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates") ?? new List<HiveId>();
                        var altTemplates = uow.Repositories.Get<File>(true, altTemplateIds.ToArray());
                        dest.AlternativeTemplates = altTemplates.Select(x => FrameworkContext.TypeMappers.Map<Template>(x));
                    }
                });

            #endregion

            #region File -> Template

            this.CreateMap<File, Template>()
                .CreateUsing(x => new Template(x.Id))
                .ForMember(x => x.Name, x => x.MapFrom(y => y.Name))
                .ForMember(x => x.Alias, x => x.MapFrom(y => y.GetFileNameWithoutExtension()));

            #endregion

            #region TypedAttributeCollection > IEnumerable<Field>

            this.CreateMap<TypedAttributeCollection, IEnumerable<Field>>()
                .CreateUsing(x =>
                {
                    var output = new HashSet<Field>();
                    foreach (var attrib in x)
                    {
                        output.Add(Map<TypedAttribute, Field>(attrib));
                    }
                    return output;
                });

            #endregion

            #region TypedAttribute > Field

            this.CreateMap<TypedAttribute, Field>()
                .CreateUsing(x => new Field(Map<AttributeDefinition, FieldDefinition>(x.AttributeDefinition)))
                .ForMember(x => x.Values, opt => opt.MapFrom(x => x.Values));

            #endregion



            #region TypedAttributeValueCollection > IEnumerable<KeyedFieldValue>)

            this.CreateMap<KeyValuePair<string, object>, KeyedFieldValue>()
                .CreateUsing(x => new KeyedFieldValue(x.Key, x.Value));

            this.CreateMap<TypedAttributeValueCollection, IEnumerable<KeyedFieldValue>>()
                .CreateUsing(x => new HashSet<KeyedFieldValue>());

            #endregion

            #region AttributeDefinition > FieldDefinition

            this.CreateMap<AttributeDefinition, FieldDefinition>();

            #endregion

            #region EntitySchema > ContentType

            this.CreateMap<EntitySchema, ContentType>()
                .CreateUsing(x => new ContentType(x.Alias, x.Name, Map<IEnumerable<AttributeDefinition>, IEnumerable<FieldDefinition>>(x.AttributeDefinitions)));

            #endregion
        }
    }

}