using System.Web.Mvc;
using Umbraco.Cms.Web.Context;

namespace Umbraco.Cms.Web.Mvc.ControllerFactories
{
    public interface IExtendedControllerFactory : IControllerFactory
    {
        /// <summary>
        /// Gets the application context.
        /// </summary>
        /// <remarks></remarks>
        IUmbracoApplicationContext ApplicationContext { get; }
    }
}