using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.EmbeddedViewEngine;
using System.Web.Mvc;
using System.Web;
using System.ComponentModel;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;

using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using File = Umbraco.Framework.Persistence.Model.IO.File;
using Umbraco.Hive;

namespace Umbraco.Cms.Web.PropertyEditors.Upload
{
    [EmbeddedView("Umbraco.Cms.Web.PropertyEditors.Upload.Views.UploadEditor.cshtml", "Umbraco.Cms.Web.PropertyEditors")]
    [ModelBinder(typeof(UploadEditorModelBinder))]
    [Bind(Exclude = "ContentId,PropertyAlias,File")]
    public class UploadEditorModel : EditorModel<UploadPreValueModel>, IValidatableObject
    {
        private readonly IBackOfficeRequestContext _backOfficeRequestContext;
        private readonly GroupUnitFactory<IFileStore> _hive;
        private readonly HiveId _contentId;
        private readonly string _propertyAlias;

        private File _file;

        public UploadEditorModel(UploadPreValueModel preValueModel, IBackOfficeRequestContext backOfficeRequestContext, 
            HiveId contentId, string propertyAlias)
            : base(preValueModel)
        {
            _backOfficeRequestContext = backOfficeRequestContext;
            _hive = _backOfficeRequestContext.Application.Hive.GetWriter<IFileStore>(new Uri("storage://file-uploader"));
            _contentId = contentId;
            _propertyAlias = propertyAlias;
        }

        [ScaffoldColumn(false)]
        [ReadOnly(true)]
        public HiveId ContentId
        {
            get { return _contentId;  }
        }

        [ScaffoldColumn(false)]
        [ReadOnly(true)]
        public string PropertyAlias
        {
            get { return _propertyAlias; }
        }

        /// <summary>
        /// The media value
        /// </summary>
        [ScaffoldColumn(false)]
        [ReadOnly(true)]
        public File File
        {
            get
            {
                if (Value.IsNullValueOrEmpty())
                    return null;

                if (_file == null)
                {
                    using (var uow = _hive.Create())
                    {
                        _file = uow.Repositories.Get<File>(Value);
                    }
                }

                return _file;
            }
        }

        /// <summary>
        /// A Guid that is used to track which folder to store the media in, when new media is created this model generates this Id which gets stored in the repository.
        /// </summary>
        public Guid MediaId { get; set; }

        /// <summary>
        /// Gets or sets the new file.
        /// </summary>
        public HttpPostedFileBase NewFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remove file.
        /// </summary>
        [DisplayName("Remove file")]
        public bool RemoveFile { get; set; }

        /// <summary>
        /// The media value
        /// </summary>
        public HiveId Value { get; set; }

        public override void SetModelValues(IDictionary<string, object> serializedVal)
        {
            if (serializedVal.ContainsKey("MediaId"))
            {
                MediaId = Guid.Parse(serializedVal["MediaId"].ToString());
            }
            if (serializedVal.ContainsKey("Value"))
            {
                Value = HiveId.Parse((string) serializedVal["Value"]);
            }
        }

