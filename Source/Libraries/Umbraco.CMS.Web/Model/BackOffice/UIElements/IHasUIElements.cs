using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Cms.Web.Model.BackOffice.UIElements
{
    public interface IHasUIElements
    {
        IList<UIElement> UIElements { get; }
    }
}
