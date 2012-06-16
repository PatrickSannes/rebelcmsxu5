using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;

namespace Umbraco.Cms.Web.PropertyEditors.ColorSwatchPicker
{
    /// <summary>
    /// Represents a hex value for an allowed color
    /// </summary>
    /// 

    public class ColorItem
    {
        [Required]
        [RegularExpression(
            "^\\#([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$", 
            ErrorMessage="Not a valid color please folow the following format: #ff0099")]
        [ShowLabel(false)]
        public string HexValue { get; set; }
    }



    [ModelBinder(typeof(ColorSwatchPickerPreValueModelBinder))]
    public class ColorSwatchPickerPreValueModel : PreValueModel
    {

        [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.ColorSwatchPicker.Views.ColorItems.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
        public IList<ColorItem> Colors { get; set; }

        [AllowDocumentTypePropertyOverride]
        public bool IsRequired { get; set; }

        public override string GetSerializedValue()
        {
            var xml = new XElement("preValues",
                    new XElement("preValue", new XAttribute("name", "Colors"),
                                new XCData(
                                    string.Join(",", Colors
                                    .Select(x => x.HexValue).ToArray()))),
                    new XElement("preValue", new XAttribute("name", "IsRequired"),
                        new XCData(IsRequired.ToString())));

            return xml.ToString();
        }

        public override void SetModelValues(string serializedVal)
        {
            Colors = new List<ColorItem>();
            if (!string.IsNullOrEmpty(serializedVal))
            {
                var xml = XElement.Parse(serializedVal);

                Colors =
                    xml.Elements("preValue")
                        .Where(x => (string) x.Attribute("name") == "Colors")
                        .Single().Value.ToLower()
                        .Split(',')
                        .Select(x => new ColorItem { HexValue = x })
                        .ToList();

                IsRequired = xml.Elements("preValue").Any(x => (string)x.Attribute("name") == "IsRequired" && x.Value == bool.TrueString);
            }
        }
    }
}
