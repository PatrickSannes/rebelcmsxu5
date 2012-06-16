using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Umbraco.Cms.Web.Model.BackOffice;

namespace Umbraco.Cms.Web.Trees
{
    /// <summary>
    /// A specialized JSON result for ISearchableTree
    /// </summary>
    public class TreeSearchJsonResult : JsonResult
    {
        private readonly IEnumerable<SearchResultItem> _results;

        public TreeSearchJsonResult(IEnumerable<SearchResultItem> results)
        {
            _results = results;
            Data = null;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            //this will set everything up for us but because data is null, it wont write out the json, so we'll 
            //write it out with our own results
            base.ExecuteResult(context);
            //write the results
            if (_results != null)
            {
                var serializer = new JavaScriptSerializer();
                context.HttpContext.Response.Write(serializer.Serialize(_results));
            }
        }

        /// <summary>
        /// Hide the data member so it cannot be set publicly
        /// </summary>
        private new object Data { get; set; }
    }
}