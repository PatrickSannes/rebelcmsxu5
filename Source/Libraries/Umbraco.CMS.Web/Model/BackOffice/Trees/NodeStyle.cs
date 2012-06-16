using System.Collections.Generic;

namespace Umbraco.Cms.Web.Model.BackOffice.Trees
{

    /// <summary>
    /// A class used to style a tree model
    /// </summary>
    public sealed class NodeStyle
    {
        internal NodeStyle()
        {
            AppliedClasses = new List<string>();
        }

        private const string DimNodeCssClass = "dim";
        private const string HighlightNodeCssClass = "overlay-new";
        private const string SecureNodeCssClass = "overlay-protect";

        internal List<string> AppliedClasses { get; private set; }

        /// <summary>
        /// Dims the color of the model
        /// </summary>
        public void DimNode()
        {
            if (!AppliedClasses.Contains(DimNodeCssClass))
                AppliedClasses.Add(DimNodeCssClass);
        }

        /// <summary>
        /// Adds the star icon highlight overlay to a model
        /// </summary>
        public void HighlightNode()
        {
            if (!AppliedClasses.Contains(HighlightNodeCssClass))
                AppliedClasses.Add(HighlightNodeCssClass);
        }

        /// <summary>
        /// Adds the padlock icon overlay to a model
        /// </summary>
        public void SecureNode()
        {
            if (!AppliedClasses.Contains(SecureNodeCssClass))
                AppliedClasses.Add(SecureNodeCssClass);
        }

        /// <summary>
        /// Adds a custom class to the li model of the tree
        /// </summary>
        /// <param name="cssClass"></param>
        public void AddCustom(string cssClass)
        {
            if (!AppliedClasses.Contains(cssClass))
                AppliedClasses.Add(cssClass);
        }
    }
}
