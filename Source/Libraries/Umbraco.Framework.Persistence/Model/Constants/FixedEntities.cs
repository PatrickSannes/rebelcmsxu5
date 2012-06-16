using Umbraco.Framework.Persistence.Model.Constants.Entities;

namespace Umbraco.Framework.Persistence.Model.Constants
{
    public static class FixedEntities
    {
        public static readonly SubContentRoot MediaVirtualRoot = new SubContentRoot(FixedHiveIds.MediaVirtualRoot);
        public static readonly SubContentRoot MediaRecycleBin = new SubContentRoot(FixedHiveIds.MediaRecylceBin);
        public static readonly SubContentRoot ContentVirtualRoot = new SubContentRoot(FixedHiveIds.ContentVirtualRoot);
        public static readonly SubContentRoot ContentRecycleBin = new SubContentRoot(FixedHiveIds.ContentRecylceBin);
        public static readonly SubContentRoot DictionaryVirtualRoot = new SubContentRoot(FixedHiveIds.DictionaryVirtualRoot);

        public static SubContentRoot UserVirtualRoot
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.UserVirtualRoot);
            }
        } 
        
        public static SubContentRoot UserGroupVirtualRoot
        {
            get
            {
                return new SubContentRoot(FixedHiveIds.UserGroupVirtualRoot);
            }
        }
    }
}