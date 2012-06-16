using Umbraco.Framework.EntityGraph.Domain;
using Umbraco.Framework.EntityGraph.Domain.Brokers;
using Umbraco.Framework.EntityGraph.Domain.EntityAdaptors;

namespace Prototyping.Umbraco.Framework.Playground
{
    public class Article : Content
    {
        public Article()
        {
            // Basic setup
            this.Setup();

            // Get the DocType for this Content
            EntityType = new ArticleDocType();

            this.AsDynamic().textdata = "New text data";
        }
    }
}