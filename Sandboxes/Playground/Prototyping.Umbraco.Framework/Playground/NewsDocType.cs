using Umbraco.Framework;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.EntityGraph.Domain.Brokers;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using Umbraco.CMS.Domain;

namespace Prototyping.Umbraco.Framework.Playground
{
    public class NewsDocType : DocType
    {
        public NewsDocType()
        {
            // Define a name and an alias for the doctype
            // Setup<TAllowedType> is an extension method declared on EntityFactory
            this.Setup("news", "News");

            // Create a new tab group
            var newsGroup = EntityFactory.Create<AttributeGroupDefinition>("newsdata", "News Data");
            base.AttributeSchema.AttributeGroupDefinitions.Add(newsGroup);


            // Create a serialization type for persisting this to the repository
            var stringSerializer = EntityFactory.Create<StringSerializationType>("string", "String");
            stringSerializer.DataSerializationType = DataSerializationTypes.String;

            // Create a data type
            var textInputField = EntityFactory.Create<AttributeTypeDefinition>("textInputField", "Text Input Field");

            // Create a new property with that data type in our tab group
            var bodyText = EntityFactory.CreateAttributeIn<AttributeDefinition>("bodyText", "Body Text", textInputField,
                                                                                stringSerializer, newsGroup);


            // Create a data type
            var pictureInputField = EntityFactory.Create<AttributeTypeDefinition>("pictureInputField",
                                                                                  "Picture Input Field");

            // Create a new property with that data type in our tab group
            var image = EntityFactory.CreateAttributeIn<AttributeDefinition>("image", "Image", pictureInputField,
                                                                             stringSerializer, newsGroup);


            // Specify that only an Article can be a child of a News item
            var articleDocType = new ArticleDocType();
            base.GraphSchema.PermittedDescendentTypes.Add(articleDocType);
        }
    }
}