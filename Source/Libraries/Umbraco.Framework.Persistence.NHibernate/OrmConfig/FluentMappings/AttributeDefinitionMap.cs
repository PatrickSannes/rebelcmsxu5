// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'AttributeDefinition' entity, represented by the 'AttributeDefinition' class.</summary>
	public partial class AttributeDefinitionMap : ClassMap<AttributeDefinition>
    {
		/// <summary>Initializes a new instance of the <see cref="AttributeDefinitionMap"/> class.</summary>
		public AttributeDefinitionMap()
        {
			Table("AttributeDefinition");
			OptimisticLock.None();

		    Id(x => x.Id)
		        .Access.CamelCaseField(Prefix.Underscore)
		        .Column("Id")
		        .GeneratedBy.Custom<GuidCombUriGenerator>();

            Map(x => x.Alias)
               .Access.CamelCaseField(Prefix.Underscore)
               .Not.Nullable()
               .Length(64)
               .Index(this.GenerateIndexName(x => x.Alias));

			Map(x=>x.Description).CustomType("StringClob").Access.CamelCaseField(Prefix.Underscore);

		    Map(x => x.Name).Access.CamelCaseField(Prefix.Underscore)
		        .Length(128);

			Map(x=>x.XmlConfiguration).CustomType("StringClob").Access.CamelCaseField(Prefix.Underscore);

		    Map(x => x.Ordinal).Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
                .Default("0")
		        .Index(this.GenerateIndexName(x => x.Ordinal));

		    References(x => x.AttributeDefinitionGroup)
		        .Access.CamelCaseField(Prefix.Underscore)
		        .Not.Nullable()
                .Cascade.Merge() // Used to be All, but deleting AttributeDefinition cascades to delete this too
		        .Fetch.Join()
		        .LazyLoad(Laziness.False)
		        .Column("AttributeDefinitionGroupId")
		        .Index(this.GenerateIndexName(x => x.AttributeDefinitionGroup));

            HasMany(x => x.Attributes)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.MergeSaveAllDeleteOrphan()
                .Fetch.Select()
                .AsSet()
                .Inverse()
                .LazyLoad() // Reverse navigator so lazy-load
                //.KeyColumns.Add("AttributeDefinitionId");
                .Key(x =>
                {
                    x.Column("AttributeDefinitionId");
                    x.ForeignKey(this.GenerateFkName(y => y.Attributes));
                });

		    References(x => x.AttributeSchemaDefinition)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge() // Used to be All, but deleting AttributeDefinition cascades to delete this too
		        .Fetch.Select()
		        .Not.Nullable()
		        .Column("AttributeSchemaDefinitionId")
		        .Index(this.GenerateIndexName(x => x.AttributeSchemaDefinition));

		    References(x => x.AttributeType)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge() // Used to be All, but deleting AttributeDefinition cascades to delete this too
		        .Fetch.Join()
		        .LazyLoad(Laziness.False)
		        .Not.Nullable()
		        .Column("AttributeTypeId")
		        .Index(this.GenerateIndexName(x => x.AttributeType));

			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
