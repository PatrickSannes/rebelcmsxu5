// Generated: 09/02/2011 12:19:36 +00:00

using FluentNHibernate.Mapping;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings
{
	/// <summary>Represents the mapping of the 'AttributeType' entity, represented by the 'AttributeType' class.</summary>
	public partial class AttributeTypeMap : ClassMap<AttributeType>
    {
		/// <summary>Initializes a new instance of the <see cref="AttributeTypeMap"/> class.</summary>
		public AttributeTypeMap()
        {
			Table("AttributeType");
			OptimisticLock.None();

			Id(x=>x.Id)
				.Access.CamelCaseField(Prefix.Underscore)
				.Column("Id")
                .GeneratedBy.Custom<GuidCombUriGenerator>();

            Map(x => x.Alias)
               .Access.CamelCaseField(Prefix.Underscore)
               .Not.Nullable()
               .Index(this.GenerateIndexName(x => x.Alias));

			Map(x=>x.Description).CustomType("StringClob").Access.CamelCaseField(Prefix.Underscore);
			Map(x=>x.Name).Access.CamelCaseField(Prefix.Underscore);
			Map(x=>x.PersistenceTypeProvider).Access.CamelCaseField(Prefix.Underscore);
			Map(x=>x.RenderTypeProvider).Access.CamelCaseField(Prefix.Underscore);
			Map(x=>x.XmlConfiguration).CustomType("StringClob").Access.CamelCaseField(Prefix.Underscore);

            //NOTE: This was commented out for some reason, i had to uncomment it so that when deleting an Attribute type (data type)
            // the changes cascade to the entity... without this when you reload the entity there will ben an exception, plus i'm pretty
            // sure this makes sense here. SD
            HasMany(x => x.AttributeDefinitions)
                .Access.CamelCaseField(Prefix.Underscore)
                .Cascade.DeleteOrphan() // APN changed to delete orphan so saving an AttributeType only causes deletions to AttributeDefinitions, not saves
                .Fetch.Join()
                .AsSet()
                .Inverse()
                .LazyLoad() // Reverse navigator so lazy-load
                //.KeyColumns.Add("AttributeTypeId");
                .Key(x =>
                {
                    x.Column("AttributeTypeId");
                    x.ForeignKey(this.GenerateFkName(y => y.AttributeDefinitions));
                });

			AdditionalMappingInfo();
		} 
				
		/// <summary>Partial method for adding additional mapping information in a partial class./ </summary>
		partial void AdditionalMappingInfo();
	} 
}  
