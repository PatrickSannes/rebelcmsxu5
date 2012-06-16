namespace Umbraco.Framework.Persistence.Model.Constants.Entities
{
    public class SystemRoot : TypedEntity
    {
        public SystemRoot()
        {
            this.EntitySchema = new RootEntitySchema();
            this.Id = FixedHiveIds.SystemRoot;
        }
    }
}
