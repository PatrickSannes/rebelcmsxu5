namespace Umbraco.Framework.Persistence.Model.Constants.Entities
{
    public class SubContentRoot : TypedEntity
    {
        public SubContentRoot(HiveId id)
        {
            this.SetupFromSchema<RootEntitySchema>();
            this.Id = id;
            this.RelationProxies.EnlistParentById(FixedHiveIds.SystemRoot, FixedRelationTypes.DefaultRelationType);
        }
    }
}