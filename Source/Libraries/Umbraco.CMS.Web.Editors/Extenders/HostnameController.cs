using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Mvc.ViewEngines;
using Umbraco.Framework;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Security;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Editors.Extenders
{
    /// <summary>
    /// Used for the hostname dialog
    /// </summary>
    public class HostnameController : ContentControllerExtenderBase
    {
        public HostnameController(IBackOfficeRequestContext backOfficeRequestContext)
            : base(backOfficeRequestContext)
        {
        }

        [HttpGet]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Hostnames })] 
        public virtual ActionResult Hostname(HiveId? id)
        {
            if (id.IsNullValueOrEmpty())
                return HttpNotFound();

            using (var uow = Hive.Create<IContentStore>())
            {
                //get the typed/content entity for which to assign hostnames
                var entity = uow.Repositories.Get<TypedEntity>(id.Value);
                if (entity == null)
                    throw new ArgumentException("Could not find entity with id " + id);

                //get the assigned hostnames
                var assignedHostnames = uow.Repositories.GetEntityByRelationType<Hostname>(FixedRelationTypes.HostnameRelationType, id.Value);

                //get the hostname relations (so there's only one query)
                var hostnameRelations = uow.Repositories.GetChildRelations(id.Value, FixedRelationTypes.HostnameRelationType).ToArray();

                return View(new HostnamesModel
                {
                    Id = id.Value,
                    VirtualDirectory = HttpContext.Request.ApplicationPath.TrimEnd('/'),
                    AssignedHostnames = assignedHostnames
                        .Select(x =>
                        {
                            //BUG: This should never be null but currently the call to GetRelations is doing some weird caching!
                            var sortOrder = hostnameRelations.Where(r => r.DestinationId == x.Id).SingleOrDefault();
                            var h = new HostnameEntryModel
                            {
                                Id = x.Id,
                                Hostname = x.Attribute<string>(HostnameSchema.HostnameAlias),
                                SortOrder = sortOrder == null ? 0 : sortOrder.Ordinal
                            };
                            return h;
                        })
                        .ToList()
                });
            }
        }

        [ActionName("Hostname")]
        [HttpPost]
        //[UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Hostnames })] 
        public virtual JsonResult HostnameForm(HostnamesModel model)
        {
            Mandate.ParameterNotEmpty(model.Id, "Id");

            //need to validate the hostnames, first need to remove the invalid required field of the main host name field as this is only required for client side validation
            ModelState.RemoveAll(x => x.Key == "NewHostname");
            foreach (var h in model.AssignedHostnames)
            {
                if (!Regex.IsMatch(h.Hostname, @"^([\w-\.:]+)$", RegexOptions.IgnoreCase))
                {
                    ModelState.AddDataValidationError(string.Format("{0} is an invalid host name", h));
                }
                else if (h.Hostname.Contains(":") && !Regex.IsMatch(h.Hostname.Split(':')[1], @"^\d+$"))
                {
                    ModelState.AddDataValidationError(string.Format("{0} is an invalid port number", h.Hostname.Split(':')[1]));
                }
                //check if the hostname already exists in the collection
                if (BackOfficeRequestContext.RoutingEngine.DomainList.ContainsHostname(h.Hostname))
                {
                    //check if that hostname is assigned to a different node
                    if (BackOfficeRequestContext.RoutingEngine.DomainList[h.Hostname].ContentId != model.Id)
                    {
                        ModelState.AddDataValidationError(string.Format("{0} is already assigned to node id {1}", h.Hostname, BackOfficeRequestContext.RoutingEngine.DomainList[h.Hostname].ContentId));
                    }
                }
                if (model.AssignedHostnames.Count(x => x.Hostname == h.Hostname) > 1)
                {
                    ModelState.AddDataValidationError(string.Format("{0} is a duplicate entry in the submitted list", h));
                }
                if (!ModelState.IsValid)
                    return ModelState.ToJsonErrors();
            }

            using (var uow = Hive.Create<IContentStore>())
            {
                //get the content entity for the hostname assignment
                var entity = uow.Repositories.Get<TypedEntity>(model.Id);
                if (entity == null)
                    throw new NullReferenceException("Could not find entity with id " + model.Id);

                //map the hostname entities from the model
                var hostnames = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<IEnumerable<Hostname>>(model);
                //need to remove the hostnames that no longer exist
                var assignedHostnames = uow.Repositories.GetEntityByRelationType<Hostname>(FixedRelationTypes.HostnameRelationType, model.Id);
                foreach (var a in assignedHostnames.Where(x => !hostnames.Select(h => h.Id).Contains(x.Id)))
                {
                    uow.Repositories.Delete<Hostname>(a.Id);
                }

                uow.Repositories.AddOrUpdate(hostnames);
                uow.Complete();

                //clears the domain cache
                BackOfficeRequestContext.RoutingEngine.ClearCache(clearDomains: true, clearGeneratedUrls: true);

                var successMsg = "Hostname.Success.Message".Localize(this, new
                {
                    Count = model.AssignedHostnames.Count,
                    Name = entity.GetAttributeValue(NodeNameAttributeDefinition.AliasValue, "Name")
                });
                Notifications.Add(new NotificationMessage(successMsg, "Hostname.Title".Localize(this), NotificationType.Success));

                return new CustomJsonResult(new
                {
                    success = true,
                    notifications = Notifications,
                    msg = successMsg
                }.ToJsonString);
            }

        }

    }
}
