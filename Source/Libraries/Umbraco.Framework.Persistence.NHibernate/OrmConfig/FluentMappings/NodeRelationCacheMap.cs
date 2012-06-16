// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'NodeRelationCache' entity, represented by the 'NodeRelationCache' class.</summary>
	public partial class NodeRelationCacheMap : ClassMap<NodeRelationCache>
    {
		/// <summary>Initializes a new instance of the <see cref="NodeRelationCacheMap"/> class.</summary>
		public NodeRelationCacheMap()
        {
			Table("NodeRelationCache");
			OptimisticLock.None();

			Id(x=>x.Id)
				.Access.CamelCaseField(Prefix.Underscore)
				.Column("Id")
				.GeneratedBy.Custom<GuidCombUriGenerator>();

			Map(x=>x.DistanceFromOriginal).Access.CamelCaseField(Prefix.Underscore);

		    References(x => x.EndNode)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
		        .Fetch.Select()
		        .Not.Nullable()
		        .Column("EndNodeId")
		        .Index(this.GenerateIndexName(x => x.EndNode));

		    References(x => x.NodeRelation)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
		        .Fetch.Select()
		        .Not.Nullable()
		        .Column("NodeRelationId")
		        .Index(this.GenerateIndexName(x => x.NodeRelation));

		    References(x => x.StartNode)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
		        .Fetch.Select()
		        .Not.Nullable()
		        .Column("StartNodeId")
		        .Index(this.GenerateIndexName(x => x.StartNode));

			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
