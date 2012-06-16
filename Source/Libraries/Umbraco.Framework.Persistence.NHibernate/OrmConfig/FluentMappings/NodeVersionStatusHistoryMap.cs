// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'NodeVersionStatusHistory' entity, represented by the 'NodeVersionStatusHistory' class.</summary>
	public partial class NodeVersionStatusHistoryMap : ClassMap<NodeVersionStatusHistory>
    {
		/// <summary>Initializes a new instance of the <see cref="NodeVersionStatusHistoryMap"/> class.</summary>
		public NodeVersionStatusHistoryMap()
        {
			Table("NodeVersionStatusHistory");
			OptimisticLock.None();

			Id(x=>x.Id)
				.Access.CamelCaseField(Prefix.Underscore)
				.Column("Id")
				.GeneratedBy.Custom<GuidCombUriGenerator>();

		    Map(x => x.Date).Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
		        .Index(this.GenerateIndexName(x => x.Date));

		    References(x => x.NodeVersion)
		        .Access.CamelCaseField(Prefix.Underscore)
		        .Cascade.Merge()
		        .Not.Nullable()
		        .Fetch.Select()
		        .LazyLoad() // Reverse navigator so lazy-load
		        .Column("NodeVersionId")
		        .Index(this.GenerateIndexName(x => x.NodeVersionStatusType));

		    References(x => x.NodeVersionStatusType)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
		        .Not.Nullable()
		        .Fetch.Select()
		        .Column("NodeVersionStatusTypeId")
		        .Index(this.GenerateIndexName(x => x.NodeVersionStatusType));

			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
