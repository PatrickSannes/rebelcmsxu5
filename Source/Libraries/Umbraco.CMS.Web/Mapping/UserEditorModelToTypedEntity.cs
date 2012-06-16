using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    internal class UserEditorModelToTypedEntity : ContentEditorModelToTypedEntity<UserEditorModel, User>
    {
        public UserEditorModelToTypedEntity(AbstractFluentMappingEngine engine, MapResolverContext resolverContext, params string[] ignoreAttributeAliases)
            : base(engine, resolverContext, ignoreAttributeAliases)
        {
            MappingContext
                //ignore all custom properties (except password) as these need to be mapped by the underlying attributes
                .IgnoreMember(x => x.LastPasswordChangeDate)
                .IgnoreMember(x => x.LastActivityDate)
                .IgnoreMember(x => x.LastLoginDate)
                .IgnoreMember(x => x.IsApproved)
                .IgnoreMember(x => x.Email)
                .IgnoreMember(x => x.Username)
                .IgnoreMember(x => x.Name)
                .IgnoreMember(x => x.Applications)
                .IgnoreMember(x => x.StartMediaHiveId)
                .IgnoreMember(x => x.StartContentHiveId)
                .IgnoreMember(x => x.SessionTimeout);
        }
    }
}