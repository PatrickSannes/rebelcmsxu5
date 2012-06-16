using Umbraco.Framework;
using Umbraco.Framework.Data.Common;
using Umbraco.Framework.EntityGraph.Domain.Brokers;
using Umbraco.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph;
using Umbraco.CMS.Domain;

namespace Prototyping.Umbraco.Framework.Playground
{
    public class ArticleDocType : DocType
    {
        public ArticleDocType()
        {
            // Mock-up a fake root
            var root = new TypedEntityVertex().SetupRoot();

            // Define a name and an alias for the doctype
            // Setup<TAllowedType> is an extension method declared on EntityFactory)
            this.SetupTypeDefinition("article", "Article", root);

            // Create a new tab group
            var textGroup = EntityFactory.Create<AttributeGroupDefinition>("textdata", "Text Data");
            base.AttributeSchema.AttributeGroupDefinitions.Add(textGroup);

            // Create a data type
            var textInputField = EntityFactory.Create<AttributeTypeDefinition>("textInputField", "Text Input Field");

            // Create a serialization type for persisting this to the repository
            var stringSerializer = EntityFactory.Create<StringSerializationType>("string", "String");
            stringSerializer.DataSerializationType = DataSerializationTypes.String;

            // Create a new property with that data type in our tab group
            var bodyText = EntityFactory.CreateAttributeIn<AttributeDefinition>("bodyText", "Body Text", textInputField,
                                                                                stringSerializer, textGroup);

            // Specify that an Article can be a child of an Article
            base.GraphSchema.PermittedDescendentTypes.Add(this);
        }
    }
}