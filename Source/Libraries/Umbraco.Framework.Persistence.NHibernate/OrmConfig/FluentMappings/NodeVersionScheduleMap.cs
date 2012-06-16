// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'NodeVersionSchedule' entity, represented by the 'NodeVersionSchedule' class.</summary>
	public partial class NodeVersionScheduleMap : ClassMap<NodeVersionSchedule>
    {
		/// <summary>Initializes a new instance of the <see cref="NodeVersionScheduleMap"/> class.</summary>
		public NodeVersionScheduleMap()
        {
			Table("NodeVersionSchedule");
			OptimisticLock.None();

			Id(x=>x.Id)
				.Access.CamelCaseField(Prefix.Underscore)
				.Column("Id")
				.GeneratedBy.Custom<GuidCombUriGenerator>();

		    Map(x => x.EndDate).Access.CamelCaseField(Prefix.Underscore)
		        .Index(this.GenerateIndexName(x => x.EndDate));

		    Map(x => x.StartDate).Access.CamelCaseField(Prefix.Underscore)
		        .Index(this.GenerateIndexName(x => x.StartDate));

		    References(x => x.NodeVersion)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
		        .Not.Nullable()
		        .Fetch.Select()
		        .LazyLoad()
		        .Column("NodeVersionId")
		        .Index(this.GenerateIndexName(x => x.NodeVersion));

		    References(x => x.NodeVersionStatusType)
		        .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
		        .Not.Nullable()
		        .Fetch.Join()
		        .Column("NodeVersionStatusTypeId")
		        .Index(this.GenerateIndexName(x => x.NodeVersionStatusType));


			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
