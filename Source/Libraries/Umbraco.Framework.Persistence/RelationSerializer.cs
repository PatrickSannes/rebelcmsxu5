using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;

namespace Umbraco.Framework.Persistence
{
    /// <summary>
    /// A class used to serialize/deserialize a relation
    /// </summary>
    public class RelationSerializer
    {
        public static IRelationById FromXml(string xmlString)
        {
            var xml = XDocument.Parse(xmlString);

            var relation = new RelationById(
                HiveId.Parse(xml.Root.Attribute("sourceId").Value),
                HiveId.Parse(xml.Root.Attribute("destinationId").Value),
                // TODO: Might be something other than RelationType
                new RelationType(xml.Root.Attribute("type").Value),
                Int32.Parse(xml.Root.Attribute("ordinal").Value)
                );

            if(xml.Root.HasElements)
            {
                foreach (var metaDatum in xml.Root.Elements("metaDatum"))
                {
                    relation.MetaData.Add(new RelationMetaDatum(metaDatum.Attribute("key").Value,
                                                                metaDatum.Attribute("value").Value));
                }
            }

            return relation;
        }

        public static XDocument ToXml(IReadonlyRelation<IRelatableEntity, IRelatableEntity> model)
        {
            var relation = new XElement("relation",
                new XAttribute("type", model.Type.RelationName),
                new XAttribute("sourceId", model.SourceId.ToString(HiveIdFormatStyle.AsUri)),
                new XAttribute("destinationId", model.DestinationId.ToString(HiveIdFormatStyle.AsUri)),
                new XAttribute("ordinal", model.Ordinal));

            foreach(var metaDatum in model.MetaData)
            {
                relation.Add(new XElement("metaDatum",
                    new XAttribute("key", metaDatum.Key),
                    new XAttribute("value", metaDatum.Value)));
            }

            return new XDocument(relation);
        }
    }
}
