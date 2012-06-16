using System;
using System.Web.Mvc;
using System.Web.Mvc.Resources;
using System.Web.Routing;
using System.Web.Script.Serialization;

namespace Umbraco.Cms.Web.Mvc
{
    /// <summary>
    /// Allows for a custom JsonSerializer to serialize the result
    /// </summary>
    public class CustomJsonResult : JsonResult
    {
        public readonly Func<string> OutputJson;

        public CustomJsonResult(Func<string> outputJson)
        {
            OutputJson = outputJson;
            Data = null;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            //this will set everything up for us but because data is null, it wont write out the json, so we'll 
            //write it out with our own results
            base.ExecuteResult(context);
            //write the results
            if (OutputJson != null)
            {
                context.HttpContext.Response.Write(OutputJson());
            }
        }

        /// <summary>
        /// Hide the data member so it cannot be set publicly
        /// </summary>
        private new object Data { get; set; }
    }
}