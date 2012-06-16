using System;
using Umbraco.Framework.Context;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Tests.CoreAndFramework.TypeMapping
{
    public class FakeMappingEngine : AbstractFluentMappingEngine
    {
        public FakeMappingEngine(IFrameworkContext frameworkContext)
            : base(frameworkContext)
        {
        }

        public override void ConfigureMappings()
        {            
        }
    }
}