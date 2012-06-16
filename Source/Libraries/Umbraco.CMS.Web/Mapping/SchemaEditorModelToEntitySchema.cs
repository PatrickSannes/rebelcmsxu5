using System;
using System.Linq;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    using global::System.Collections.Generic;

    /// <summary>
    /// Maps properties from SchemaEditorModel to EntitySchema existing objects
    /// </summary>
    internal class SchemaEditorModelToEntitySchema<TEditorModel> : TypeMapper<TEditorModel, EntitySchema>
        where TEditorModel : AbstractSchemaEditorModel
    {
        private readonly MapResolverContext _resolverContext;

        public SchemaEditorModelToEntitySchema(AbstractFluentMappingEngine engine, MapResolverContext resolverContext, Action<TEditorModel, EntitySchema> additionAfterMap = null)
            : base(engine)
        {
            _resolverContext = resolverContext;
            MappingContext
                .ForMember(x => x.UtcCreated, opt => opt.MapUsing<UtcCreatedMapper>())
                .ForMember(x => x.UtcModified, opt => opt.MapUsing<UtcModifiedMapper>())
                .ForMember(x => x.SchemaType, opt => opt.MapFrom(x => FixedSchemaTypes.Content))
                .AfterMap((from, to) =>
                {
                    // Commented out the below as working on multi-inheritance and the blow assumes one parent only (MB - 2011-11-15)

                    //if (!from.ParentId.IsNullValueOrEmpty())
                    //{
                    //    // Enlist the specified parent
                    //    // TODO: The ContentEditorModel needs to supply us with the ordinal of the existing relationship, if it exists
                    //    // otherwise we're always going to reset the Ordinal to 0
                    //    to.RelationProxies.EnlistParentById(from.ParentId, FixedRelationTypes.DefaultRelationType, 0);
                    //}

                    // AttributeGroups on a schema are a union between manually added groups and those referenced in AttributeDefinitions
                    // so rather than clearing the AttributeGroups and the definitions and remapping, first let's create a list of groups
                    // separately
                    var mappedGroups = engine.Map<IEnumerable<Tab>, IEnumerable<AttributeGroup>>(from.DefinedTabs);
                    var existingOrNewGeneralGroup = to.AttributeGroups.Where(x => x.Alias == FixedGroupDefinitions.GeneralGroupAlias)
                        .SingleOrDefault() ?? FixedGroupDefinitions.GeneralGroup;

                    // Now let's go through the attribute definitions
                    to.AttributeDefinitions.Clear();
                    to.AttributeGroups.Clear();
                    var definitionMapper = new DocumentTypePropertyToAttributeDefinition(engine, _resolverContext, false);
                    foreach (var p in from.Properties)
                    {
                        var attrDef = definitionMapper.Map(p);
                        if (p.TabId.IsNullValueOrEmpty())
                        {
                            attrDef.AttributeGroup = existingOrNewGeneralGroup;
                        }
                        else
                        {
                            var found = mappedGroups.SingleOrDefault(x => x.Id == p.TabId);
                            attrDef.AttributeGroup = found ?? existingOrNewGeneralGroup;
                        }

                        to.TryAddAttributeDefinition(attrDef);
                    }

                    // Now if there are any groups left that aren't linked to attributes add those too
                    var unusedGroups = mappedGroups.Except(to.AttributeGroups).ToArray();
                    to.AttributeGroups.AddRange(unusedGroups);

                    ////we'll map the attribute groups from defined tabs
                    //to.AttributeDefinitions.Clear();
                    //to.AttributeGroups.Clear();
                    //from.DefinedTabs.ForEach(y =>
                    //                           to.AttributeGroups.Add(
                    //                               engine.Map<Tab, AttributeGroup>(y)));
                    
                    ////see if there is a general group, if not create one
                    //var generalGroup = to.AttributeGroups.Where(x => x.Alias == FixedGroupDefinitions.GeneralGroupAlias).SingleOrDefault();
                    //if(generalGroup == null)
                    //{
                    //    to.AttributeGroups.Add(FixedGroupDefinitions.GeneralGroup);
                    //}

                    ////Here we need to make sure the same AttributeGroup instances that were just created
                    //// are assigned to the AttributeDefinition, to do this we create a new mapper instance and tell it
                    //// not to map AttributeGroups and instead we'll assign the group manually.
                    //var attDefMapper = new DocumentTypePropertyToAttributeDefinition(engine, _resolverContext, false);
                    //foreach(var p in from.Properties)
                    //{
                    //    var attrDef = attDefMapper.Map(p);
                    //    if (p.TabId.IsNullValueOrEmpty())
                    //    {
                    //        attrDef.AttributeGroup = generalGroup;
                    //    }
                    //    else
                    //    {
                    //        var found = to.AttributeGroups.SingleOrDefault(x => x.Id == p.TabId);
                    //        attrDef.AttributeGroup = found ?? generalGroup;
                    //    }
                    //    to.TryAddAttributeDefinition(attrDef);
                    //}

                    //ensure the 'in built' properties exist
                    to.TryAddAttributeDefinition(new NodeNameAttributeDefinition(existingOrNewGeneralGroup));
                    to.TryAddAttributeDefinition(new SelectedTemplateAttributeDefinition(existingOrNewGeneralGroup));

                    to.SetXmlConfigProperty("thumb", from.Thumbnail);

                    //save the description,icon
                    to.SetXmlConfigProperty("description", from.Description);
                    to.SetXmlConfigProperty("icon", from.Icon);
                    
                    //save the allowed children as a list of guids (for now)
                    to.SetXmlConfigProperty("allowed-children",
                                              (from.AllowedChildren != null &&
                                               from.AllowedChildren.Any())
                                                  ? from.AllowedChildren
                                                        .Where(x => x.Selected)
                                                        .Select(x => x.Value ?? string.Empty).ToArray()
                                                  : new string[] { });

                    //save the is abstract/inheritable only setting
                    to.SetXmlConfigProperty("is-abstract", from.IsAbstract);

                    if (additionAfterMap != null)
                    {
                        additionAfterMap(from, to);
                    }

                });
        }
    }
}
