// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'NodeRelationType' entity, represented by the 'NodeRelationType' class.</summary>
	public partial class NodeRelationTypeMap : ClassMap<NodeRelationType>
    {
		/// <summary>Initializes a new instance of the <see cref="NodeRelationTypeMap"/> class.</summary>
		public NodeRelationTypeMap()
        {
			Table("NodeRelationType");
			OptimisticLock.None();

		    Id(x => x.Id)
		        .Access.CamelCaseField(Prefix.Underscore)
		        .Column("Id")
		        .GeneratedBy.Custom<GuidCombUriGenerator>();

		    Map(x => x.Alias)
		        .Not.Nullable()
		        .Unique()
		        .Length(64)
		        .Index(this.GenerateIndexName(x => x.Alias))
		        .Access.CamelCaseField(Prefix.Underscore);

		    //NaturalId().Property(x => x.Alias);

		    //Map(x => x.Alias).Access.CamelCaseField(Prefix.Underscore);
			Map(x=>x.Description).CustomType("StringClob").Access.CamelCaseField(Prefix.Underscore);

		    Map(x => x.Name).Access.CamelCaseField(Prefix.Underscore);

		    HasMany(x => x.NodeRelations)
		        .Access.CamelCaseField(Prefix.Underscore)
		        //.Cascade.MergeSaveAllDeleteOrphan()
                .Cascade.All()
		        .Fetch.Select()
		        .AsSet()
		        .Inverse()
		        .LazyLoad() // Reverse navigator, so lazy-load
		        //.KeyColumns.Add("NodeRelationTypeId")
                .Key(x =>
                {
                    x.Column("NodeRelationTypeId");
                    x.ForeignKey(this.GenerateFkName(y => y.NodeRelations));
                })
		        .Cache.IncludeAll().ReadWrite();

			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
