using System;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive
{
    public static class FactoryExtensions
    {
        //BUG: Well, not sure if this is a bug or not really but there's currently only 1 way to get the root HiveId for an IO
        // entity and thats to get Hive to return the entity based on a '/' Id.
        public static HiveId GetRootNodeId(this ReadonlyGroupUnitFactory<IFileStore> factory)
        {
            using (var uow = factory.CreateReadonly())
            {
                var e = uow.Repositories.Get<File>(new HiveId("/"));
                return e.Id;
            }
        }

        //BUG: Well, not sure if this is a bug or not really but there's currently only 1 way to get the root HiveId for an IO
        // entity and thats to get Hive to return the entity based on a '/' Id.
        public static HiveId GetRootNodeId(this GroupUnitFactory<IFileStore> factory)
        {
            using (var uow = factory.Create())
            {
                var e = uow.Repositories.Get<File>(new HiveId("/"));
                return e.Id;
            }
        }

    }
}