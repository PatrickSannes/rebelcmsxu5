using System;
using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Mvc.ModelBinders.BackOffice
{

    ///// <summary>
    ///// Custom model binder to support creating an instance that doesn't have a parameterless contructor
    ///// </summary>
    //[ModelBinderFor(typeof(UserEditorModel))]    
    //public class UserEditorModelBinder : StandardModelBinder
    //{
    //    private readonly IBackOfficeRequestContext _requestContext;

    //    public UserEditorModelBinder(IBackOfficeRequestContext requestContext)
    //    {
    //        _requestContext = requestContext;
    //    }

    //    protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
    //    {
    //        var hive = _requestContext.Application.Hive.GetReader(new Uri("security://members"));
    //        var model = new UserEditorModel(hive);
    //        return model;
    //    }
    //}

    /// <summary>
    /// Model binder for MediaTypeEditorModel
    /// </summary>
    [ModelBinderFor(typeof(MediaTypeEditorModel))]    
    public class MediaTypeModelBinder : StandardModelBinder
    {
        /// <summary>
        /// Binds custom properties that the default model binder wont be able to bind
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="propertyDescriptor"></param>
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor)
        {
            switch (propertyDescriptor.Name)
            {
                case "AllowedChildren":
                case "InheritFrom":
                    this.BindSelectList(controllerContext, bindingContext, propertyDescriptor);
                    break;
                default:
                    base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
                    break;
            }
        }


    }
}