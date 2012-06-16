using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Framework.DataManagement.Linq.ResultBinding;

namespace Umbraco.Framework.Persistence.XmlStore.DataManagement.Linq
{
    public class XElementSourceFieldBinder : SourceFieldBinder
    {
        private readonly XElement _source;

        public XElementSourceFieldBinder(XElement source)
        {
            _source = source;
        }

        public static XElementSourceFieldBinder New(object source)
        {
            if (typeof(XElement).IsAssignableFrom(source.GetType()))
            {
                return new XElementSourceFieldBinder(source as XElement);
            }

            throw new NotSupportedException(string.Format("Type of source object for field binding ({0}) isn't supported by this binder ({1})", source.GetType(),
                typeof(XElementSourceFieldBinder).Name));
        }

        public override IEnumerable<string> GetFieldNames()
        {
            var hardcodedAttributes = new[] {"name", "schema-name", "schema-id"};
            var elementAttributes = _source.Attributes().Select(xAttribute => xAttribute.Name.LocalName);
            var childAttributes = _source.Elements().Where(x=>x.Attribute("isDoc") == null).Select(xChild => xChild.Name.LocalName);
            return hardcodedAttributes.Union(elementAttributes).Union(childAttributes);
        }

        public override object Source
        {
            get { return _source; }
        }

        public override object GetFieldValue(string fieldName)
        {
            switch (fieldName.ToLowerInvariant())
            {
                case "id":
                    var value = (string) _source.Attribute(fieldName);
                    return new HiveId(Convert.ToInt32(value));
                case "schema-name":
                    return (string) _source.Name.LocalName;

            }

            if (_source.Attributes(fieldName).SingleOrDefault() != null)
                return _source.Attribute(fieldName).Value;
            var childElement = _source.Elements(fieldName).SingleOrDefault();
            if (childElement != null)
                return childElement.Value;
            return null;
        }

        public override object SetFieldValue(string fieldName, object value)
        {
            _source.Attribute(fieldName).Value = value.ToString();
            return _source;
        }
    }
}