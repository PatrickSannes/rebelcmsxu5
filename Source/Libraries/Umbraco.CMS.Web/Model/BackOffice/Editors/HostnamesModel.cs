using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// A view model representing hostnames assigned to an entity
    /// </summary>
    [Bind(Include = "Id,AssignedHostnames")]
    public class HostnamesModel : DialogModel
    {
        public HostnamesModel()
        {
            AssignedHostnames = new List<HostnameEntryModel>();
        }

        /// <summary>
        /// The ID of the content node to assign hostnames
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public HiveId Id { get; set; }

        [Required]
        [ReadOnly(true)]
        public string NewHostname { get; set; }

        /// <summary>
        /// The current application path
        /// </summary>
        [ReadOnly(true)]
        public string VirtualDirectory { get; set; }

        public IList<HostnameEntryModel> AssignedHostnames { get; set; }
    }
}