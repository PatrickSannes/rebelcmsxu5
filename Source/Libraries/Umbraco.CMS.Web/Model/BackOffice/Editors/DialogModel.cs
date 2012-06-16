using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    [Bind(Exclude = "UIElements")]
    public abstract class DialogModel : IHasUIElements
    {
        /// <summary>
        /// List of toolbar buttons / elements to display for this model
        /// </summary>
        /// <value>
        /// The UI elements.
        /// </value>
        [ReadOnly(true)]
        [ScaffoldColumn((false))]
        public IList<UIElement> UIElements
        {
            get { return new List<UIElement> { new DialogSaveButtonUIElement() }; }
        }
    }
}
