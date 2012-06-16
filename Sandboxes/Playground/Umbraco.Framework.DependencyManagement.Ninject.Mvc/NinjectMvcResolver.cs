using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Ninject;

namespace Umbraco.Framework.DependencyManagement.Ninject.Mvc
{
    public class NinjectMvcResolver : NinjectResolver, System.Web.Mvc.IDependencyResolver
    {
        private readonly NinjectResolver _resolver;

        public NinjectMvcResolver(NinjectResolver resolver, IKernel kernel) : base(kernel)
        {
            _resolver = resolver;
        }

        /// <summary>
        /// Resolves singly registered services that support arbitrary object creation.
        /// </summary>
        /// <returns>
        /// The requested service or object.
        /// </returns>
        /// <param name="serviceType">The type of the requested service or object.</param>
        public object GetService(Type serviceType)
        {
            return base.Kernel.GetService(serviceType);
        }

        /// <summary>
        /// Resolves multiply registered services.
        /// </summary>
        /// <returns>
        /// The requested services.
        /// </returns>
        /// <param name="serviceType">The type of the requested services.</param>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return base.Kernel.GetAll(serviceType);
        }
    }
}
