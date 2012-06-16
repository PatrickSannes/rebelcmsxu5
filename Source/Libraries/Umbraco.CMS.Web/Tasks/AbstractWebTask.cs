using System;
using Umbraco.Cms.Web.Context;
using Umbraco.Framework.Context;
using Umbraco.Framework.Tasks;

namespace Umbraco.Cms.Web.Tasks
{
    public abstract class AbstractWebTask : AbstractTask
    {
        protected AbstractWebTask(IUmbracoApplicationContext applicationContext) : base(applicationContext.FrameworkContext)
        {
            ApplicationContext = applicationContext;
        }

        public IUmbracoApplicationContext ApplicationContext { get; private set; }
    }
}