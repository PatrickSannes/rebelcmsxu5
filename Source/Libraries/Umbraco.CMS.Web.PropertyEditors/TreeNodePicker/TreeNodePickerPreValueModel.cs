using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees;

namespace Umbraco.Cms.Web.PropertyEditors.TreeNodePicker
{
    [Bind(Exclude = "TreeId")]
    [ModelBinder(typeof(TreeNodePickerPreValueModelBinder))]
    public class TreeNodePickerPreValueModel : PreValueModel
    {
        private readonly IEnumerable<Lazy<TreeController, TreeMetadata>> _trees;

        public TreeNodePickerPreValueModel(IEnumerable<Lazy<TreeController, TreeMetadata>> trees)
        {
            _trees = trees;            
        }

        /// <summary>
        /// Gets or sets the tree id.
        /// </summary>
        /// <value>
        /// The tree id.
        /// </value>
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public Guid TreeId
        {
            get
            {
                var value = AvailableTrees.Where(x => x.Selected).Select(x => x.Value).FirstOrDefault();
                return value == null ? default(Guid) : new Guid(value);
            }
            set
            {
                var item = AvailableTrees.SingleOrDefault(x => x.Value == value.ToString("N"));
                if(item != null) 
                    item.Selected = true;
            }
        }


        /// <summary>
        /// Gets or sets the available trees.
        /// </summary>
        /// <value>
        /// The available trees.
        /// </value>
        [Display(Name = "Tree type")]
        [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.TreeNodePicker.Views.TreeTypeDropDown.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
        public IEnumerable<SelectListItem> AvailableTrees { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        [AllowDocumentTypePropertyOverride]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Returns the trees available within umbraco
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SelectListItem> GetAvailableTrees()
        {
            return _trees.Where(x =>
                    x.Metadata.Id == new Guid(CorePluginConstants.ContentTreeControllerId) ||
                    x.Metadata.Id == new Guid(CorePluginConstants.MediaTreeControllerId))
                .Select(x => new SelectListItem
                {
                    Text = x.Metadata.TreeTitle,
                    Value = x.Metadata.Id.ToString("N")
                })
                .OrderBy(x => x.Text)
                .ToList();
        }

        /// <summary>
        /// Get the list of Ids and create the select list
        /// </summary>
        /// <param name="serializedVal"></param>
        public override void SetModelValues(string serializedVal)
        {
            if (_trees == null)
                return; // Need a list of trees to set modle values

            AvailableTrees = GetAvailableTrees();

            if (!string.IsNullOrWhiteSpace(serializedVal))
            {
                var xml = XElement.Parse(serializedVal);

                var treeIdEl = xml.Elements("preValue").SingleOrDefault(x => (string)x.Attribute("name") == "TreeId");
                if (treeIdEl != null)
                {
                    TreeId = new Guid(treeIdEl.Value);
                }

                IsRequired = xml.Elements("preValue").Any(x => (string)x.Attribute("name") == "IsRequired" && x.Value == bool.TrueString);
            }
        }

        /// <summary>
        /// Return a serialized string of values for the pre value editor model
        /// </summary>
        /// <returns></returns>
        public override string GetSerializedValue()
        {
            var xml = new XElement("preValues",
                new XElement("preValue", new XAttribute("name", "TreeId"), new XCData(TreeId.ToString("N"))),
                new XElement("preValue", new XAttribute("name", "IsRequired"), new XCData(IsRequired.ToString())));

            return xml.ToString();
        }
    }
}
