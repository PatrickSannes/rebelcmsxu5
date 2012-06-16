using System;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;

namespace Umbraco.Framework.Persistence.Model.Constants
{
    public static class FixedGroupDefinitions
    {
        public static readonly DateTimeOffset FixedSystemDate = new DateTimeOffset(2011, 7, 1, 0, 0, 0, TimeSpan.FromHours(0));

        public static AttributeGroup FixedDates(this AttributeGroup group)
        {
            group.UtcCreated = FixedSystemDate;
            group.UtcModified = FixedSystemDate;
            group.UtcStatusChanged = FixedSystemDate;
            return group;
        }

        //TODO: All of these names need to be localization key values, not the 'real' values

        public const string GeneralGroupAlias = "umbraco-internal-general-properties";

        public static AttributeGroup GeneralGroup
        {
            get { return new AttributeGroup(GeneralGroupAlias, "General Properties", 100000).FixedDates(); }
        }

        public const string UserDetailsAlias = "umbraco-internal-user-details";
        public const string MemberDetailsAlias = "umbraco-internal-member-details";

        public static AttributeGroup MemberDetails
        {
            get { return new AttributeGroup(MemberDetailsAlias, "Member Details", 0).FixedDates(); }
        }

        public static AttributeGroup UserDetails
        {
            get { return new AttributeGroup(UserDetailsAlias, "User Details", 0).FixedDates(); }
        }

        public const string UserGroupDetailsAlias = "umbraco-internal-user-group-details";

        public static AttributeGroup UserGroupDetails
        {
            get { return new AttributeGroup(UserGroupDetailsAlias, "User Group Details", 0).FixedDates(); }
        }

        public const string TemplateDetailsAlias = "umbraco-internal-template-details";

        public static AttributeGroup TemplateDetails
        {
            get { return new AttributeGroup(TemplateDetailsAlias, "Template Details", 0).FixedDates(); }
        }

        public const string FilePropertiesAlias = "umbraco-internal-file-properties";

        public static AttributeGroup FileProperties
        {
            get { return new AttributeGroup(FilePropertiesAlias, "File Properties", 0).FixedDates(); }
        }

        public const string TranslationsAlias = "umbraco-internal-translations";

        public static AttributeGroup Translations
        {
            get { return new AttributeGroup(TranslationsAlias, "Translations", 0).FixedDates(); }
        }
    }
}