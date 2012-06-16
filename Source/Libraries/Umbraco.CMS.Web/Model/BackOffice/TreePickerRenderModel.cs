using System;
using Umbraco.Framework;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Model.BackOffice
{
    /// <summary>
    /// A model representing a tree picker
    /// </summary>
    public class TreePickerRenderModel
    {
        public TreePickerRenderModel()
        {
            Id = Name = new MvcHtmlString("gen_" + Guid.NewGuid().ToString("N"));
            ChooseLinkText = "Choose...";
            DeleteLinkText = "Delete";
            ModalTitle = "Choose item";
            StartNodeId = HiveId.Empty;
        }

        /// <summary>
        /// The ID of the tree picker input field
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public MvcHtmlString Id { get; set; }

        /// <summary>
        /// The Name of the tree picker input field
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public MvcHtmlString Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public HiveId? SelectedValue { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        public string SelectedText { get; set; }

        /// <summary>
        /// Gets or sets the tree controller id.
        /// </summary>
        /// <value>
        /// The tree controller id.
        /// </value>
        public Guid TreeControllerId { get; set; }

        /// <summary>
        /// Gets or sets the tree virtual root.
        /// </summary>
        /// <value>
        /// The tree virtual root.
        /// </value>
        public HiveId TreeVirtualRootId { get; set; }

        /// <summary>
        /// Gets or sets the start node id.
        /// </summary>
        /// <value>
        /// The start node id.
        /// </value>
        public HiveId StartNodeId { get; set; }

        /// <summary>
        /// The string to display in the picker header
        /// </summary>
        public string ModalTitle { get; set; }

        /// <summary>
        /// The JavaScript method name to call when a node is selected
        /// </summary>
        public string NodeClickHandler { get; set; }

        /// <summary>
        /// The text to display for the choose link
        /// </summary>
        public string ChooseLinkText { get; set; }

        /// <summary>
        /// The text to display for the choose link
        /// </summary>
        public string DeleteLinkText { get; set; }

        /// <summary>
        /// Allows you to pass in additional parameters to the tree which will show up as query strings in the tree controller
        /// </summary>
        public object AdditionalParameters { get; set; }
    }
}