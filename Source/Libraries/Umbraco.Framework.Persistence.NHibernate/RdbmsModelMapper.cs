using System;
using System.Reflection;
using Umbraco.Framework.Context;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.RdbmsModel;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Framework.Persistence.NHibernate
{
    public sealed class RdbmsModelMapper : AbstractFluentMappingEngine
    {
        private NhSessionHelper _nhSessionHelper;

        public RdbmsModelMapper(NhSessionHelper nhSessionHelper, IFrameworkContext frameworkContext)
            : base(frameworkContext)
        {
            _nhSessionHelper = nhSessionHelper;
        }

        public override object Map(object source, Type sourceType, Type destinationType)
        {
            if (destinationType.FullName.Contains("Rdbms"))
            {
                var identifiable = source as IReferenceByHiveId;
                if (identifiable != null && identifiable.Id.Value.Type == HiveIdValueTypes.Guid)
                {
                    var existing = _nhSessionHelper.NhSession.Get(destinationType, (Guid)identifiable.Id.Value);
                    if (existing != null)
                    {
                        base.Map(source, existing, sourceType, destinationType);
                        return existing;
                    }
                }
            }
            return base.Map(source, sourceType, destinationType);
        }

        /// <summary>
        /// Abstract method used to configure all of the mappings
        /// </summary>
        public override void ConfigureMappings()
        {
            #region NodeVersionStatusType > RevisionStatusType

            this.CreateMap<NodeVersionStatusType, RevisionStatusType>()
                .CreateUsing(x => new RevisionStatusType(x.Alias, x.Name));

            #endregion

            #region RevisionStatusType > NodeVersionStatusType

            this.CreateMap<RevisionStatusType, NodeVersionStatusType>();

            #endregion

            #region Locale > LanguageInfo

            this.CreateMap<Locale, LanguageInfo>()
                .AfterMap((s, d) => d.InferCultureFromKey());

            #endregion

            #region LanguageInfo > Locale

            this.CreateMap<LanguageInfo, Locale>()
                .ForMember(x => x.LanguageIso, opt => opt.MapFrom(x => x.Key));

            #endregion

            #region (Rdbms)AttributeType > AttributeType

            this.CreateMap<AttributeType, Model.Attribution.MetaData.AttributeType>()
                .ForMember(x => x.RenderTypeProviderConfig, opt => opt.MapFrom(x => x.XmlConfiguration))
                .AfterMap(
                    (s, t) =>
                        {
                            if (string.IsNullOrEmpty(s.PersistenceTypeProvider)) return;

                            // TODO: Use TypeFinder, but requires TypeFinder to be moved into Framework. Otherwise, add caching
                            var reverseSerializationType = Type.GetType(s.PersistenceTypeProvider, false, false);

                            
                            if (reverseSerializationType != null)
                            {
                                var create = Activator.CreateInstance(reverseSerializationType);
                                t.SerializationType = create as IAttributeSerializationDefinition;
                            }
                            else
                            {
                                throw new TypeLoadException(
                                    string.Format(
                                        "Cannot load type '{0}' which is specified for this AttributeType in the database; either the Assembly has not been loaded into the AppDomain or it's been renamed since this item was last saved.",
                                        s.PersistenceTypeProvider));
                            }
                        });

            #endregion

            #region (Model)AttributeType > AttributeType
            this.CreateMap<Model.Attribution.MetaData.AttributeType, AttributeType>()
                .ForMember(x => x.XmlConfiguration, opt => opt.MapFrom(x => x.RenderTypeProviderConfig))
                .ForMember(x => x.PersistenceTypeProvider, opt => opt.MapFrom(x =>
                    {
                        var serializationType = x.SerializationType;
                        return serializationType != null ? serializationType.GetType().AssemblyQualifiedName : null;
                    }))
                .AfterMap((s, t) =>
                    {
                        if (t.Alias == null) t.Alias = StringExtensions.ToUmbracoAlias(s.Name);
                    });
            #endregion
        }
    }
}
