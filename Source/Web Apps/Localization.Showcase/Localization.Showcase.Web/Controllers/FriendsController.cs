using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Localization.Showcase.Web.Models;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Localization.Configuration;
using System.Threading;
using System.Globalization;

namespace Localization.Showcase.Web.Controllers
{
    public class FriendsController : Controller
    {
        TextManager _textManager;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            var toggleParam = requestContext.HttpContext.Request.QueryString["custom-languages"];
            if (toggleParam != null)
            {
                CustomLanguageLogic.Toggle(toggleParam == "true");
            }
            CustomLanguageLogic.TryChangeLanguage(requestContext.RouteData.Values["language"] + "");
            //Try set the language from route parameters
            try
            {                
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(requestContext.RouteData.Values["language"] + "");
            }
            catch { }            
        }

        public FriendsController()
        {
            //Assume that this was injected
            _textManager = LocalizationConfig.TextManager;
        }

        //
        // GET: /Friends/

        public ActionResult Index(FriendsChooseModel model)            
        {            
            model.Friends = Friend.All;
            if (model.ChosenFriends == null)
            {
                model.ChosenFriends = new Dictionary<int, bool>();
                ModelState.Clear();
            }
            else
            {
                //Postback (yeah, this is a demo project...)

                var chosenFriends = model.ChosenFriends.Where(x => x.Value).Select(x=>x.Key).ToList();

                int missing = FriendSettings.RequiredFriends - chosenFriends.Count;
                if (missing > 0)
                {
                    ModelState.AddModelError("", _textManager.Get("RequiredFriendsError", new { Missing = missing }, encode: false));
                }
                else
                {
                    model.AddedFriends = chosenFriends.Select(id=>model.Friends.First(x=>x.ID == id)).ToList(); 
                }
            }
                                  

            return View(model);
        }

    }    
}
