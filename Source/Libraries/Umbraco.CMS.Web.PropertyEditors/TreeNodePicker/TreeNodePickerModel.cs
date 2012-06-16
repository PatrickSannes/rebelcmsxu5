using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web.PropertyEditors.TreeNodePicker
{
    [Bind(Exclude = "TreeMetadata,TreeVirtualRootId")]
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.TreeNodePicker.Views.TreeNodePicker.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    public class TreeNodePickerModel : EditorModel<TreeNodePickerPreValueModel>, IValidatableObject
    {
        private readonly IEnumerable<Lazy<TreeController, TreeMetadata>> _trees;

        public TreeNodePickerModel(TreeNodePickerPreValueModel preValueModel, IEnumerable<Lazy<TreeController, TreeMetadata>> trees)
            : base(preValueModel)
        {
            _trees = trees;
        }

        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public TreeMetadata TreeMetadata 
        { 
            get
            {
                var tree = _trees.SingleOrDefault(x => x.Metadata.Id == PreValueModel.TreeId);
                return (tree != null)? tree.Metadata : null;
            }
        }

        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public HiveId TreeVirtualRootId
        {
            get
            {
                if (TreeMetadata.Id == new Guid(CorePluginConstants.ContentTreeControllerId))
                    return FixedHiveIds.ContentVirtualRoot;

                if (TreeMetadata.Id == new Guid(CorePluginConstants.MediaTreeControllerId))
                    return FixedHiveIds.MediaVirtualRoot;

                return HiveId.Empty;
            }
        }

        public HiveId Value { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(PreValueModel.IsRequired && Value.IsNullValueOrEmpty())
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
