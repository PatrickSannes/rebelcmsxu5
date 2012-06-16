using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
    /// <summary>Represents the mapping of the 'AttributeIntegerValue' entity, represented by the 'AttributeIntegerValue' class.</summary>
    public partial class AttributeDecimalValueMap : ClassMap<AttributeDecimalValue>
    {
        /// <summary>Initializes a new instance of the <see cref="AttributeIntegerValueMap"/> class.</summary>
        public AttributeDecimalValueMap()
        {
            Table("AttributeDecimalValue");
            OptimisticLock.None();

            Id(x => x.Id)
                .Access.CamelCaseField(Prefix.Underscore)
                .Column("Id")
                .GeneratedBy.Custom<GuidCombUriGenerator>();

            Map(x => x.Value).Access.CamelCaseField(Prefix.Underscore)
                .Index(this.GenerateIndexName(x => x.Value));

            Map(x => x.ValueKey).Access.CamelCaseField(Prefix.Underscore)
                .Not.Nullable()
                .Index(this.GenerateIndexName(x => x.ValueKey));

            References(x => x.Attribute)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge()
                .Fetch.Select()
                .Not.Nullable()
                .Column("AttributeId")
                .Index(this.GenerateIndexName(x => x.Attribute));

            References(x => x.Locale)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.Merge() // TODO: This should be SaveUpdate() but need to fix mapping code; using Merge doesn't fire if cascade is SaveOrUpdate
                .Fetch.Select()
                .Not.Nullable()
                .Column("LocaleId")
                .Index(this.GenerateIndexName(x => x.Locale));

            AdditionalMappingInfo();
        }

        /// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
        partial void AdditionalMappingInfo();
    }
}