using Umbraco.Framework.Persistence.Model.Constants.Schemas;

namespace Umbraco.Framework.Persistence.Model.Constants
{
    public static class FixedSchemas
    {
        public static UserSchema User { get { return new UserSchema(); } }
        public static UserGroupSchema UserGroup { get { return new UserGroupSchema(); } }
        public static ContentRootSchema ContentRootSchema { get { return new ContentRootSchema(); } }
        public static MediaRootSchema MediaRootSchema { get { return new MediaRootSchema(); } }
        public static MediaFolderSchema MediaFolderSchema { get { return new MediaFolderSchema(); } }
        public static MediaImageSchema MediaImageSchema { get { return new MediaImageSchema(); } }
        public static HostnameSchema HostnameSchema { get { return new HostnameSchema(); } }
        public static DictionaryRootSchema DictionaryRootSchema { get { return new DictionaryRootSchema(); } }
        public static DictionaryItemSchema DictionaryItemSchema { get { return new DictionaryItemSchema(); } }
    }
}