using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    public class InternationalizeAttribute : ActionFilterAttribute
    {
        private IUmbracoApplicationContext _applicationContext;
        public IUmbracoApplicationContext ApplicationContext
        {
            get { return _applicationContext ?? (_applicationContext = DependencyResolver.Current.GetService<IUmbracoApplicationContext>()); }
            set { _applicationContext = value; }
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var model = filterContext.ActionParameters.SingleOrDefault(x => x.Key == "model").Value as IUmbracoRenderModel;
            if (model == null)
                return;

            var currentNodeId = model.CurrentNode.Id;

            //TODO: Cache this

            using (var uow = ApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var ancestorIds = uow.Repositories.GetAncestorsIdsOrSelf(currentNodeId, FixedRelationTypes.DefaultRelationType);
                foreach (var ancestorId in ancestorIds)
                {
                    var languageRelation =
                        uow.Repositories.GetParentRelations(ancestorId, FixedRelationTypes.LanguageRelationType).
                            SingleOrDefault();

                    if (languageRelation == null) 
                        continue;

                    var isoCode = languageRelation.MetaData.SingleOrDefault(x => x.Key == "IsoCode").Value;

                    Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(isoCode);
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(isoCode);

                    return;
                }
            }
        }
    }
}
