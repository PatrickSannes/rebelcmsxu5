using System;

namespace Umbraco.Cms.Web.Mvc.ViewEngines
{
    /// <summary>
    /// An attribute for a controller that specifies that the ViewEngine should look for views for this controller using a different controllers name.
    /// This is useful if you want to share views between specific controllers but don't want to have to put all of the views into the Shared folder.
    /// </summary>
    public class AlternateViewEnginePathAttribute : Attribute
    {
        public string AlternateControllerName { get; set; }
        public string AlternateAreaName { get; set; }
        public AlternateViewEnginePathAttribute(string altControllerName)
        {
            AlternateControllerName = altControllerName;
            AlternateAreaName = "";
        }

        public AlternateViewEnginePathAttribute(string altControllerName, string altAreaName)
        {
            AlternateControllerName = altControllerName;
            AlternateAreaName = altAreaName;
        }
    }
}