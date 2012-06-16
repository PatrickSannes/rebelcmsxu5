// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'Locale' entity, represented by the 'Locale' class.</summary>
	public partial class LocaleMap : ClassMap<Locale>
    {
		/// <summary>Initializes a new instance of the <see cref="LocaleMap"/> class.</summary>
		public LocaleMap()
        {
			Table("Locale");
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

		    Map(x => x.LanguageIso).Column("LanguageISO").Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
		        .Index(this.GenerateIndexName(x => x.LanguageIso));

			Map(x=>x.Name).Access.CamelCaseField(Prefix.Underscore);

            //HasMany(x => x.AttributeDateValues)
            //    .Access.CamelCaseField(Prefix.Underscore)
            //    .Cascade.MergeSaveAllDeleteOrphan()
            //    .Fetch.Select()
            //    .AsSet()
            //    .Inverse()
            //    .LazyLoad()
            //    //.KeyColumns.Add("LocaleId");
            //    .Key(x =>
            //    {
            //        x.Column("LocaleId");
            //        x.ForeignKey(this.GenerateFkName(y => y.AttributeDateValues));
            //    });

            //HasMany(x => x.AttributeIntegerValues)
            //    .Access.CamelCaseField(Prefix.Underscore)
            //    .Cascade.MergeSaveAllDeleteOrphan()
            //    .Fetch.Select()
            //    .AsSet()
            //    .Inverse()
            //    .LazyLoad()
            //    //.KeyColumns.Add("LocaleId");
            //    .Key(x =>
            //    {
            //        x.Column("LocaleId");
            //        x.ForeignKey(this.GenerateFkName(y => y.AttributeIntegerValues));
            //    });

            //HasMany(x => x.AttributeLongStringValues)
            //    .Access.CamelCaseField(Prefix.Underscore)
            //    .Cascade.MergeSaveAllDeleteOrphan()
            //    .Fetch.Select()
            //    .AsSet()
            //    .Inverse()
            //    .LazyLoad()
            //    //.KeyColumns.Add("LocaleId");
            //    .Key(x =>
            //    {
            //        x.Column("LocaleId");
            //        x.ForeignKey(this.GenerateFkName(y => y.AttributeLongStringValues));
            //    });

            //HasMany(x => x.AttributeStringValues)
            //    .Access.CamelCaseField(Prefix.Underscore)
            //    .Cascade.MergeSaveAllDeleteOrphan()
            //    .Fetch.Select()
            //    .AsSet()
            //    .Inverse()
            //    .LazyLoad()
            //    //.KeyColumns.Add("LocaleId");
            //    .Key(x =>
            //    {
            //        x.Column("LocaleId");
            //        x.ForeignKey(this.GenerateFkName(y => y.AttributeStringValues));
            //    });

			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
