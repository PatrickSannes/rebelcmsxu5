using System.Web.Mvc;

namespace Umbraco.Cms.Web.Model.BackOffice
{
    /// <summary>
    /// Interface for adding the ability to update a model with new data (i.e. - from a form post)
    /// </summary>
    public interface IModelUpdator
    {
        /// <summary>
        /// Updates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="fieldPrefix">The field prefix.</param>
        bool BindModel(dynamic model, string fieldPrefix);

        ModelStateDictionary ModelState { get; }
    }
}