// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'Attribute' entity, represented by the 'Attribute' class.</summary>
	public partial class AttributeMap : ClassMap<Attribute>
    {
		/// <summary>Initializes a new instance of the <see cref="AttributeMap"/> class.</summary>
		public AttributeMap()
        {
			Table("Attribute");
			OptimisticLock.None();

		    Id(x => x.Id)
		        .Access.CamelCaseField(Prefix.Underscore)
		        .Column("Id")
		        .GeneratedBy.Custom<GuidCombUriGenerator>();

            References(x => x.AttributeDefinition)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
                .Fetch.Select()
                .Not.Nullable()
                .Column("AttributeDefinitionId")
                .Index(this.GenerateIndexName(x => x.AttributeDefinition));

            References(x => x.NodeVersion)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
                .Fetch.Select()
                .Not.Nullable()
                .LazyLoad()
                .Column("NodeVersionId")
                .Index(this.GenerateIndexName(x => x.NodeVersion));

            HasMany(x => x.AttributeDecimalValues)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.MergeSaveAllDeleteOrphan()
                .Fetch.Select().LazyLoad()
                .AsSet()
                .Inverse()
                .Key(x =>
                {
                    x.Column("AttributeId");
                    x.ForeignKey(this.GenerateFkName(y => y.AttributeDecimalValues));
                });

		    HasMany(x => x.AttributeDateValues)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.MergeSaveAllDeleteOrphan()
                .Fetch.Select().LazyLoad()
		        .AsSet()
		        .Inverse()
                .Key(x =>
                {
                    x.Column("AttributeId");
                    x.ForeignKey(this.GenerateFkName(y => y.AttributeDateValues));
                });

			HasMany(x=>x.AttributeIntegerValues)
				.Access.CamelCaseField(Prefix.Underscore)
                .Cascade.MergeSaveAllDeleteOrphan()
                .Fetch.Select().LazyLoad()
				.AsSet()
				.Inverse()
                .Key(x =>
                {
                    x.Column("AttributeId");
                    x.ForeignKey(this.GenerateFkName(y => y.AttributeIntegerValues));
                });

			HasMany(x=>x.AttributeLongStringValues)
				.Access.CamelCaseField(Prefix.Underscore)
                .Cascade.MergeSaveAllDeleteOrphan()
				.Fetch.Select().LazyLoad() // Lazy-load long strings
				.AsSet()
				.Inverse()
                .Key(x =>
                {
                    x.Column("AttributeId");
                    x.ForeignKey(this.GenerateFkName(y => y.AttributeLongStringValues));
                });

			HasMany(x=>x.AttributeStringValues)
				.Access.CamelCaseField(Prefix.Underscore)
				.Cascade.MergeSaveAllDeleteOrphan()
				.Fetch.Select().LazyLoad()
				.AsSet()
				.Inverse()
                .Key(x =>
                {
                    x.Column("AttributeId");
                    x.ForeignKey(this.GenerateFkName(y => y.AttributeStringValues));
                });

			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
