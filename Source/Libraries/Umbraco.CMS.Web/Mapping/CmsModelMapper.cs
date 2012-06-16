using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Configuration.ApplicationSettings;
using Umbraco.Cms.Web.Configuration.Dashboards;
using Umbraco.Cms.Web.Configuration.Languages;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.TinyMCE.InsertMacro;
using Umbraco.Cms.Web.Mvc.Metadata;
using Umbraco.Cms.Web.Security.Permissions;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Security;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using ParameterEditorModel = Umbraco.Cms.Web.Model.BackOffice.ParameterEditors.EditorModel;
using PropertyEditorModel = Umbraco.Cms.Web.Model.BackOffice.PropertyEditors.EditorModel;
using Umbraco.Cms.Web.Model;

namespace Umbraco.Cms.Web.Mapping
{
    /// <summary>
    /// Model mapper for web models for use in the CMS
    /// </summary>
    public sealed class CmsModelMapper : AbstractFluentMappingEngine
    {
        private readonly MapResolverContext _resolverContext;

        public CmsModelMapper(MapResolverContext resolverContext)
            : base(resolverContext.FrameworkContext)
        {
            _resolverContext = resolverContext;
        }

        public override void ConfigureMappings()
        {
            ConfigureInternalCmsModelMappings();
            ConfigurePersistenceToModelMappings();
            ConfigureModelToPersistenceMappings();

        }



