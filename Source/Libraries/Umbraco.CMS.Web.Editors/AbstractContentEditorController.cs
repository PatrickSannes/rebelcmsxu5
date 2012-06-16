using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors
{
    /// <summary>
    /// Abstract controller for use with BasicContentEditorModel types
    /// </summary>
    [SupportClientNotifications]
    [SupportModelEditing]
    public abstract class AbstractContentEditorController : StandardEditorController
    {
        protected AbstractContentEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        /// <summary>
        /// The hive provider to use for data access calls
        /// </summary>
        public abstract GroupUnitFactory Hive { get; }

        [UmbracoAuthorize(Permissions = new[] {FixedPermissionIds.Update})]
        public abstract override ActionResult Edit(HiveId? id);

        /// <summary>
        /// JSON action to delete a node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Delete })]
        public virtual JsonResult Delete(HiveId? id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            using (var uow = Hive.Create<IContentStore>())
            {
                uow.Repositories.Delete<TypedEntity>(id.Value);                                
                uow.Complete();
            }           

            //return a successful JSON response
            return Json(new { message = "Success" });
        }

        /// <summary>
        /// This method is used for creating new content and for updating an existing entity with new attributes that may have been
        /// created on the document type (attribution schema).
        /// </summary>
        /// <param name="contentViewModel"></param>
        /// <remarks>
        /// We need to paste the model back together now with the correct Ids since we generated them on the last Action and we've re-generated them again now.
        /// This is done by looking up the Alias Key/Value pair in the posted form element for each of the model's properties. When the key value pair is found
        /// we can extract the id that was assigned to it in the HTML markup and re-assign that id to the actual property id so it binds.
        /// </remarks>
        protected void ReconstructModelPropertyIds(BasicContentEditorModel contentViewModel)
        {
            foreach (var p in contentViewModel.Properties)
            {
                var alias = p.Alias;
                //find the alias form post key/value pair that matches this alias
                var aliasKey = (Request.Form.AllKeys
                    .Where(x => x.EndsWith("__Alias__")
                                && x.StartsWith(HiveIdExtensions.HtmlIdPrefix))
                    .Where(key => Request.Form[key] == alias)).SingleOrDefault();
                if (aliasKey != null)
                {
                    //now we can extract the ID that this property was related to
                    var originalHiveId = HiveIdExtensions.TryParseFromHtmlId(aliasKey); //HiveId.TryParse(aliasKey.Split('.')[0].Split('_')[1]);)
                    if (originalHiveId.Success)
                    {
                        //find the new editor model property with the attribute definition so we can update it's Ids to the original Ids so they bind
                        var contentProperty = contentViewModel.Properties.Where(x => x.Alias == alias).Single();
                        //update the property's ID to the originally generated id
                        contentProperty.Id = originalHiveId.Result;
                    }
                }

                //if it was null it means we can't reconstruct or there's been no property data passed in

            }
        }
    }
}