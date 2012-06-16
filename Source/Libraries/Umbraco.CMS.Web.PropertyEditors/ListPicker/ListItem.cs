using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice;

namespace Umbraco.Cms.Web.PropertyEditors.ListPicker
{
    public class ListItem
    {
        [Required]
        [ShowLabel(false)]
        public string Id { get; set; }

        [Required]
        [ShowLabel(false)]
        public string Value { get; set; }
    }
}