        /// <summary>
        /// Creates mappings for Cms to/from Cms models
        /// </summary>
        public void ConfigureInternalCmsModelMappings()
        {
            this.SelfMap<HiveId>().CreateUsing(x => new HiveId(x.ProviderGroupRoot, x.ProviderId, x.Value));

            this.SelfMap<Tab>();

            this.SelfMap<DataType>()
                .CreateUsing(x => new DataType(x.Id, x.Name, x.Alias, x.PropertyEditor, x.Prevalues));

            this.SelfMap<DocumentTypeProperty>()
                .CreateUsing(x => new DocumentTypeProperty(x.DataType));

            this.CreateMap<HashSet<Tab>, HashSet<Tab>>().CreateUsing(x => new HashSet<Tab>());

            #region DocumentTypeEditorModel -> new UserGroupEditorModel

            //Creates a *NEW* UserGroupEditorModel from a DocumentTypeEditorModel

            this.CreateMap(new DocumentTypeToNewContentModel<UserGroupEditorModel>(this))
                .CreateUsing(x => new UserGroupEditorModel());

            #endregion

            #region DocumentTypeEditorModel -> new UserEditorModel

            //Creates a *NEW* UserEditorModel from a DocumentTypeEditorModel

            this.CreateMap(new DocumentTypeToNewContentModel<UserEditorModel>(this))
                .CreateUsing(x => new UserEditorModel(_resolverContext.Hive.GetReader(new Uri("security://users"))));

            #endregion

            //#region DocumentTypeEditorModel -> new MemberEditorModel

            ////Creates a *NEW* MemberEditorModel from a DocumentTypeEditorModel

            //this.CreateMap(new DocumentTypeToNewContentModel<MemberEditorModel>(this))
            //    .CreateUsing(x => new MemberEditorModel(_resolverContext.Hive.GetReader(new Uri("security://members"))));

            //#endregion

            #region DocumentTypeEditorModel -> new MediaEditorModel

            //Creates a *NEW* ContentEditorModel from a MediaEditorModel))

            this.CreateMap(new DocumentTypeToNewContentModel<MediaEditorModel>(this))
                .CreateUsing(x => new MediaEditorModel());

            #endregion

            #region DocumentTypeEditorModel -> new ContentEditorModel

            //Creates a *NEW* ContentEditorModel from a DocumentTypeEditorModel))

            this.CreateMap(new DocumentTypeToNewContentModel<ContentEditorModel>(this))
                .CreateUsing(x => new ContentEditorModel());

            #endregion

            #region DocumentTypeEditorModel -> new DictionaryItemEditorModel

            //Creates a *NEW* ContentEditorModel from a DocumentTypeEditorModel))

            this.CreateMap(new DocumentTypeToNewContentModel<DictionaryItemEditorModel>(this))
                .CreateUsing(x => new DictionaryItemEditorModel());

            #endregion

            #region HashSet<DocumentTypeProperty> -> HashSet<ContentProperty>

            this.CreateMap<HashSet<DocumentTypeProperty>, HashSet<ContentProperty>>()
                    .CreateUsing(
                        x => new HashSet<ContentProperty>(Map<IEnumerable<DocumentTypeProperty>, IEnumerable<ContentProperty>>(x)));

            #endregion

            #region DocumentTypeProperty -> new ContentProperty

            //maps a Document Type property to a *NEW* content property with a new generated Id

            this.CreateMap<DocumentTypeProperty, ContentProperty>()
                .CreateUsing(x => new ContentProperty(new HiveId(Guid.NewGuid()), x, new Dictionary<string, object>()))
                .IgnoreMember(x => x.Id)
                .IgnoreMember(x => x.UtcCreated)
                .IgnoreMember(x => x.UtcModified)
                .AfterMap((source, dest) =>
                {
                    //need to set the property editor context for this property if it needs it
                    if (dest.DocTypeProperty.DataType.InternalPropertyEditor != null
                        && dest.DocTypeProperty.DataType.InternalPropertyEditor is IContentAwarePropertyEditor)
                    {
                        var contentAwarePropEditor = (IContentAwarePropertyEditor)dest.DocTypeProperty.DataType.InternalPropertyEditor;
                        contentAwarePropEditor.SetContentProperty(dest);
                    }
                });

            #endregion

            #region DataType -> DataTypeEditorModel

            this.CreateMap<DataType, DataTypeEditorModel>()
                .CreateUsing(x => new DataTypeEditorModel())
                .ForMember(x => x.PreValueEditorModel, opt => opt.MapFrom(x => x.GetPreValueModel()))
                .ForMember(x => x.PropertyEditorId, opt => opt.MapFrom(x => x == null ? null : (Guid?)x.PropertyEditor.Id));

            #endregion

            #region IApplication -> ApplicationTrayModel

            this.CreateMap<IApplication, ApplicationTrayModel>()
                .MapMemberFrom(x => x.IconType, x => x.Icon.StartsWith(".") ? IconType.Sprite : IconType.Image)
                .MapMemberFrom(x => x.Icon, x => x.Icon.StartsWith(".") ? x.Icon.TrimStart('.') : x.Icon)
                .MapMemberFrom(x => x.Name, x => x.Alias.Localize(fallback: x.Name));

            #endregion

            #region IEnumerable<IDashboardGroup> -> DashboardApplicationModel

            this.CreateMap<IEnumerable<IDashboardGroup>, DashboardApplicationModel>(true)
                    .CreateUsing(x => new DashboardApplicationModel())
                    .AfterMap((s, t) =>
                    {
                        var dashboards = s.SelectMany(x => x.Dashboards).ToArray();
                        var tabs = dashboards.Select(x => x.TabName)
                            .Distinct()
                            .OrderBy(x => x)
                            .Select((x, index) => new Tab
                            {
                                Id = new HiveId(x.EncodeAsGuid()),
                                Alias = x.ToUmbracoAlias(),
                                Name = x,
                                SortOrder = index
                            }).ToArray();

                        t.Tabs = tabs;
                        t.Dashboards = dashboards.Select(d => new DashboardItemModel
                        {
                            TabId = new HiveId(d.TabName.EncodeAsGuid()),
                            ViewName = d.Name,
                            DashboardType = d.DashboardType,
                            Matches = s.Where(x => x.Dashboards.Contains(d)).Single().Matches
                        });
                    });
            #endregion

            #region MacroEditorModel -> SetParametersModel

            this.CreateMap<MacroEditorModel, SetParametersModel>()
                .CreateUsing(x => new SetParametersModel())
                .IgnoreMember(x => x.ContentId)
                .MapMemberFrom(x => x.MacroAlias, y => y.Alias)
                .AfterMap((s, t) =>
                {
                    var macroParams = new List<MacroParameterModel>();
                    macroParams.AddRange(s.MacroParameters.Select(Map<MacroParameterDefinitionModel, MacroParameterModel>));
                    t.MacroParameters = macroParams;
                });

            #endregion

            #region MacroParameterDefinitionModel -> MacroParameterModel

            this.CreateMap<MacroParameterDefinitionModel, MacroParameterModel>()
                .CreateUsing(x => new MacroParameterModel())
                .AfterMap((source, dest) =>
                    {
                        var paramEditor = _resolverContext.ParameterEditorFactory.GetParameterEditor(source.ParameterEditorId);
                        dest.ParameterEditorModel = ((dynamic)paramEditor.Value).CreateEditorModel();
                    });

            #endregion

            #region InsertMacro_SetParametersModel -> InsertMacro_InsertMacroModel

            this.CreateMap<SetParametersModel, InsertMacroModel>()
                .CreateUsing(x => new InsertMacroModel())
                .IgnoreMember(x => x.MacroParameters)
                .AfterMap((s, t) =>
                {
                    t.MacroParameters = new Dictionary<string, object>();
                    foreach (var macroParameterModel in s.MacroParameters)
                    {
                        t.MacroParameters.Add(macroParameterModel.Alias, macroParameterModel.ParameterEditorModel.GetSerializedValue());
                    }
                });


            #endregion
        }

