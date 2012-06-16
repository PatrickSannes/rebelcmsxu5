using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.Model.BackOffice.UIElements;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    /// <summary>
    /// A model representing moving a node
    /// </summary>
    [Bind(Exclude = "TreeRenderModel,FromName")]
    public class MoveModel : DialogModel
    {
        [HiddenInput(DisplayValue = false)]
        public HiveId SelectedItemId { get; set; }

        [ReadOnly(true)]
        public string SelectedItemName { get; set; }

        [HiddenInput(DisplayValue = false)]
        public HiveId ToId { get; set; }

        [ScaffoldColumn(false)]
        [ReadOnly(true)]
        public TreeRenderModel TreeRenderModel { get; set; }
    }
}