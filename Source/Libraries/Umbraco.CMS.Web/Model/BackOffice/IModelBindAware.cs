namespace Umbraco.Cms.Web.Model.BackOffice
{
    /// <summary>
    /// Indicates that an object is capable of being updated
    /// </summary>
    public interface IModelBindAware
    {
        /// <summary>
        /// Updates using the object from the specified updator
        /// </summary>
        /// <param name="modelUpdator">The updator.</param>
        void BindModel(IModelUpdator modelUpdator);
    }
}