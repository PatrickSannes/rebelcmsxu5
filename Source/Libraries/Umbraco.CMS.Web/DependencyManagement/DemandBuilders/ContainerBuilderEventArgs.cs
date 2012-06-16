using System;
using Umbraco.Framework.DependencyManagement;

namespace Umbraco.Cms.Web.DependencyManagement.DemandBuilders
{
    public class ContainerBuilderEventArgs : EventArgs
    {
        public ContainerBuilderEventArgs(IContainerBuilder builder)
        {
            Builder = builder;
        }

        public IContainerBuilder Builder { get; private set; }
    }
}