        public override IDictionary<string, object> GetSerializedValue()
        {
            var val = new Dictionary<string, object>();

            //generate an id if we need one
            if (MediaId == Guid.Empty)
            {
                MediaId = Guid.NewGuid();                
            }

            //add the media id to be saved
            val.Add("MediaId", MediaId.ToString("N"));

            // Check to see if we should delete the current file
            // either becuase remove file is checked, or we have a replacement file
            if (RemoveFile || HasFile())
            {
                if (!Value.IsNullValueOrEmpty())
                {
                    // delete entire property folder (deletes image and any thumbnails stored)
                    //var folderHiveId = HiveId.Parse("storage://file-uploader/string/" + MediaId.ToString("N"));
                    var folderHiveId = new HiveId("storage", "file-uploader", new HiveIdValue(MediaId.ToString("N")));

                    using (var uow = _hive.Create())
                    {
                        uow.Repositories.Delete<File>(Value); // Must delete file entity so that relations are deleted
                        uow.Repositories.Delete<File>(folderHiveId);
                        uow.Complete();
                    }
                }
            }

            // If we've received a File from the binding, we need to save it
            if (HasFile())
            {
                // Open a new unit of work to write the file
                using (var uow = _hive.Create())
                {
                    // Create main file
                    var file = new File
                    {
                        RootedPath = MediaId.ToString("N") + "/" + Path.GetFileName(NewFile.FileName).Replace(" ", "")
                    };

                    var stream = NewFile.InputStream;
                    if (stream.CanRead && stream.CanSeek)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        using (var mem = new MemoryStream())
                        {
                            stream.CopyTo(mem);
                            file.ContentBytes = mem.ToArray();
                        }
                    }

                    uow.Repositories.AddOrUpdate(file);

                    // Create thumbnails (TODO: Need to encapsulate this so it can be reused in other places?)
                    if (file.IsImage())
                    {
                        var img = Image.FromFile(file.RootedPath);

                        // Create default thumbnail
                        CreateThumbnail(uow, file, img, MediaId.ToString("N"), 100);

                        // Create additional thumbnails
                        if(!string.IsNullOrEmpty(PreValueModel.Sizes))
                        {
                            var sizes = PreValueModel.Sizes.Split(',');
                            foreach(var size in sizes)
                            {
                                var intSize = 0;
                                if(Int32.TryParse(size, out intSize))
                                {
                                    CreateThumbnail(uow, file, img, MediaId.ToString("N"), intSize);
                                }
                            }
                        }
                    }

                    uow.Complete();

                    val.Add("Value", file.Id);
                }
            }
            else if (!Value.IsNullValueOrEmpty() && !RemoveFile)
            {
                val.Add("Value", Value);
            }
            else
            {
                val.Add("Value", HiveId.Empty);
            }

            return val;
           
        }

        private bool HasFile()
        {
            return NewFile != null && NewFile.ContentLength > 0 && !string.IsNullOrEmpty(NewFile.FileName);
        }

        private void CreateThumbnail(IGroupUnit<IFileStore> uow, File original, Image image, string mediaId, int maxWidthHeight)
        {
            var extension = Path.GetExtension(original.Name).ToLower();
            var thumbFileName = Path.GetFileNameWithoutExtension(original.Name) + "_" + maxWidthHeight + extension;

            // Create file entity
            var thumb = new File
            {
                RootedPath = mediaId + "/" + thumbFileName             
            };

            // Resize image
            var val = (float)image.Width / (float)maxWidthHeight;
            var val2 = (float)image.Height / (float)maxWidthHeight;

            var num = Math.Max(val, val2);

            var num2 = (int)Math.Round((double)((float)image.Width / num));
            var num3 = (int)Math.Round((double)((float)image.Height / num));

            if (num2 == 0)
            {
                num2 = 1;
            }

            if (num3 == 0)
            {
                num3 = 1;
            }

            using(var bitmap = new Bitmap(num2, num3))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var destRect = new Rectangle(0, 0, num2, num3);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

                var imageEncoders = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo encoder = null;
                if (extension == ".png" || extension == ".gif")
                    encoder = imageEncoders.Single(t => t.MimeType.Equals("image/png"));
                else
                    encoder = imageEncoders.Single(t => t.MimeType.Equals("image/jpeg"));

                var stream = new MemoryStream();
                var encoderParameters = new EncoderParameters();
                encoderParameters.Param[0] = new EncoderParameter(global::System.Drawing.Imaging.Encoder.Quality, 90L);
                bitmap.Save(stream, encoder, encoderParameters);

                thumb.ContentBytes = stream.ToArray();
            }

            // Add or update file
            uow.Repositories.AddOrUpdate(thumb);

            // Create relation
            uow.Repositories.AddRelation(original, thumb, FixedRelationTypes.ThumbnailRelationType, 0, new RelationMetaDatum("size", maxWidthHeight.ToString()));
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreValueModel.IsRequired && Value.IsNullValueOrEmpty() && !HasFile())
            {
                yield return new ValidationResult("Value is required", new[] { "Value" });
            }
        }
    }
}
