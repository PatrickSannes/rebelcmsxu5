using System;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Mapping;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.System;

using Umbraco.Framework;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.DependencyManagement.DemandBuilders
{
    /// <summary>
    /// registers all model mappers and resolvers into the container
    /// </summary>
    public class ModelMappingsDemandBuilder : IDependencyDemandBuilder
    {
        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            containerBuilder.For<MapResolverContext>().KnownAsSelf().ScopedAs.Singleton();

            // register the model mappers
            containerBuilder.For<RenderTypesModelMapper>()
                .KnownAs<AbstractMappingEngine>()
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();

            containerBuilder
                .For<FrameworkModelMapper>()
                .KnownAs<AbstractMappingEngine>()
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();

            //register model mapper for web model objects
            containerBuilder
                .For<CmsModelMapper>()
                .KnownAs<AbstractMappingEngine>()
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();

            //register model mapper for membership provider models
            containerBuilder
                .For<MembershipModelMapper>()
                .KnownAs<AbstractMappingEngine>()
                .WithMetadata<TypeMapperMetadata, bool>(x => x.MetadataGeneratedByMapper, true)
                .ScopedAs.Singleton();
        }
    }
}