using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Umbraco.Cms.Web.Mvc
{
    public class HierarchicalSelectListItem : SelectListItem
    {
        public string[] ParentValues { get; set; }
    }
}
