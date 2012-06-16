using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Cms.Web.Model.BackOffice.UIElements
{
    [UIElement("B93A1441-FF27-4E7D-9257-FA1F1AD32BB9", "Umbraco.UI.UIElements.ButtonUIElement")]
    public class DialogSaveButtonUIElement : SaveButtonUIElement
    {
        public DialogSaveButtonUIElement()
        {
            AdditionalData.Add("data-bind", "visible: !success()");
        }
    }
}
