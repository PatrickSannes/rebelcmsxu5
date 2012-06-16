﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Mvc.Controllers;


namespace Umbraco.Cms.Web.Mvc.ActionInvokers
{
    /// <summary>
    /// Ensures that if an action for the Template name is not explicitly defined by a user, that the 'Index' action will execute
    /// </summary>
    public class UmbracoActionInvoker : RoutableRequestActionInvoker
    {
        public UmbracoActionInvoker(IRoutableRequestContext routableRequestContext)
            : base(routableRequestContext)
        { }

        /// <summary>
        /// Ensures that if an action for the Template name is not explicitly defined by a user, that the 'Index' action will execute
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="controllerDescriptor"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        protected override ActionDescriptor FindAction(ControllerContext controllerContext, ControllerDescriptor controllerDescriptor, string actionName)
        {
            var ad = base.FindAction(controllerContext, controllerDescriptor, actionName);
            
            //now we need to check if it exists, if not we need to return the Index by default
            if (ad == null)
            {
                //check if the controller is an instance of IUmbracoController
                if (controllerContext.Controller is UmbracoController)
                {
                    //return the Index method info object of the IUmbracoController
                    return new ReflectedActionDescriptor(((UmbracoController)controllerContext.Controller).GetType().GetMethod("Index"), "Index", controllerDescriptor);
                }
            }
            return ad;
        }

    }
}