// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'AttributeLongStringValue' entity, represented by the 'AttributeLongStringValue' class.</summary>
	public partial class AttributeLongStringValueMap : ClassMap<AttributeLongStringValue>
    {
		/// <summary>Initializes a new instance of the <see cref="AttributeLongStringValueMap"/> class.</summary>
		public AttributeLongStringValueMap()
        {
			Table("AttributeLongStringValue");
			OptimisticLock.None();

			Id(x=>x.Id)
				.Access.CamelCaseField(Prefix.Underscore)
				.Column("Id")
				.GeneratedBy.Custom<GuidCombUriGenerator>();

			Map(x=>x.Value).CustomType("StringClob").Access.CamelCaseField(Prefix.Underscore);

		    Map(x => x.ValueKey).Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
		        .Index(this.GenerateIndexName(x => x.ValueKey));

		    References(x => x.Attribute)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
		        .Fetch.Select()
		        .Not.Nullable()
		        .Index(this.GenerateIndexName(x => x.Attribute))
		        .Column("AttributeId");

			References(x=>x.Locale)
				.Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge() // TODO: This should be SaveUpdate() but need to fix mapping code
				.Fetch.Select()
                .Not.Nullable()
                .Index(this.GenerateIndexName(x => x.Locale))
                .Column("LocaleId");

			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
