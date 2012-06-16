using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Trees
{

    /// <summary>
    /// A collection of TreeNode objects
    /// </summary>
    public class TreeNodeCollection : List<TreeNode>
    {

        /// <summary>
        /// Serializes the collection to JSON in a jsTree format
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            return ToJson().ToString();
        }

        internal JArray ToJson()
        {
            return
                 new JArray(this.Select(x =>
                     {
    
                         //add the json id to the additional data (metadata)
                         if (!x.AdditionalData.ContainsKey("jsonId"))
                         {
                             x.AdditionalData.Add("jsonId", x.HiveId.ToJsonObject());    
                         }
                         
                         //create the standard structure
                         var obj = new JObject(
                             new JProperty("data",
                                 new JObject(
                                     new JProperty("title", x.Title),
                             //HTML attributes to add to the anchor object
                                     new JProperty("attr",
                                         new JObject(
                                             new JProperty("href", x.EditorUrl))),
                             //need to pre-pend a '/' if it is a file, otherwise a class is used
                                     new JProperty("icon",
                                         string.IsNullOrEmpty(x.Icon)
                                             ? "tree-folder" //set default to 'tree-folder' class
                                             : x.Icon))),
                             //Create the metadata for the node which sets the jsonurl/editor path 
                            new JProperty("metadata",
                                 new JObject(
                                     new JProperty("editorPath", x.EditorUrl),
                                     new JProperty("jsonUrl", x.JsonUrl),
                                     new JProperty("nodePath", x.NodePath),
                             //add the additional data to the metadata
                                     x.AdditionalData.Select(custom => new JProperty(custom.Key, custom.Value)).ToArray(),
                                     new JProperty("ctxMenu",
                                         new JArray(x.MenuActions.Select(m => m.Metadata.Id.ToString()))))),
                             //HTML attributes to add to the li object
                             new JProperty("attr",
                                 new JObject(
                                    new JProperty("class", string.Join(" ", x.Style.AppliedClasses)),
                                    new JProperty("id", x.HiveId.GetHtmlId()))));

                         //only add children if there are children
                         if (x.HasChildren)
                         {
                             obj.Add(new JProperty("state", "closed"));
                             obj.Add(new JProperty("children", x.Children == null ? new JArray() : x.Children.ToJson()));
                         }

                         return obj;
                     }));
        }


    }
}
