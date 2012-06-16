using System;
using System.Collections.Generic;

namespace Umbraco.Framework.Persistence.Model.Constants
{
    //TODO: SD: I've commented out unused ones until we need them!

    /// <summary>
    /// Reserved Hive Ids
    /// </summary>
    public static class FixedHiveIds
    {
        public static readonly HiveId SystemRoot = HiveId.ConvertIntToGuid("system", null, -1);
        public static readonly HiveId ContentVirtualRoot = HiveId.ConvertIntToGuid("system", null, -2);
        public static readonly HiveId MediaVirtualRoot = HiveId.ConvertIntToGuid("system", null, -3);
        public static readonly HiveId ContentRecylceBin = HiveId.ConvertIntToGuid("content", null, -4);
        public static readonly HiveId MediaRecylceBin = HiveId.ConvertIntToGuid("media", null, -7);
        public static readonly HiveId UserVirtualRoot = HiveId.ConvertIntToGuid(new Uri("security://users"), null, -5);
        public static readonly HiveId UserGroupVirtualRoot = HiveId.ConvertIntToGuid(new Uri("security://user-groups"), null, -6);
        public static readonly HiveId DictionaryVirtualRoot = HiveId.ConvertIntToGuid(new Uri("dictionary://"), null, -10);
        
        public static readonly HiveId RootSchema = HiveId.ConvertIntToGuid("system", null, -500);

        //public static readonly HiveId SecurityGroupRoot = HiveId.ConvertIntToGuid("system", null, -600);
        //public static readonly HiveId SecurityPrincipleRoot = HiveId.ConvertIntToGuid("system", null, -601);
        //public static readonly HiveId MemberRoot = HiveId.ConvertIntToGuid("system", null, -602);

        public static readonly HiveId DictionaryRootSchema = HiveId.ConvertIntToGuid("system", null, -708);
        public static readonly HiveId DictionaryItemSchema = HiveId.ConvertIntToGuid("dictionary", null, -709);

        public static readonly HiveId UserSchema = HiveId.ConvertIntToGuid("system", null, -701);
        public static readonly HiveId UserGroupSchema = HiveId.ConvertIntToGuid("system", null, -702);
        //public static readonly HiveId MemberSchema = HiveId.ConvertIntToGuid("system", null, -703);
        public static readonly HiveId ContentRootSchema = HiveId.ConvertIntToGuid("system", null, -704);
        public static readonly HiveId MediaRootSchema = HiveId.ConvertIntToGuid("system", null, -705);
        public static readonly HiveId HostnameSchema = HiveId.ConvertIntToGuid("system", null, -706);
        public static readonly HiveId MemberSchema = HiveId.ConvertIntToGuid("system", null, -707);
        //we can't make this negative as that will imply they are 'system' and not editable
        public static readonly HiveId MediaFolderSchema = new HiveId("media", null, new HiveIdValue("mfs".EncodeAsGuid()));
        public static readonly HiveId MediaImageSchema = new HiveId("media", null, new HiveIdValue("mis".EncodeAsGuid()));

        public static readonly HiveId DraftStatusType = HiveId.ConvertIntToGuid(-100);
        public static readonly HiveId PublishedStatusType = HiveId.ConvertIntToGuid(-101);
        public static readonly HiveId CreatedStatusType = HiveId.ConvertIntToGuid(-102);
        public static readonly HiveId UnpublishedStatusType = HiveId.ConvertIntToGuid(-103);
        public static readonly HiveId SavedStatusType = HiveId.ConvertIntToGuid(-104);

        public static readonly HiveId StringAttributeType = HiveId.ConvertIntToGuid(-300);
        public static readonly HiveId TextAttributeType = HiveId.ConvertIntToGuid(-301);
        public static readonly HiveId IntegerAttributeType = HiveId.ConvertIntToGuid(-302);
        public static readonly HiveId DateTimeAttributeType = HiveId.ConvertIntToGuid(-303);
        public static readonly HiveId BoolAttributeType = HiveId.ConvertIntToGuid(-304);
        public static readonly HiveId ReadOnlyAttributeType = HiveId.ConvertIntToGuid(-305);
        public static readonly HiveId ContentPickerAttributeType = HiveId.ConvertIntToGuid(-310);
        public static readonly HiveId MediaPickerAttributeType = HiveId.ConvertIntToGuid(-311);
        public static readonly HiveId ApplicationsListPickerAttributeType = HiveId.ConvertIntToGuid(-312);
        public static readonly HiveId PermissionsListPickerAttributeType = HiveId.ConvertIntToGuid(-314);
        public static readonly HiveId UserGroupsListPickerAttributeType = HiveId.ConvertIntToGuid(-315);
        public static readonly HiveId FileUploadAttributeType = HiveId.ConvertIntToGuid(-316);
        public static readonly HiveId DictionaryItemTranslationsAttributeType = HiveId.ConvertIntToGuid(-317);
        public static readonly HiveId NodeNameAttributeTypeId = HiveId.ConvertIntToGuid(-306);
        public static readonly HiveId SelectedTemplateAttributeTypeId = HiveId.ConvertIntToGuid(-307);
        public static readonly HiveId ByteArrayAttributeType = HiveId.ConvertIntToGuid(-308);
        public static readonly HiveId UserGroupMemberAttributeType = HiveId.ConvertIntToGuid(-308);
    }
}