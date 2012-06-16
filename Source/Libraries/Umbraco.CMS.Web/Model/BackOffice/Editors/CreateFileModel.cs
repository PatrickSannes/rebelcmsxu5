using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Framework;

namespace Umbraco.Cms.Web.Model.BackOffice.Editors
{
    [Bind(Exclude = "AvailableFileExtensions,FileThumbnail,FolderThumbnail,AvailableStubs")]
    public class CreateFileModel : IValidatableObject
    {
        [Required]
        public string Name { get; set; }

        [HiddenInput(DisplayValue = false)]
        public HiveId ParentId { get; set; }
        
        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public string CurrentFolderPath { get; set; }

        public string FileExtension { get; set; }

        [DisplayName("Template")]
        public HiveId? Stub { get; set; }

        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public string[] AvailableFileExtensions { get; set; }

        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public IEnumerable<SelectListItem> AvailableStubs { get; set; }

        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public string FolderThumbnail { get; set; }

        [ReadOnly(true)]
        [ScaffoldColumn(false)]
        public string FileThumbnail { get; set; }

        public CreateFileType CreateType { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CreateType == CreateFileType.File && string.IsNullOrEmpty(FileExtension))
            {
                yield return new ValidationResult("File extension is required", new[] { "FileExtension" });
            }
        }
    }
}