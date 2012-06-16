using System;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.ActionInvokers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ExtendedByAttribute : Attribute
    {
        public Type ControllerExtenderType { get; private set; }

        public ExtendedByAttribute(Type controllerExtenderType)
        {
            ControllerExtenderType = controllerExtenderType;
            Mandate.That(typeof(Controller).IsAssignableFrom(controllerExtenderType), 
                         x => new ArgumentException("controllerExtenderType must be of type Controller"));
        }

        public object[] AdditionalParameters { get; set; }

    }
}