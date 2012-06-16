using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.UserGroupUsers
{
    [UmbracoPropertyEditor]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [PropertyEditor(CorePluginConstants.UserGroupUsersPropertyEditorId, "UserGroupUsers", "User Group Users")]
    public class UserGroupUsersEditor : ContentAwarePropertyEditor<UserGroupUsersEditorModel>
    {
        private readonly IBackOfficeRequestContext _backOfficeRequestContext;

        public UserGroupUsersEditor(IBackOfficeRequestContext backOfficeRequestContext)
        {
            Mandate.ParameterNotNull(backOfficeRequestContext, "backOfficeRequestContext");

            _backOfficeRequestContext = backOfficeRequestContext;
        }

        public override UserGroupUsersEditorModel CreateEditorModel()
        {
            return new UserGroupUsersEditorModel(IsContentModelAvailable ? GetContentModelValue<UserGroupEditorModel, string>(x => x.Name, string.Empty) : string.Empty, _backOfficeRequestContext.Application);
        }
    }
}
