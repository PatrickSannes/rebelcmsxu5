// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'NodeRelationTag' entity, represented by the 'NodeRelationTag' class.</summary>
	public partial class NodeRelationTagMap : ClassMap<NodeRelationTag>
    {
		/// <summary>Initializes a new instance of the <see cref="NodeRelationTagMap"/> class.</summary>
		public NodeRelationTagMap()
        {
			Table("NodeRelationTag");
			OptimisticLock.None();

			Id(x=>x.Id)
				.Access.CamelCaseField(Prefix.Underscore)
				.Column("Id")
				.GeneratedBy.Custom<GuidCombUriGenerator>();

		    Map(x => x.Name).Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
                .Length(64)
		        .Index(this.GenerateIndexName(x => x.Name));

		    Map(x => x.Value).Access.CamelCaseField(Prefix.Underscore)
		        .Index(this.GenerateIndexName(x => x.Value));

		    References(x => x.NodeRelation)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
		        .Not.Nullable()
		        .Fetch.Select()
                .LazyLoad()
		        .Column("NodeRelationId")
		        .Index(this.GenerateIndexName(x => x.NodeRelation));

			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
