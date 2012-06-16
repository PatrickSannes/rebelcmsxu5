// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'NodeVersionStatusType' entity, represented by the 'NodeVersionStatusType' class.</summary>
	public partial class NodeVersionStatusTypeMap : ClassMap<NodeVersionStatusType>
    {
		/// <summary>Initializes a new instance of the <see cref="NodeVersionStatusTypeMap"/> class.</summary>
		public NodeVersionStatusTypeMap()
        {
			Table("NodeVersionStatusType");
			OptimisticLock.None();

		    Id(x => x.Id)
		        .Access.CamelCaseField(Prefix.Underscore)
		        .Column("Id")
		        .GeneratedBy.Custom<GuidCombUriGenerator>();

		    //NaturalId().Property(x => x.Name);

		    Map(x => x.IsSystem).Access.CamelCaseField(Prefix.Underscore).Not.Nullable();

		    Map(x => x.Name).Access.CamelCaseField(Prefix.Underscore);

		    Map(x => x.Alias).Access.CamelCaseField(Prefix.Underscore)
                .Length(16)
                .Unique()
                .Not.Nullable()
		        .Index(this.GenerateIndexName(x => x.Alias));

            HasMany(x => x.NodeVersionSchedules)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.MergeSaveAllDeleteOrphan()
                .Fetch.Select()// This is a select, not a join, because it's common that this type (NodeVersionStatusType) is included in other queries and we don't want that to cascade to a left outer join including NodeVersionSchedules
                .AsSet()
                .Inverse()
                .LazyLoad() // Reverse navigator, so lazy-load
                //.KeyColumns.Add("NodeVersionStatusTypeId");
                .Key(x =>
                {
                    x.Column("NodeVersionStatusTypeId");
                    x.ForeignKey(this.GenerateFkName(y => y.NodeVersionSchedules));
                });


			HasMany(x=>x.NodeVersionStatuses)
				.Access.CamelCaseField(Prefix.Underscore)
				.Cascade.MergeSaveAllDeleteOrphan()
				.Fetch.Select() // This is a select, not a join, because it's common that this type (NodeVersionStatusType) is included in other queries and we don't want that to cascade to a left outer join including NodeVersionStatuses
				.AsSet()
				.Inverse()
				.LazyLoad() // Reverse navigator, so lazy-load
				//.KeyColumns.Add("NodeVersionStatusTypeId");
                .Key(x =>
                {
                    x.Column("NodeVersionStatusTypeId");
                    x.ForeignKey(this.GenerateFkName(y => y.NodeVersionStatuses));
                });

			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
