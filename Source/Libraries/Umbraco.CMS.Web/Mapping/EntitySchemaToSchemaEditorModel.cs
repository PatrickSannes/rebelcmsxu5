using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;

using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    /// <summary>
    /// Maps properties from EntitySchema to SchemaEditorModel existing objects
    /// </summary>
    internal class EntitySchemaToSchemaEditorModel<TEditorModel> : TypeMapper<EntitySchema, TEditorModel>
        where TEditorModel : AbstractSchemaEditorModel
    {
        public EntitySchemaToSchemaEditorModel(
            AbstractFluentMappingEngine engine, 
            Func<EntitySchema, TEditorModel> createWith,
            Action<EntitySchema, TEditorModel> additionalAfterMap = null)
            : base(engine)
        {

            MappingContext
                .CreateUsing(createWith)
                .MapMemberFrom(x => x.Thumbnail, x => x.GetXmlConfigProperty("thumb"))
                .MapMemberFrom(x => x.AllowedChildIds, x => x.GetXmlPropertyAsList<HiveId>("allowed-children"))
                .MapMemberFrom(x => x.Icon, x => x.GetXmlConfigProperty("icon"))
                .MapMemberFrom(x => x.Thumbnail, x => x.GetXmlConfigProperty("thumb"))
                .MapMemberFrom(x => x.DefinedTabs, x => MappingContext.Engine.Map<IEnumerable<AttributeGroup>, HashSet<Tab>>(x.AttributeGroups.ToArray()))
                .MapMemberFrom(x => x.Description, x => x.GetXmlConfigProperty("description"))
                .MapMemberFrom(x => x.IsAbstract, x => x.GetXmlConfigProperty("is-abstract") != null && bool.Parse(x.GetXmlConfigProperty("is-abstract")))
                .AfterMap((from, to) =>
                    {
                        // Commented out the below as working on multi-inheritance and the blow assumes one parent only (MB - 2011-11-15)

                        //var firstParentFound =
                        //from.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType).
                        //    FirstOrDefault();

                        //if (firstParentFound != null && !firstParentFound.Item.SourceId.IsNullValueOrEmpty())
                        //    to.ParentId = firstParentFound.Item.SourceId;

                        //first, map the properties from the attribute definition (but not the special fields)
                        to.Properties.Clear();
                        from.AttributeDefinitions
                            //.Where(x => !FixedAttributeDefinitionAliases.AllAliases.Contains(x.Alias))
                            .ForEach(y =>
                                     to.Properties.Add(
                                         MappingContext.Engine.Map<AttributeDefinition, DocumentTypeProperty>(y)));

                        //set the available tabs select list
                        to.AvailableTabs = new List<SelectListItem>(
                            to.DefinedTabs.Where(x => !x.Id.IsNullValueOrEmpty()).OrderBy(x => x.SortOrder)
                                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() }))
                            .ToArray();

                        //set the inherit from list
                        to.InheritFromIds =
                            from.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType).Select(
                                x => x.Item.SourceId);

                        var merged = from as CompositeEntitySchema;
                        if (merged != null)
                        {
                            to.InheritedProperties.Clear();
                            merged.InheritedAttributeDefinitions
                                .ForEach(y => to.InheritedProperties.Add(MappingContext.Engine.Map<AttributeDefinition, DocumentTypeProperty>(y)));

                            to.InheritedTabs =
                                MappingContext.Engine.Map<IEnumerable<AttributeGroup>, HashSet<Tab>>(
                                    merged.InheritedAttributeGroups.ToArray());
                        }

                        if (additionalAfterMap != null)
                        {
                            additionalAfterMap(from, to);    
                        }
                    });

        }
    }
}
