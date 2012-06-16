using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    using Umbraco.Framework.Persistence.Model.Attribution.MetaData;

    /// <summary>
    /// This will look up all of the defined properties on the TypedEntity's EntitySchema and ensure they 
    /// exist in the TypedEntity's TypedAttributeCollection, then we'll map the TypedAttributeCollection to the ContentProperty collection.
    /// </summary>
    /// <remarks>
    /// The re-mapping of new properties is required because if new attribution schema definition properties are created, then 
    /// need to be reflected in the content since when we add/remove a doc type property we don't go and create a new revision for 
    /// every content peice that is associated with the document type.
    /// </remarks>
    internal class TypedEntityToContentProperties<TEntity> : StandardMemberMapper<TEntity, HashSet<ContentProperty>>
        where TEntity : TypedEntity
    {
        public TypedEntityToContentProperties(AbstractFluentMappingEngine currentEngine, MapResolverContext context)
            : base(currentEngine, context)
        {
        }

        public override HashSet<ContentProperty> GetValue(TEntity source)
        {
            //check all schema definitions for the TPE and add any that don't exist in the TPE's attributes.
            source.SetupFromSchema();

            //now we need to remove any properties that don't exist anymore, excluding the 'special' internal fields
            IEnumerable<string> allAliases = source.EntitySchema.AttributeDefinitions.Select(a => a.Alias)
                //.Concat(FixedAttributeDefinitionAliases.AllAliases)
                .ToArray();

            // check the aliases of inherited properties as we don't want to remove those
            var compositeSchema = source.EntitySchema as CompositeEntitySchema;
            if (compositeSchema != null)
            {
                allAliases = allAliases.Concat(compositeSchema.InheritedAttributeDefinitions.Select(x => x.Alias)).ToArray();
            }

            var toRemove = source.Attributes.Where(x => !allAliases.Contains(x.AttributeDefinition.Alias))
                .Select(x => x.Id)
                .ToArray();
            source.Attributes.RemoveAll(x => toRemove.Contains(x.Id));

            //we don't want to map the 'special' properties
            return CurrentEngine.Map<IEnumerable<TypedAttribute>, HashSet<ContentProperty>>(source.Attributes);
        }
    }
}