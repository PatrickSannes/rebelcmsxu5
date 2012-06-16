// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'NodeVersion' entity, represented by the 'NodeVersion' class.</summary>
	public partial class NodeVersionMap : ClassMap<NodeVersion>
    {
		/// <summary>Initializes a new instance of the <see cref="NodeVersionMap"/> class.</summary>
		public NodeVersionMap()
        {
			Table("NodeVersion");
			OptimisticLock.None();

			Id(x=>x.Id)
				.Access.CamelCaseField(Prefix.Underscore)
				.Column("Id")
                .GeneratedBy.Custom<GuidCombUriGenerator>();

		    Map(x => x.DateCreated).Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
		        .Index(this.GenerateIndexName(x => x.DateCreated));

			Map(x=>x.DefaultName).Access.CamelCaseField(Prefix.Underscore);

		    References(x => x.AttributeSchemaDefinition)
		        .Access.CamelCaseField(Prefix.Underscore)
		        .Not.Nullable()
		        .Cascade.Merge()
		        .Index(this.GenerateIndexName(x => x.AttributeSchemaDefinition));

            HasMany(x => x.Attributes)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.MergeSaveAllDeleteOrphan()
                .Fetch.Join() // We rarely need a NodeVersion without its Attributes
                .AsSet()
                .Inverse()
                //.LazyLoad() // Not relevant when using a join fetch
                //.LazyLoad().Fetch.Select()
                //.KeyColumns.Add("NodeVersionId");
                .Key(x =>
                {
                    x.Column("NodeVersionId");
                    x.ForeignKey(this.GenerateFkName(y => y.Attributes));
                });

		    References(x => x.Node)
		        .Access.CamelCaseField(Prefix.Underscore)
		        .Cascade.Merge()
		        .Not.Nullable()
		        .Fetch.Select()
		        .Column("NodeId")
		        .Index(this.GenerateIndexName(x => x.Node));

            HasMany(x => x.NodeVersionSchedules)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.MergeSaveAllDeleteOrphan()
                .Fetch.Select()
                .BatchSize(5)
                .AsSet()
                .Inverse()
                .LazyLoad()
                .Key(x =>
                {
                    x.Column("NodeVersionId");
                    x.ForeignKey(this.GenerateFkName(y => y.NodeVersionSchedules));
                });

            HasMany(x => x.NodeVersionStatuses)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.MergeSaveAllDeleteOrphan()
                .Fetch.Select()
                .BatchSize(20)
                .AsSet()
                .Inverse()
                //.LazyLoad()
                .Key(x =>
                {
                    x.Column("NodeVersionId");
                    x.ForeignKey(this.GenerateFkName(y => y.NodeVersionStatuses));
                });

			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
