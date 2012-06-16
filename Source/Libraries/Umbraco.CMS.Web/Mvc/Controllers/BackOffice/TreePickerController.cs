using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Mvc.Controllers.BackOffice
{
    public class TreePickerController : Controller
    {
        private readonly IRoutableRequestContext _requestContext;

        public TreePickerController(IRoutableRequestContext requestContext)
        {
            _requestContext = requestContext;
        }

        /// <summary>
        /// Renders a tree picker
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult Index(TreePickerRenderModel model)
        {
            if(model != null && model.SelectedValue != null && !model.SelectedValue.Value.IsNullValueOrEmpty() && string.IsNullOrWhiteSpace(model.SelectedText))
            {
                model.SelectedText = "Unknown";

                var treeMetaData = _requestContext.RegisteredComponents
                    .TreeControllers
                    .Where(x => x.Metadata.Id == model.TreeControllerId)
                    .SingleOrDefault();

                if(treeMetaData != null && model.SelectedValue.Value.Value == model.TreeVirtualRootId.Value)
                {
                    model.SelectedText = treeMetaData.Metadata.TreeTitle;
                }
                else
                {
                    var hive = _requestContext.Application.Hive.GetReader<IContentStore>(model.SelectedValue.Value.ToUri());
                    if (hive != null)
                    {
                        using (var uow = hive.CreateReadonly())
                        {
                            var entity = uow.Repositories.Get<TypedEntity>(model.SelectedValue.Value);
                            if (entity != null)
                            {
                                var nameAttr = entity.GetAttributeValueAsString(NodeNameAttributeDefinition.AliasValue, "Name");
                                    // TODO: Can't guarantee attribute is "name"?)
                                if (!string.IsNullOrEmpty(nameAttr))
                                    model.SelectedText = Server.UrlEncode(nameAttr).Replace("+", "%20");
                            }
                        }
                    }
                }
            }

            return PartialView("TreePickerPartial", model);
        }
    }
}
