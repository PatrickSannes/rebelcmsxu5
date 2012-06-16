using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Mvc.ActionFilters;

using Umbraco.Framework;

namespace Umbraco.Cms.Web.Editors
{

    /// <summary>
    /// The base controller that all editor controllers should inherit from which supports Updating
    /// </summary>
    public abstract class StandardEditorController : DashboardEditorController, IModelUpdator
    {
        protected StandardEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }      

        /// <summary>
        /// The Action to render the editor
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public abstract ActionResult Edit(HiveId? id);

        /// <summary>
        /// The IUpdator method to update the posted model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fieldPrefix"></param>
        /// <returns></returns>
        public virtual bool BindModel(dynamic model, string fieldPrefix)
        {
            Mandate.ParameterNotNull(model, "model");
            if (string.IsNullOrEmpty(fieldPrefix))
            {
                return TryUpdateModel(model, ValueProvider);
            }
            return TryUpdateModel(model, fieldPrefix, new string[] { }, new string[] { }, ValueProvider);
        }

    }
}
