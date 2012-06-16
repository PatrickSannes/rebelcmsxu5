using System;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.TypeMapping;

namespace Umbraco.Cms.Web.Mapping
{
    internal class MemberToMemberEditorModel<TMember, TMemberEditorModel> : TypedEntityToContentEditorModel<TMember, TMemberEditorModel>
        where TMember : Member
        where TMemberEditorModel : MemberEditorModel
    {
        public MemberToMemberEditorModel(AbstractFluentMappingEngine engine, MapResolverContext resolverContext, Action<TMember, TMemberEditorModel> additionalAfterMap = null)
            : base(engine, resolverContext, additionalAfterMap)
        {
            MappingContext
                //ignore all custom properties as these need to be mapped by the underlying attributes
                .ForMember(x => x.Name, opt => opt.Ignore())
                .ForMember(x => x.Username, opt => opt.Ignore())
                .ForMember(x => x.Password, opt => opt.Ignore())
                .ForMember(x => x.ConfirmPassword, opt => opt.Ignore())
                .ForMember(x => x.Email, opt => opt.Ignore())
                .ForMember(x => x.IsApproved, opt => opt.Ignore())
                .ForMember(x => x.LastLoginDate, opt => opt.Ignore())
                .ForMember(x => x.LastActivityDate, opt => opt.Ignore())
                .ForMember(x => x.LastPasswordChangeDate, opt => opt.Ignore());
        }
    }
}