using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework.Diagnostics;

namespace Umbraco.Cms.Web.PropertyEditors.ListPicker
{
    [ModelBinder(typeof(ListPickerPreValueModelBinder))]
    public class ListPickerPreValueModel : PreValueModel
    {

        // TODO: Make this public, and allow people to set a data source
        // TODO: Need to create a nice way to enter a data source. Just fully qualified path? or dropdown? how to block system data sources?
        private string DataSource { get; set; }

        [UIHint("EnumDropDownList")]
        public ListPickerMode Mode { get; set; }

        [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.ListPicker.Views.ListPickerItems.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
        public IList<ListItem> Items { get; set; }

        [AllowDocumentTypePropertyOverride]
        public bool IsRequired { get; set; }

        public override string GetSerializedValue()
        {
            var xml = new XElement("preValues");

            //mode
            xml.Add(new XElement("preValue", new XAttribute("name", "Mode"),
                            new XCData(Mode.ToString())));

            //items
            var itemsxml = new XElement("preValue",
                new XAttribute("name", "ListItems"),
                new XAttribute("dataSource", DataSource ?? ""));

            foreach (var i in Items)
            {
                itemsxml.Add(new XElement("item", 
                                new XAttribute("id", i.Id),
                                new XCData(i.Value)));
            }

            xml.Add(itemsxml);

            xml.Add(new XElement("preValue", new XAttribute("name", "IsRequired"),
                        new XCData(IsRequired.ToString())));

            return xml.ToString();
        }

        public override void SetModelValues(string serializedVal)
        {
            if (!string.IsNullOrEmpty(serializedVal))
            {
                Mode = ListPickerMode.ListBox;
                Items = new List<ListItem>();
                DataSource = "";

                try
                {
                    var xml = XElement.Parse(serializedVal);

                    ListPickerMode parsed;
                    var firstModeFromXml = xml.Elements("preValue").FirstOrDefault(x => (string)x.Attribute("name") == "Mode");
                    var parsedEnum = Enum.TryParse<ListPickerMode>(firstModeFromXml != null ? firstModeFromXml.Value : ListPickerMode.ListBox.ToString(), out parsed);
                    if (!parsedEnum) parsed = ListPickerMode.ListBox;

                    Mode = parsed;

                    var listItemsEl = xml.Elements("preValue")
                        .Where(x => (string)x.Attribute("name") == "ListItems")
                        .Single();

                    if (listItemsEl.Attribute("dataSource") != null && !string.IsNullOrEmpty(listItemsEl.Attribute("dataSource").Value))
                    {
                        DataSource = listItemsEl.Attribute("dataSource").Value;

                        var dataSourceType = Type.GetType(DataSource);
                        var dataSourceInstance = Activator.CreateInstance(dataSourceType) as IListPickerDataSource;

                        Items = dataSourceInstance.GetData().Select(x => new ListItem { Value = x.Value, Id = x.Key }).ToList();
                    }
                    else
                    {
                        Items = listItemsEl.Elements("item")
                            .Where(x => (string)x.Attribute("id") != null)
                            .Select(x => new ListItem { Value = x.Value, Id = x.Attribute("id").Value.ToString() })
                            .ToList();
                    }

                    IsRequired = xml.Elements("preValue").Any(x => (string)x.Attribute("name") == "IsRequired" && x.Value == bool.TrueString);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ListPickerPreValueModel>("Could not bind to prevalues, xml is: \n" + serializedVal, ex);
                }
            }
        }
    }
}
