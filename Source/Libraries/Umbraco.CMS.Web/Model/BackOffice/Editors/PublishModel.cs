using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// Model representing the publish dialog
    /// </summary>
    [Bind(Exclude = "Name")]
    public class PublishModel : DialogModel
    {
        public HiveId Id { get; set; }
        
        [ReadOnly(true)]
        public string Name { get; set; }

        public bool IncludeChildren { get; set; }

        public bool IncludeUnpublishedChildren { get; set; }

    }
}