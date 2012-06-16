using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.PropertyEditors.RichTextBox.Resources;
using System.Text.RegularExpressions;
using System.Drawing;
using Umbraco.Framework;
using System.Web.Mvc;

using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.PropertyEditors.RichTextBox
{
    /// <summary>
    /// Represents the rich text box pre-value editor
    /// </summary>    
    [ModelBinder(typeof(RichTextBoxPreValueModelBinder))]
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.RichTextBox.Views.PreValueEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class RichTextBoxPreValueModel : PreValueModel
    {
        private readonly IUmbracoApplicationContext _appContext;

        public RichTextBoxPreValueModel(IUmbracoApplicationContext appContext)
        {
            _appContext = appContext;
        }

        /// <summary>
        /// The valid TinyMCE elements to support, by default this is empty which means TinyMCE will use the defaults
        /// see: http://www.tinymce.com/wiki.php/Configuration:valid_elements
        /// </summary>
        public string ValidElements { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the label or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if should show label; otherwise, <c>false</c>.
        /// </value>
        public bool ShowLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the context menu or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if should show the context menu; otherwise, <c>false</c>.
        /// </value>
        public bool ShowContextMenu { get; set; }

        /// <summary>
        /// Gets or sets the size of the editor (width/height)
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.RichTextBox.Views.SizeInputField.cshtml", "Umbraco.Cms.Web.PropertyEditors")]        
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the list of features supported by the rich text box.
        /// </summary>
        /// <value>
        /// The features.
        /// </value>
        [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.RichTextBox.Views.FeaturesCheckboxList.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
        public IEnumerable<SelectListItem> Features { get; set; }

        /// <summary>
        /// Gets or sets the stylesheets supported by the rich text box.
        /// </summary>
        /// <value>
        /// The stylesheets.
        /// </value>
        [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.RichTextBox.Views.StylesheetsCheckboxList.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
        public IEnumerable<SelectListItem> Stylesheets { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        [AllowDocumentTypePropertyOverride]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Returns the features that TinyMCE is capable of supporting (i.e. bold, italic, etc...)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SelectListItem> GetFeatures()
        {
            var xml = XDocument.Parse(RichTextBoxResources.FeaturesList);
            return xml.Root.Elements("feature").Select(x => new SelectListItem
            {
                Text = (string)x.Attribute("name"),
                Value = (string)x.Attribute("alias")
            }).ToArray();
        }

        /// <summary>
        /// Returns the stylesheets available within umbraco
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SelectListItem> GetStylesheets()
        {
            using (var uow = _appContext.Hive.OpenReader<IFileStore>(new Uri("storage://stylesheets")))
            {
                var stylesheet = uow.Repositories.GetAll<Umbraco.Framework.Persistence.Model.IO.File>();
                return stylesheet.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToArray();
            }
        }

        /// <summary>
        /// Return a serialized string of values for the pre value editor model
        /// </summary>
        /// <returns></returns>
        public override string GetSerializedValue()
        {
            var xml = new XElement("preValues",
                new XElement("preValue", new XAttribute("name", "ShowLabel"), new XCData(ShowLabel.ToString())),
                new XElement("preValue", new XAttribute("name", "ShowContextMenu"), new XCData(ShowContextMenu.ToString())),                
                new XElement("preValue", new XAttribute("name", "Size"),
                            new XCData(Size.Width + "x" + Size.Height)),
                new XElement("preValue", new XAttribute("name", "Features"),
                            new XCData(
                                string.Join(",", Features
                                .Where(x => x.Selected)
                                .Select(x => x.Value).ToArray()))),
                new XElement("preValue", new XAttribute("name", "Stylesheets"),
                            new XCData(
                                string.Join(",", Stylesheets
                                .Where(x => x.Selected)
                                .Select(x => x.Value).ToArray()))),
                new XElement("preValue", new XAttribute("name", "IsRequired"),
                            new XCData(IsRequired.ToString())));

            if (!ValidElements.IsNullOrWhiteSpace())
            {
                xml.Add(new XElement("preValue", new XAttribute("name", "validElements"), new XCData(ValidElements.Trim())));
            }

            return xml.ToString();
        }

        /// <summary>
        /// set the pre values from the serialized values in the repository
        /// </summary>
        /// <param name="serializedVal"></param>
        public override void SetModelValues(string serializedVal)
        {
            if (_appContext == null)
                return; // Need an app context to set model values

            Stylesheets = GetStylesheets();
            Features = GetFeatures();
            Size = new Size(650, 400);

            if (!string.IsNullOrEmpty(serializedVal))
            {
                var xml = XElement.Parse(serializedVal);

                ShowLabel = xml.Elements("preValue").Any(x => (string)x.Attribute("name") == "ShowLabel" && x.Value == bool.TrueString);
                ShowContextMenu = xml.Elements("preValue").Any(x => (string)x.Attribute("name") == "ShowContextMenu" && x.Value == bool.TrueString);
                ValidElements = (string)xml.Elements("preValue").Where(x => (string)x.Attribute("name") == "validElements").SingleOrDefault();
                var featuresEl = xml.Elements("preValue").Where(x => (string)x.Attribute("name") == "Features").SingleOrDefault();
                if(featuresEl != null)
                {
                    var features = featuresEl.Value.ToUpper().Split(',');
                    foreach (var e in Features.Where(x => features.Contains(x.Value.ToUpper())))
                    {
                        e.Selected = true;
                    }
                }

                var stylesheetsEl = xml.Elements("preValue").Where(x => (string)x.Attribute("name") == "Stylesheets").SingleOrDefault();
                if (stylesheetsEl != null)
                {
                    var stylesheets = stylesheetsEl.Value.ToUpper().Split(',');
                    foreach (var e in Stylesheets.Where(x => stylesheets.Contains(x.Value.ToUpper())))
                    {
                        e.Selected = true;
                    }
                }

                var sizeEl = xml.Elements("preValue").Where(x => (string)x.Attribute("name") == "Size").SingleOrDefault();
                if(sizeEl != null)
                {
                    var size = sizeEl.Value.Split('x');
                    var width = Size.Width;
                    var height = Size.Height;
                    if (int.TryParse(size[0], out width) && int.TryParse(size[1], out height))
                    {
                        Size = new Size(width, height);
                    }
                }

                IsRequired = xml.Elements("preValue").Any(x => (string)x.Attribute("name") == "IsRequired" && x.Value == bool.TrueString);
            }
        }
    }
}
