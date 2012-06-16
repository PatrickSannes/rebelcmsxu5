using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// An abstract View Model for Editors
    /// </summary>
    [Bind(Exclude = "NoticeBoard,UIElements")]
    public abstract class EditorModel : CoreEntityModel, IHasUIElements
    {
        protected EditorModel() 
        {
            NoticeBoard = new List<NotificationMessage>();
            UIElements = new List<UIElement>();
        }

        /// <summary>
        /// Used to display important messages about the node
        /// </summary>
        [ReadOnly(true)]
        [ScaffoldColumn((false))]
        [UIHint("NoticeBoard")]
        public IList<NotificationMessage> NoticeBoard { get; protected set; }

        /// <summary>
        /// List of toolbar buttons / elements to display for this model
        /// </summary>
        /// <value>
        /// The UI elements.
        /// </value>
        [ReadOnly(true)]
        [ScaffoldColumn((false))]
        public IList<UIElement> UIElements { get; set; }
    }
}
