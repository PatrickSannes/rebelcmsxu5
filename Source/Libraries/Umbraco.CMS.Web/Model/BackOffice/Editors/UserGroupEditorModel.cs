using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    public class UserGroupEditorModel : BasicContentEditorModel
    {

        /// <summary>
        /// Parent is always the UserVirtualRoot
        /// </summary>
        public override HiveId ParentId
        {
            get { return FixedHiveIds.UserGroupVirtualRoot; }
            set { return; }
        }

        /// <summary>
        /// Override the 'Name' property to lookup/retreive from the Name dynamic property of this object
        /// </summary>
        [Required]
        public override string Name
        {
            get { return GetPropertyEditorModelValue(NodeNameAttributeDefinition.AliasValue, x => x.Value); }
            set { SetPropertyEditorModelValue(NodeNameAttributeDefinition.AliasValue, x => x.Value = value); }
        }

        /// <summary>
        /// Gets or sets the permissions.
        /// </summary>
        /// <value>
        /// The permissions.
        /// </value>
        public IEnumerable<PermissionStatusModel> Permissions { get; set; }

        public IEnumerable<SelectListItem> Users
        {
            get { return GetPropertyEditorModelValue(UserGroupSchema.UsersAlias, x => x.Value); }
            set { SetPropertyEditorModelValue(UserGroupSchema.UsersAlias, x => x.Value = value); }
        }

        protected override void PopulateUIElements()
        {
            UIElements.Clear();
            UIElements.Add(new SaveButtonUIElement());
        }
    }
}
