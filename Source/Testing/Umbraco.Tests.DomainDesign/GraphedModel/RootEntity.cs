namespace Umbraco.Tests.DomainDesign.GraphedModel
{
    public class RootEntity : Entity
    {
        public RootEntity()
        {
            GraphCache = new IndexedEntityCollection();
            Relations2 = GraphCache;
            RootEntity = this;
        }

        public IndexedEntityCollection Relations2 { get; set; }
    }
}