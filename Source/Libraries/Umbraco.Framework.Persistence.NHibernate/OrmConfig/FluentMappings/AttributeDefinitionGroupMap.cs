using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
    /// <summary>Represents the mapping of the 'AttributeDefinitionGroup' entity, represented by the 'AttributeDefinitionGroup' class.</summary>
    public partial class AttributeDefinitionGroupMap : SubclassMap<AttributeDefinitionGroup>
    {
        /// <summary>Initializes a new instance of the <see cref="AttributeDefinitionGroupMap"/> class.</summary>
        public AttributeDefinitionGroupMap()
        {
            KeyColumn("NodeId");

            Map(x => x.Alias)
                .Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
                .Length(64)
                .Index(this.GenerateIndexName(x => x.Alias));

            Map(x => x.Description).CustomType("StringClob").Access.CamelCaseField(Prefix.Underscore);

            Map(x=>x.Name).Access.CamelCaseField(Prefix.Underscore);

            Map(x => x.Ordinal).Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
                .Default("0")
                .Index(this.GenerateIndexName(x => x.Ordinal));

            HasMany(x => x.AttributeDefinitions)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge() // APN 2011/12/15 Important: Changes AttributeDefinitionGroupMap to have Merge (rather than Merge + Delete Orphan) cascade style. NH has decided to start cascading deletions of AttributeDefinitions (which cascades further to stored Attributes) when a group is removed from a schema, even if those AttributeDefinitions are still referenced by the schema to another group (e.g. in the case of moving a def to another group, and deleting the previous group, in one operation). So to counter this (after much debugging and attempts at alternatives) I've disabled the cascading of deletions from a group to its attribute definitions, meaning in practise attribute definitions have to be manually deleted first / at the same time as a group if desired. It's never actually desired in the backoffice so it's fine for now.
                .Fetch.Select()
                .AsSet()
                .Inverse()
                .LazyLoad() // Reverse navigator so lazy-load
                //.KeyColumns.Add("AttributeDefinitionGroupId");
                .Key(x =>
                {
                    x.Column("AttributeDefinitionGroupId");
                    x.ForeignKey(this.GenerateFkName(y => y.AttributeDefinitions));
                });

            References(x => x.AttributeSchemaDefinition)
                .Not.Nullable()
                .Cascade.Merge()
                .LazyLoad()
                .Index(this.GenerateIndexName(x => x.AttributeSchemaDefinition))
                .Fetch.Select()
                .Column("AttributeSchemaDefinitionId");

            Key(x =>
                {
                    x.Column("NodeId");
                    x.ForeignKey(this.GenerateFkName<AttributeDefinitionGroup, Node>());
                });

            AdditionalMappingInfo();
        }

        /// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
        partial void AdditionalMappingInfo();
    }
}