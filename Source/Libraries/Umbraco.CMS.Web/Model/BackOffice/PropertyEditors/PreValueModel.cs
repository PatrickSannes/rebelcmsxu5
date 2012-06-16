using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.PropertyEditors
{
    /// <summary>
    /// Abstract class representing a Property Editor's model to render it's Pre value editor
    /// </summary>
    public abstract class PreValueModel
    {                     

        private ModelMetadata _modelMetadata;
        
        /// <summary>
        /// Returns the meta data for the current pre value model
        /// </summary>
        protected ModelMetadata MetaData
        {
            get
            {
                return _modelMetadata ?? (_modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => this, GetType()));
            }
        }

        /// <summary>
        /// Return a serialized string of values for the pre value editor model
        /// </summary>
        /// <returns></returns>
        public virtual string GetSerializedValue()
        {
            //get all editable properties
            var editableProps = MetaData.Properties.Where(x => x.ShowForEdit);
            var xmlBody = new XElement("preValues");
            foreach (var p in editableProps)
            {
                //by default, we will not support complex modelled properties, developers will need to override
                //the GetSerializedValue method if they need support for this.
                if (p.IsComplexType)
                {
                    //TODO: We should magically support this
                    throw new NotSupportedException("The default serialization implementation of PreValueModel does not support properties that are complex models");
                }

                var xmlItem = new XElement("preValue",
                    new XAttribute("name", p.PropertyName),
                    new XAttribute("type", p.ModelType.FullName),
                    new XCData(p.Model.ToXmlString(p.ModelType)));

                xmlBody.Add(xmlItem);
            }
            return xmlBody.ToString();
        }
                
        /// <summary>
        /// called by the subsystem to load the values from the data store into the model
        /// </summary>
        /// <param name="serializedVal"></param>
        public virtual void SetModelValues(string serializedVal)
        {
            if (string.IsNullOrEmpty(serializedVal))
            {
                return;
            }

            var xml = XElement.Parse(serializedVal);
            var modelProperties = GetType().GetProperties();
            if (xml.Name != "preValues")
            {
                throw new XmlException("The XML format for the serialized value is invalid");
            }
            foreach (var xmlPreValue in xml.Elements("preValue"))
            {
                if (xmlPreValue.Attribute("name") != null)
                {
                    //get the property with the name
                    var prop = modelProperties.Where(x => x.Name == (string)xmlPreValue.Attribute("name")).SingleOrDefault();
                    if (prop != null)
                    {
                        //set the property value
                        var converter = TypeDescriptor.GetConverter(prop.PropertyType);
                        if (converter != null) prop.SetValue(this, converter.ConvertFromString(xmlPreValue.Value), null);
                    }
                }
            }
        }
    }
}
