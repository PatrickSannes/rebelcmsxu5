using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.NodeName
{
    //public class NodeNameModelBinder : DefaultModelBinder
    //{
    //    public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    //    {
    //        var unboundModel = (NodeNameEditorModel)bindingContext.Model;
    //        var unboundName = unboundModel.Name;

    //        var boundModel = (NodeNameEditorModel)base.BindModel(controllerContext, bindingContext);
    //        if (boundModel != null)
    //        {
    //            var boundName = boundModel.Name;
    //            var boundUrlName = boundModel.UrlName;

    //            // Update UrlName if empty, previous name was empty or alias was generated url name
    //            if (string.IsNullOrWhiteSpace(boundUrlName) || string.IsNullOrWhiteSpace(unboundName) ||
    //                (unboundName != boundName && boundUrlName == unboundModel.Urlify(unboundName)))
    //            {
    //                boundModel.UrlName = unboundModel.Urlify(boundName);
    //            }
    //        }

    //        return boundModel;
    //    }
    //}
}
