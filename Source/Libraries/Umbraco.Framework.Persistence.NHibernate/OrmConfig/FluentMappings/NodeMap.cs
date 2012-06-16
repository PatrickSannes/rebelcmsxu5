// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
    using System;

    using System.Collections.Generic;

    using FluentNHibernate.MappingModel.Collections;

    /// <summary>Represents the mapping of the 'Node' entity, represented by the 'Node' class.</summary>
    public partial class NodeMap : ClassMap<Node>
    {
        /// <summary>Initializes a new instance of the <see cref="NodeMap"/> class.</summary>
        public NodeMap()
        {
            Table("Node");
            OptimisticLock.None();

            Id(x => x.Id)
                .Access.CamelCaseField(Prefix.Underscore)
                .Column("Id")
                .GeneratedBy.Custom<GuidCombUriGenerator>();

            Map(x => x.DateCreated).Access.CamelCaseField(Prefix.Underscore).Not.Nullable();

            Map(x => x.IsDisabled).Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
                .Index(this.GenerateIndexName(x => x.IsDisabled));

            HasMany(x => x.IncomingRelationCaches)
                .Access.CamelCaseField(Prefix.Underscore)
                //.Cascade.AllDeleteOrphan().ForeignKeyCascadeOnDelete()
                .Cascade.MergeSaveAllDeleteOrphan()//.ForeignKeyCascadeOnDelete()
                //.Cascade.All()
                //.Cascade.Merge()
                .Fetch.Select()
                .AsSet()
                .Inverse()
                .BatchSize(20)
                .LazyLoad()
                .Key(x =>
                {
                    x.Column("EndNodeId");
                    x.ForeignKey(this.GenerateFkName(y => y.IncomingRelationCaches));
                })
                .Cache.IncludeAll().ReadWrite();

            HasMany(x => x.IncomingRelations)
                .Access.CamelCaseField(Prefix.Underscore)
                //.Cascade.AllDeleteOrphan().ForeignKeyCascadeOnDelete()
                .Cascade.MergeSaveAllDeleteOrphan()//.ForeignKeyCascadeOnDelete()
                //.Cascade.All()
                //.Cascade.Merge()
                .Fetch.Select()
                .AsSet()
                .Inverse()
                .BatchSize(20)
                .LazyLoad()
                .Key(x =>
                {
                    x.Column("EndNodeId");
                    x.ForeignKey(this.GenerateFkName(y => y.IncomingRelations));
                })
                ;//.Cache.IncludeAll().ReadWrite(); This was disabled 27/Sep by APN to resolve an issue where new published items are not routable

            HasMany(x => x.OutgoingRelationCaches)
                .Access.CamelCaseField(Prefix.Underscore)
                //.Cascade.AllDeleteOrphan().ForeignKeyCascadeOnDelete()
                .Cascade.MergeSaveAllDeleteOrphan()//.ForeignKeyCascadeOnDelete()
                //.Cascade.All()
                //.Cascade.Merge()
                .Fetch.Select()
                .AsSet()
                .Inverse()
                .BatchSize(20)
                .LazyLoad()
                .Key(x =>
                {
                    x.Column("StartNodeId");
                    x.ForeignKey(this.GenerateFkName(y => y.OutgoingRelationCaches));
                })
                .Cache.IncludeAll().ReadWrite();

            HasMany(x => x.OutgoingRelations)
                .Access.CamelCaseField(Prefix.Underscore)
                //.Cascade.AllDeleteOrphan().ForeignKeyCascadeOnDelete()
                .Cascade.MergeSaveAllDeleteOrphan()//.ForeignKeyCascadeOnDelete()
                //.Cascade.All()
                //.Cascade.Merge()
                .Fetch.Select()
                .AsSet()
                .Inverse()
                .BatchSize(20)
                .LazyLoad()
                .Key(x =>
                {
                    x.Column("StartNodeId");
                    x.ForeignKey(this.GenerateFkName(y => y.OutgoingRelations));
                })
                ;//.Cache.IncludeAll().ReadWrite(); This was disabled 27/Sep by APN to resolve an issue where new published items are not routable

            HasMany(x => x.NodeVersions)
                .Access.Property()
                .Cascade.MergeSaveAllDeleteOrphan()//.ForeignKeyCascadeOnDelete()
                .Fetch.Select()
                .AsSet()
                .Inverse()
                .BatchSize(20)
                .LazyLoad()
                .Key(x =>
                {
                    x.Column("NodeId");
                    x.ForeignKey(this.GenerateFkName(y => y.NodeVersions));
                })
                .Cache.IncludeAll().ReadWrite();

            AdditionalMappingInfo();
        }

        /// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
        partial void AdditionalMappingInfo();
    }

    public class NodeVersionComparer : IComparer<NodeVersion>
    {
        public int Compare(NodeVersion x, NodeVersion y)
        {
            throw new NotImplementedException();
        }
    }
}
