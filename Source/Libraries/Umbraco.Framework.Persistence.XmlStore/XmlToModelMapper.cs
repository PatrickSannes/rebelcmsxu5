using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;

namespace Umbraco.Framework.Persistence.XmlStore
{
    public static class XmlToModelMapper
    {
        public static TypedEntity MapTypedEntity(XElement xElement)
        {
            Mandate.ParameterNotNull(xElement, "xElement");

            var attribs = new HashSet<TypedAttribute>();
            var ordinal = 0;
            foreach (var childElement in xElement.Elements().Where(x => !x.HasAttributes))
            {
                var typedAttribute = new TypedAttribute(new AttributeDefinition()
                                                            {
                                                                Alias = childElement.Name.LocalName,
                                                                Name = childElement.Name.LocalName,
                                                                Ordinal = ordinal,
                                                                Id = HiveId.Empty
                                                            }, childElement.Value)
                                         {
                                             Id = HiveId.Empty
                                         };
                attribs.Add(typedAttribute);
                ordinal++;
            }

            var nodeId = (int)xElement.Attribute("id");
            var returnValue = new TypedEntity
            {
                // TODO: Replace provider id with injected value inside UoWFactory
                Id = new HiveId("content", "r-xmlstore-01", new HiveIdValue(nodeId))
            };
            returnValue.Attributes.Reset(attribs);

            return returnValue;
        }

    }
}