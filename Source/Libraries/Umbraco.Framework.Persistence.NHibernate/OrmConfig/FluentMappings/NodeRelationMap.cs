// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'NodeRelation' entity, represented by the 'NodeRelation' class.</summary>
	public partial class NodeRelationMap : ClassMap<NodeRelation>
    {
		/// <summary>Initializes a new instance of the <see cref="NodeRelationMap"/> class.</summary>
		public NodeRelationMap()
        {
			Table("NodeRelation");
			OptimisticLock.None();

			Id(x=>x.Id)
				.Access.CamelCaseField(Prefix.Underscore)
				.Column("Id")
				.GeneratedBy.Custom<GuidCombUriGenerator>();

		    Map(x => x.DateCreated).Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
		        .Index(this.GenerateIndexName(x => x.DateCreated));

		    Map(x => x.Ordinal).Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
                .Default("0")
		        .Index(this.GenerateIndexName(x => x.Ordinal));

		    References(x => x.EndNode)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
		        .Fetch.Join()
		        .Not.Nullable()
		        //.LazyLoad(Laziness.NoProxy)
		        .Column("EndNodeId")
		        .Index(this.GenerateIndexName(x => x.EndNode));

            HasMany(x => x.NodeRelationCaches)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.MergeSaveAllDeleteOrphan()
                .Fetch.Select()
                .AsSet()
                .Inverse()
                .LazyLoad()
                //.KeyColumns.Add("NodeRelationId");
                .Key(x =>
                {
                    x.Column("NodeRelationId");
                    x.ForeignKey(this.GenerateFkName(y => y.NodeRelationCaches));
                });

		    HasMany(x => x.NodeRelationTags)
		        .Access.CamelCaseField(Prefix.Underscore)
		        .Cascade.MergeSaveAllDeleteOrphan()
		        .Fetch.Join()
		        .AsSet()
		        .Inverse()
                .BatchSize(20)
		        //.LazyLoad()
		        //.KeyColumns.Add("NodeRelationId")
                .Key(x =>
                {
                    x.Column("NodeRelationId");
                    x.ForeignKey(this.GenerateFkName(y => y.NodeRelationTags));
                })
		        .Cache.IncludeAll().ReadWrite();

		    References(x => x.NodeRelationType)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
		        .Not.Nullable()
		        .Fetch.Join()
		        .Column("NodeRelationTypeId")
		        .Index(this.GenerateIndexName(x => x.NodeRelationType));

		    References(x => x.StartNode)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
		        .Not.Nullable()
                .Fetch.Join()
                //.LazyLoad(Laziness.NoProxy)
		        .Column("StartNodeId")
		        .Index(this.GenerateIndexName(x => x.StartNode));

            //Map(x => x.StartNodeId).Access.CamelCaseField(Prefix.Underscore);
            //Map(x => x.EndNodeId).Access.CamelCaseField(Prefix.Underscore);

			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
