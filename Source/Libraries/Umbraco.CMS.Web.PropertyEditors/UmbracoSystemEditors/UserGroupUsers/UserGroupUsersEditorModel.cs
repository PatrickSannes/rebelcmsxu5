using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.UserGroupUsers
{
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.UmbracoSystemEditors.UserGroupUsers.Views.UserGroupUsersEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class UserGroupUsersEditorModel : EditorModel
    {
        private string _userGroupName;
        private IUmbracoApplicationContext _appContext;

        public UserGroupUsersEditorModel(string userGroupName, IUmbracoApplicationContext appContext)
        {
            _userGroupName = userGroupName;
            _appContext = appContext;
        }

        public IEnumerable<SelectListItem> Value { get; set; }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            var users = new List<SelectListItem>();

            if (!string.IsNullOrEmpty(_userGroupName))
            {
                using (var uow = _appContext.Hive.OpenReader<ISecurityStore>(new Uri("security://user-groups")))
                {
                    var userGroup = uow.Repositories.GetEntityByRelationType<UserGroup>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.UserGroupVirtualRoot)
                        .SingleOrDefault(x => x.Name.ToLower() == _userGroupName.ToLower());

                    var items =
                        uow.Repositories.GetLazyChildRelations(userGroup.Id, FixedRelationTypes.UserGroupRelationType)
                            .Select(x => (TypedEntity)x.Destination)
                            .OrderBy(x => x.Attribute<string>(NodeNameAttributeDefinition.AliasValue))
                            .ToArray();

                    users.AddRange(items.Select(item => new SelectListItem { Text = item.Attribute<string>(NodeNameAttributeDefinition.AliasValue), Value = item.Id.ToString() }));
                }
            }

            Value = users;
        }

        public override IDictionary<string, object> GetSerializedValue()
        {
            return new Dictionary<string, object>();
        }
    }
}
