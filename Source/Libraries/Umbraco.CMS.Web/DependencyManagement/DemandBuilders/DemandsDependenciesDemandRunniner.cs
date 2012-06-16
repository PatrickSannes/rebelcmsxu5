using System;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.DependencyManagement;

namespace Umbraco.Cms.Web.DependencyManagement.DemandBuilders
{
    /// <summary>
    /// Runs any IDependencyDemandBuilder attached to a Type
    /// </summary>
    internal class DemandsDependenciesDemandRunniner
    {
        internal static void Run(IContainerBuilder containerBuilder, Type type)
        {
            // Check to see if the type has DemandsDependenciesAttribute, and if so, invoke the builder
            var attribs = type.GetCustomAttributes(typeof(DemandsDependenciesAttribute), true)
                .OfType<DemandsDependenciesAttribute>();
            var demandBuilders = attribs.Select(attrib => Activator.CreateInstance(attrib.DemandBuilderType))
                .OfType<IDependencyDemandBuilder>();
            foreach(var d in demandBuilders)
            {
                d.Build(containerBuilder, containerBuilder.Context);
            }
        }

    }
}