using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.DependencyManagement
{
    public class ModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(Type modelType)
        {
            var modelBinders = DependencyResolver.Current.GetServices<Lazy<IModelBinder, ModelBinderMetadata>>();

            var modelBinder = modelBinders.FirstOrDefault(t => modelType == t.Metadata.BinderType);

            return (modelBinder != null) ? modelBinder.Value : null;
        }
    }
}