        /// <summary>
        /// Creates mappings for Persistence/Hive to/from Cms View Models
        /// </summary>
        public void ConfigurePersistenceToModelMappings()
        {
            #region File -> FileEditorModel

            this.CreateMap<File, FileEditorModel>()
                .CreateUsing(x => new FileEditorModel(x.Id, x.Name, x.UtcCreated, x.UtcModified, () => Encoding.UTF8.GetString(x.ContentBytes)));
            //NOTE: currently parent Id isn't being used for the file editor, but if we want to enable it, this is what we'd do:
            //.ForMember(x => x.UtcModified, opt => opt.MapFrom(x => x.Relations.Parent<File>(FixedRelationTypes.DefaultRelationType).Id));

            #endregion

            #region UserGroup -> UserGroupPermissionsModel

            this.CreateMap<UserGroup, UserGroupPermissionsModel>()
                .CreateUsing(x => new UserGroupPermissionsModel())
                .MapMemberFrom(x => x.UserGroupId, y => y.Id)
                .MapMemberFrom(x => x.UserGroupHtmlId, y => y.Id.GetHtmlId())
                .MapMemberFrom(x => x.UserGroupName, y => y.Name)
                .AfterMap((s, t) =>
                {

                });

            #endregion

            #region PermissionMetadata -> PermissionStatusModel

            this.CreateMap<PermissionMetadata, PermissionStatusModel>()
                .CreateUsing(x => new PermissionStatusModel())
                .MapMemberFrom(x => x.PermissionId, y => y.Id)
                .MapMemberFrom(x => x.PermissionName, y => y.Name);

            #endregion

            #region AttributeGroup -> Tab

            this.CreateMap<AttributeGroup, Tab>()
                .MapMemberFrom(x => x.SortOrder, x => x.Ordinal)
                .AfterMap((from, to) =>
                {
                    if (from is InheritedAttributeGroup)
                    {
                        to.SchemaId = ((InheritedAttributeGroup)from).Schema.Id;
                    }
                });

            #endregion

            #region AttributeType -> DataTypeEditorModel

            this.CreateMap<AttributeType, DataTypeEditorModel>()
                .CreateUsing(x => new DataTypeEditorModel())
                .MapMemberUsing(x => x.PropertyEditorId, new AttributeTypeToPropertyEditorId(this, _resolverContext))
                .AfterMap((source, dest) =>
                {
                    var dataType = Map<AttributeType, DataType>(source);
                    dest.PreValueEditorModel = dataType.GetPreValueModel();
                });

            #endregion

            #region AttributeType -> DataType

            this.CreateMap<AttributeType, DataType>()
                .CreateUsing(x =>
                    {
                        if ((x.Name == null || x.Name.Value.IsNullOrWhiteSpace()) && string.IsNullOrEmpty(x.Alias))
                        {
                            throw new ArgumentNullException("Cannot create a DataType when the incoming Name and Alias are both null or empty");
                        }
                        if (string.IsNullOrEmpty(x.Alias)) x.Alias = x.Name.Value.ToUmbracoAlias();
                        return new DataType(x.Id, x.Name, x.Alias, null, x.RenderTypeProviderConfig);
                    })
                .AfterMap((source, dest) =>
                    {
                        //need to set this manually since MapUsing doesn't work with internal members
                        dest.Prevalues = source.RenderTypeProviderConfig;

                        //need to set this manually since MapUsing doesn't work with internal members
                        Guid output;
                        var guid = Guid.TryParse(source.RenderTypeProvider, out output) ? output : Guid.Empty;
                        if (guid != Guid.Empty)
                        {
                            var editor = _resolverContext.PropertyEditorFactory.GetPropertyEditor(guid);
                            if (editor != null)
                            {
                                dest.InternalPropertyEditor = editor.Value;
                            }
                        }

                    });

            #endregion

            #region TypedAttribute -> ContentProperty

            this.CreateMap<TypedAttribute, ContentProperty>()
                .CreateUsing(x =>

                             new ContentProperty(x.Id,
                                                 Map<AttributeDefinition, DocumentTypeProperty>(x.AttributeDefinition),
                                                 x.Values))
                //need to make sure the Id is ignored since we set it in the ctor!
                .ForMember(x => x.Id, opt => opt.Ignore())
                .MapMemberFrom(x => x.TabId, x => x.AttributeDefinition.AttributeGroup.Id)
                .MapMemberFrom(x => x.TabAlias, x => x.AttributeDefinition.AttributeGroup.Alias)
                .MapMemberFrom(x => x.SortOrder, x => x.AttributeDefinition.Ordinal)
                .MapMemberFrom(x => x.Name, x => x.AttributeDefinition.Name.ToString())
                .MapMemberFrom(x => x.Alias, x => x.AttributeDefinition.Alias)
                .AfterMap((source, dest) =>
                    {
                        //set the property editor context if it needs it
                        if (dest.DocTypeProperty.DataType.InternalPropertyEditor != null
                            && dest.DocTypeProperty.DataType.InternalPropertyEditor is IContentAwarePropertyEditor)
                        {
                            ((IContentAwarePropertyEditor)dest.DocTypeProperty.DataType.InternalPropertyEditor).SetContentProperty(dest);
                        }
                    });

            #endregion

            #region AttributeDefinition -> DocumentTypeProperty

            this.CreateMap<AttributeDefinition, DocumentTypeProperty>()
                .CreateUsing(
                    x => new DocumentTypeProperty(Map<AttributeType, DataType>(x.AttributeType)))
                .MapMemberFrom(x => x.SortOrder, x => x.Ordinal)
                .MapMemberFrom(x => x.TabId, x => x.AttributeGroup.Id)
                .MapMemberFrom(x => x.TabAlias, x => x.AttributeGroup.Alias)
                .AfterMap((source, dest) =>
                {
                    if (source.AttributeType != null)
                    {
                        //we need to check if this prevalue model has been overridden!
                        if (!string.IsNullOrEmpty(source.RenderTypeProviderConfigOverride))
                        {
                            var tmpDataType1 = Map<AttributeType, DataType>(source.AttributeType);
                            tmpDataType1.Prevalues = source.RenderTypeProviderConfigOverride;

                            var tmpDataType2 = Map<AttributeType, DataType>(source.AttributeType);
                            tmpDataType2.Prevalues = dest.DataType.Prevalues;

                            var tmpPreValueModel1 = tmpDataType1.GetPreValueModel();
                            var tmpPreValueModel2 = tmpDataType2.GetPreValueModel();

                            //copy overridable prevalues over
                            foreach (var prop in tmpPreValueModel2.GetType().GetProperties().Where(p => p.GetCustomAttributes<AllowDocumentTypePropertyOverrideAttribute>(false).Any()))
                            {
                                var overriddenValue = prop.GetValue(tmpPreValueModel1, null);
                                prop.SetValue(tmpPreValueModel2, overriddenValue, null);
                            }

                            //re-set the pre-value string on the data type, so now we can get the pre-value model with the overridden values
                            dest.DataType.Prevalues = tmpPreValueModel2.GetSerializedValue();
                        }
                    }


                    var inherited = source as InheritedAttributeDefinition;
                    if (inherited != null)
                    {
                        dest.SchemaId = inherited.Schema.Id;
                        dest.SchemaName = inherited.Schema.Name;
                    }
                });

            #endregion

            #region EntitySchema -> DocumentTypeInfo

            this.CreateMap<EntitySchema, DocumentTypeInfo>()
                    .CreateUsing(x =>
                    {
                        var documentTypeInfo = new DocumentTypeInfo()
                        {
                            Thumbnail = x.GetXmlConfigProperty("thumb"),
                            Name = x.Name,
                            Icon = x.GetXmlConfigProperty("icon"),
                            Description = x.GetXmlConfigProperty("description")
                        };
                        return documentTypeInfo;
                    });

            #endregion

            #region EntitySchema -> DocumentTypeEditorModel

            this.CreateMap(new EntitySchemaToSchemaEditorModel<DocumentTypeEditorModel>(this,
                x => new DocumentTypeEditorModel(),
                (f, t) =>
                {
                    t.DefaultTemplateId = f.GetXmlConfigProperty<HiveId>("default-template");
                    //get the stored allowed tempaltes
                    var allowedItems = f.GetXmlPropertyAsList<HiveId>("allowed-templates") ?? new List<HiveId>();
                    t.AllowedTemplateIds = new HashSet<HiveId>(allowedItems);
                }));

            #endregion

            #region EntitySchema -> MediaTypeEditorModel

            this.CreateMap(new EntitySchemaToSchemaEditorModel<MediaTypeEditorModel>(this, x => new MediaTypeEditorModel()));

            #endregion

            #region TypedEntity -> ContentEditorModel

            this.CreateMap(new TypedEntityToContentEditorModel<TypedEntity, ContentEditorModel>(this, _resolverContext))
                .CreateUsing(x => new ContentEditorModel());

            #endregion

            #region TypedEntity -> MediaEditorModel

            this.CreateMap(new TypedEntityToContentEditorModel<TypedEntity, MediaEditorModel>(this, _resolverContext))
                .CreateUsing(x => new MediaEditorModel());

            #endregion

            #region TypedEntity -> DictionaryItemEditorModel

            this.CreateMap(new TypedEntityToContentEditorModel<TypedEntity, DictionaryItemEditorModel>(this, _resolverContext))
                .CreateUsing(x => new DictionaryItemEditorModel());

            #endregion

            #region User -> MemberEditorModel

            this.CreateMap(new MemberToMemberEditorModel<Member, MemberEditorModel>(this, _resolverContext, (source, dest) =>
                {
                    //now update user groups
                    //TODO: Get user group relations
                }))
                .CreateUsing(x => new MemberEditorModel(_resolverContext.Hive.GetReader(new Uri("security://users"))));                

            #endregion

            #region User -> UserEditorModel

            this.CreateMap(new MemberToMemberEditorModel<User, UserEditorModel>(this, _resolverContext, (source, dest) =>
                {
                    //now update user groups
                    //TODO: Get user group relations
                }))
                .CreateUsing(x => new UserEditorModel(_resolverContext.Hive.GetReader(new Uri("security://users"))))
                .ForMember(x => x.SessionTimeout, opt => opt.Ignore())
                .ForMember(x => x.StartContentHiveId, opt => opt.Ignore())
                .ForMember(x => x.StartMediaHiveId, opt => opt.Ignore())
                .ForMember(x => x.Applications, opt => opt.Ignore())
                .ForMember(x => x.UserGroups, opt => opt.Ignore());

            #endregion

            #region UserGroup -> UserGroupEditorModel

            this.CreateMap(new TypedEntityToContentEditorModel<UserGroup, UserGroupEditorModel>(this, _resolverContext))
                .CreateUsing(x => new UserGroupEditorModel())
                //ignore all custom properties as these need to be mapped by the underlying attributes
                .ForMember(x => x.Name, opt => opt.Ignore());

            #endregion

            #region EntitySnapshot<T> > ContentEditorModel

            this.CreateMap(new EntitySnapshotToContentEditorModel<ContentEditorModel>(this));

            #endregion

            #region EntitySnapshot<T> > MediaEditorModel

            this.CreateMap(new EntitySnapshotToContentEditorModel<MediaEditorModel>(this));

            #endregion

            #region EntitySnapshot<T> > DictionaryItemEditorModel

            this.CreateMap(new EntitySnapshotToContentEditorModel<DictionaryItemEditorModel>(this));

            #endregion

            #region Revision<TypedEntity> -> ContentEditorModel

            this.CreateMap(new RevisionToContentEditorModel<ContentEditorModel>(this));

            #endregion

            #region Revision<TypedEntity> -> MediaEditorModel

            this.CreateMap(new RevisionToContentEditorModel<MediaEditorModel>(this));

            #endregion

            #region Revision<TypedEntity> -> DictionaryItemEditorModel

            this.CreateMap(new RevisionToContentEditorModel<DictionaryItemEditorModel>(this));

            #endregion

            #region LanguageElement -> LanguageEditorModel

            this.CreateMap<LanguageElement, LanguageEditorModel>()
                .ForMember(x => x.Fallbacks, opts => opts.MapFrom(x => x.Fallbacks.Select(y => y.IsoCode)));

            #endregion

            #region StylesheetRule -> StylesheetRuleEditorModel

            this.CreateMap<StylesheetRule, StylesheetRuleEditorModel>()
                .CreateUsing(x => new StylesheetRuleEditorModel())
                .MapMemberFrom(x => x.Id, x => x.RuleId)
                .ForMember(x => x.ParentId, x => x.MapFrom(y => y.StylesheetId));

            #endregion
        }

