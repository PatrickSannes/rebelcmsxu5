using System;
using System.Web.Mvc;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Mvc.ModelBinders.BackOffice
{
    /// <summary>
    /// Model binder for the FileEditorModel
    /// </summary>
    /// <remarks>
    /// The FileEditorModel doesn't have an empty constructor so this model binder simply creates it for the underlying DefaultModelBinder
    /// </remarks>
    [ModelBinderFor(typeof(FileEditorModel))]
    public class FileEditorModelBinder : DefaultModelBinder
    {

        /// <summary>
        /// Need to custom create the model since theres no empty constructor for the FileEditorModel
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            //try to extract the id from the request as this will give us all of the information about the file
            var idName = string.IsNullOrEmpty(bindingContext.ModelName) ? "id" : bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(idName);
            var rawId = valueProviderResult.AttemptedValue;

            var name = bindingContext.ValueProvider.GetValue("name").AttemptedValue;
            var content = bindingContext.ValueProvider.GetValue("fileContent").AttemptedValue;

            FileEditorModel model;
            if (string.IsNullOrEmpty(rawId))
            {
                model = FileEditorModel.CreateNew();
                model.Name = name;
                model.FileContent = content;
            }
            else
            {
                var id = HiveId.TryParse(rawId);
                if (id.Success)
                {
                    if (!id.Result.IsNullValueOrEmpty())
                    {
                        model = new FileEditorModel(id.Result, name, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
                                                    () => content);
                    }
                    else
                    {
                        model = FileEditorModel.CreateNew();
                        model.Name = name;
                        model.FileContent = content;
                    }
                }
                else
                {
                    throw new ArgumentException("The id parameter in the route values could not parse into a HiveId");
                }
            }

            return model;
        }
       
    }
}