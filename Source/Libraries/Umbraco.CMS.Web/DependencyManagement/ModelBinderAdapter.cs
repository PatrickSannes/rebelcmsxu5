using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Web.Mvc;

using Umbraco.Framework;

namespace Umbraco.Cms.Web.DependencyManagement
{
    public class ModelBinderAdapter : IModelBinder 
    {
        private readonly Type _modelBinderType;

        public ModelBinderAdapter(Type modelBinderType)
        {
            Mandate.ParameterNotNull(modelBinderType, "modelBinderType");
            _modelBinderType = modelBinderType;
        }

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var binder = DependencyResolver.Current.GetService(_modelBinderType) as IModelBinder;
            if (binder == null)
            {
                binder = ((IModelBinder)Activator.CreateInstance(_modelBinderType));
            }
            return binder.BindModel(controllerContext, bindingContext);
        }
    }
}