        /// <summary>
        /// Creates mappings for Cms View Models to/from Persistence/Hive models
        /// </summary>
        public void ConfigureModelToPersistenceMappings()
        {
            #region  FileEditorModel -> File

            this.CreateMap<FileEditorModel, File>()
                .CreateUsing(x => new File())
                .ForMember(x => x.ContentBytes, opt => opt.MapFrom(x => Encoding.UTF8.GetBytes(x.FileContent)));


            #endregion

            #region UserGroupEditorModel -> UserGroup

            this.CreateMap((new ContentEditorModelToTypedEntity<UserGroupEditorModel, UserGroup>(this, _resolverContext)))
                //ignore all custom properties as these need to be mapped by the underlying attributes
                .ForMember(x => x.Name, opt => opt.Ignore());

            #endregion

            #region MemberEditorModel -> Member

            this.CreateMap<MemberEditorModel, Member>()
                .CreateUsing(x => new Member())
                .ForMember(x => x.UtcCreated, opt => opt.MapUsing<UtcCreatedMapper>())
                .ForMember(x => x.UtcModified, opt => opt.MapUsing<UtcModifiedMapper>())
                .IgnoreMember(x => x.RelationProxies)
                .MapMemberFrom(x => x.EntitySchema, x => new MemberSchema())
                .IgnoreMember(x => x.LastPasswordChangeDate)
                .IgnoreMember(x => x.LastActivityDate)
                .IgnoreMember(x => x.LastLoginDate)
                .IgnoreMember(x => x.IsApproved)
                .IgnoreMember(x => x.Email)
                .IgnoreMember(x => x.Username)
                .IgnoreMember(x => x.Name)
                .AfterMap((source, dest) =>
                {                   
                    //add or update all properties in the model.
                    //We need to manually create a mapper for this operation in order to keep the object graph consistent
                    //with the correct instances of objects. This mapper will ignore mapping the AttributeDef on the TypedAttribute
                    //so that we can manually assign it from our EntitySchema.
                    var propertyMapper = new ContentPropertyToTypedAttribute(this, true);
                    foreach (var contentProperty in source.Properties.Where(x => x.Alias != MemberSchema.PasswordAlias))
                    {
                        var typedAttribute = propertyMapper.Map(contentProperty);
                        //now we need to manually assign the same attribute definition instance from our already mapped schema.
                        var attDef = dest.EntitySchema.AttributeDefinitions.Single(x => x.Id.Value == contentProperty.DocTypePropertyId.Value);
                        typedAttribute.AttributeDefinition = attDef;

                        dest.Attributes.SetValueOrAdd(typedAttribute);
                    }
                    //now we need to remove any properties that don't exist in the model, excluding the 'special' internal fields
                    var allAliases = source.Properties.Select(x => x.Alias).ToArray();
                    var toRemove = dest.Attributes.Where(x => !allAliases.Contains(x.AttributeDefinition.Alias))
                        .Select(x => x.Id).ToArray();
                    dest.Attributes.RemoveAll(x => toRemove.Contains(x.Id));


                });
                


            #endregion

            #region UserEditorModel -> User

            this.CreateMap(new UserEditorModelToTypedEntity(this, _resolverContext, MemberSchema.PasswordAlias));

            #endregion

            #region ContentEditorModel -> Revision<TypedEntity>

            this.CreateMap(new ContentEditorModelToRevision<ContentEditorModel>(this));

            #endregion

            #region MediaEditorModel -> Revision<TypedEntity>

            this.CreateMap(new ContentEditorModelToRevision<MediaEditorModel>(this));

            #endregion

            #region DictionaryItemEditorModel -> Revision<TypedEntity>

            this.CreateMap(new ContentEditorModelToRevision<DictionaryItemEditorModel>(this));

            #endregion

            #region ContentEditorModel -> TypedEntity

            this.CreateMap(new ContentEditorModelToTypedEntity<ContentEditorModel, TypedEntity>(this, _resolverContext));

            #endregion

            #region MediaEditorModel -> TypedEntity

            this.CreateMap(new ContentEditorModelToTypedEntity<MediaEditorModel, TypedEntity>(this, _resolverContext));

            #endregion

            #region DictionaryItemEditorModel -> TypedEntity

            this.CreateMap(new ContentEditorModelToTypedEntity<DictionaryItemEditorModel, TypedEntity>(this, _resolverContext));

            #endregion

            #region DocumentTypeEditorModel -> EntitySchema

            this.CreateMap(new SchemaEditorModelToEntitySchema<DocumentTypeEditorModel>(this, _resolverContext, (from, to) =>
                {
                    if (!from.DefaultTemplateId.IsNullValueOrEmpty())
                        to.SetXmlConfigProperty("default-template", from.DefaultTemplateId.Value);

                    //save the allowed templates as a list of ids
                    to.SetXmlConfigProperty("allowed-templates",
                                            from.AllowedTemplates
                                                .Where(x => x.Selected)
                                                .Select(x => x.Value).ToArray());
                }));

            #endregion

            #region MediaTypeEditorModel -> EntitySchema

            this.CreateMap(new SchemaEditorModelToEntitySchema<MediaTypeEditorModel>(this, _resolverContext));

            #endregion

            #region ContentProperty -> TypedAttribute

            this.CreateMap(new ContentPropertyToTypedAttribute(this));

            #endregion

            #region DataTypeEditorModel -> AttributeType

            this.CreateMap(new DataTypeToAttributeType<DataTypeEditorModel>(
                               this,
                               ((source, dest) =>
                                   {
                                       dest.RenderTypeProvider = source.PropertyEditorId.ToString();
                                       if (source.PreValueEditorModel != null)
                                       {
                                           dest.RenderTypeProviderConfig = source.PreValueEditorModel.GetSerializedValue();
                                       }
                                   })));
            #endregion

            #region DataType -> AttributeType

            this.CreateMap(new DataTypeToAttributeType<DataType>(
                               this,
                               ((source, dest) =>
                                   {
                                       if (source.PropertyEditor != null)
                                       {
                                           dest.RenderTypeProvider = ((PropertyEditor)source.PropertyEditor).Id.ToString();
                                       }
                                       dest.RenderTypeProviderConfig = source.Prevalues;
                                   })));
            #endregion

            #region DocumentTypeProperty -> AttributeDefinition

            this.CreateMap(new DocumentTypePropertyToAttributeDefinition(this, _resolverContext));

            #endregion

            #region Tab -> AttributeDefinitionGroup

            this.CreateMap<Tab, AttributeGroup>()
                .CreateUsing(x => new AttributeGroup() { Ordinal = x.SortOrder, Alias = x.Name });

            #endregion

            #region HostnameEntryModel -> Hostname))

            this.CreateMap<HostnameEntryModel, Hostname>()
                .CreateUsing(x => new Hostname())
                .MapMemberFrom(x => x.Name, x => x.Hostname);

            #endregion

            #region PermissionStatusModel -> Relation

            this.CreateMap<PermissionStatusModel, RelationMetaDatum>()
                .CreateUsing(x => new RelationMetaDatum(x.PermissionId.ToString(), x.Status.HasValue
                                                                        ? x.Status.Value.ToString()
                                                                        : PermissionStatus.Inherit.ToString()));

            #endregion

            #region HostnamesModel -> IEnumerable<Hostname>

            this.CreateMap<HostnamesModel, IEnumerable<Hostname>>()
                .CreateUsing(x => new List<Hostname>())
                .AfterMap((source, dest) =>
                {
                    //we are casting here only cuz we know we created it with a list, 
                    //if we are trying to map to existing, this isn't gonna work.
                    var list = (List<Hostname>)dest;
                    foreach (var h in source.AssignedHostnames.Select(Map<Hostname>))
                    {
                        // Enlist the specified parent
                        // TODO: The ContentEditorModel needs to supply us with the ordinal of the existing relationship, if it exists
                        // otherwise we're always going to reset the Ordinal to 0
                        h.RelationProxies.EnlistParentById(source.Id, FixedRelationTypes.HostnameRelationType, 0);

                        list.Add(h);
                    }

                });

            #endregion

            #region LanguageEditorModel -> LanguageElement

            this.CreateMap<LanguageEditorModel, LanguageElement>()
                .ForMember(x => x.Name, opt => opt.MapFrom(x => CultureInfo.GetCultureInfo(x.IsoCode).EnglishName))
                .ForMember(x => x.Fallbacks, opt => opt.Ignore())
                .AfterMap((s, t) =>
                {
                    t.Fallbacks.Clear();

                    foreach (var fallback in s.Fallbacks)
                    {
                        t.Fallbacks.Add(new FallbackElement { IsoCode = fallback });
                    }
                });

            #endregion

            #region StylesheetRuleEditorModel -> StylesheetRule

            this.CreateMap<StylesheetRuleEditorModel, StylesheetRule>()
                .ForMember(x => x.RuleId, x => x.MapFrom(y => y.Id))
                .ForMember(x => x.StylesheetId, x => x.MapFrom(y => y.ParentId));

            #endregion
        }
    }
}
