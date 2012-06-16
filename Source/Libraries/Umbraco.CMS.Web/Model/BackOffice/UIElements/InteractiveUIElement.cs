namespace Umbraco.Cms.Web.Model.BackOffice.UIElements
{
    public abstract class InteractiveUIElement : UIElement
    {
        /// <summary>
        /// The title for UI Element
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the UI Element alias.
        /// </summary>
        public string Alias { get; set; }
    }
}
