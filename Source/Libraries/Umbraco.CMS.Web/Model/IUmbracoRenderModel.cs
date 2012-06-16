namespace Umbraco.Cms.Web.Model
{

    /// <summary>
    /// The model supplied to the controller to render the content to the front-end.
    /// This model wraps the 'Content' model which is returned by the controller action to the view
    /// and should generally return the 'ContentNode' as Lazy loaded data.
    /// </summary>
    public interface IUmbracoRenderModel
    {
        /// <summary>
        /// Gets the current item associated with this request.
        /// </summary>
        /// <value>The current item.</value>
        Content CurrentNode { get; }

        
    }
}