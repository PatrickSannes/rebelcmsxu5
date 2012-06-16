using System;
using System.Linq;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using System.Web;
using File = Umbraco.Framework.Persistence.Model.IO.File;
using Image = System.Drawing.Image;
using System.IO;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Umbraco.Framework.Persistence.Model.Associations;

namespace Umbraco.Cms.Web.Mvc.Controllers.BackOffice
{
    /// <summary>
    /// Controller to return and secure Media files
    /// </summary>
    public class MediaController : BackOfficeController
    {
        public MediaController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        /// <summary>
        /// Uploads the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="docTypeId">The doc type id.</param>
        /// <param name="parentId">The parent id.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ContentResult Upload(HttpPostedFileBase file, HiveId? docTypeId, HiveId? parentId, string name)
        {
            
            GroupUnitFactory Hive = BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("media://"));

            using (var uow = Hive.Create<IContentStore>())
            {
                var schema = uow.Repositories.Schemas.Get<EntitySchema>(docTypeId.Value);
                if (schema == null)
                    throw new ArgumentException(string.Format("No schema found for id: {0} on action Create", docTypeId));

                //create the new content item
                var contentViewModel = CreateNewContentEntity(schema, name, parentId.Value);
                contentViewModel.UtcPublishedDate = DateTimeOffset.UtcNow;

                var entity = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<Revision<TypedEntity>>(contentViewModel);

                var toPublish = entity.CopyToNewRevision(FixedStatusTypes.Published);


                var fileID = StoreFile(file);

                //need to get alias of upload prop
                var docType =
                   BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(schema);


                string uploadAlias = docType.Properties.Where(p => p.DataType.PropertyEditor.Id.ToString().ToUpper() == CorePluginConstants.FileUploadPropertyEditorId).First().Alias;

                toPublish.Item.Attributes[uploadAlias].Values["Value"] = fileID;


                uow.Repositories.Revisions.AddOrUpdate(toPublish);
                uow.Complete();

                //need to return json as a string, since otherwise we'll be promted to download a file 

                string json = new
                {
                    mediaId = toPublish.Item.Id,
                    title = name
                }.ToJsonString();

                return Content(json);
            }


        }

        /// <summary>
        /// Stores the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        private HiveId StoreFile(HttpPostedFileBase file)
        {
             //saving file
            var hive = BackOfficeRequestContext.Application.Hive.GetWriter<IFileStore>(new Uri("storage://file-uploader"));

            //create new media item
            //currently code is in here but should be shared with upload prop editor

            var mediaId = Guid.NewGuid();   

            // Open a new unit of work to write the file
            using (var uow = hive.Create())
            {
                // Create main file
                var f = new File
                {
                    RootedPath = mediaId.ToString("N") + "/" + file.FileName.Replace(" ", "")
                };
                //.ToString("N")
                var stream = file.InputStream;
                if (stream.CanRead && stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var mem = new MemoryStream())
                    {
                        stream.CopyTo(mem);
                        f.ContentBytes = mem.ToArray();
                    }
                }

                uow.Repositories.AddOrUpdate(f);

                //Create thumbnails (TODO: Need to encapsulate this so it can be reused in other places?)
                if (f.IsImage())
                {
                    var img = Image.FromFile(f.RootedPath);

                    // Create default thumbnail
                    CreateThumbnail(uow, f, img, mediaId.ToString("N"), 100);
                    //// Create additional thumbnails

                }

                uow.Complete();

                return f.Id;
            }
        }

        /// <summary>
        /// Creates the thumbnail.
        /// </summary>
        /// <param name="uow">The uow.</param>
        /// <param name="original">The original.</param>
        /// <param name="image">The image.</param>
        /// <param name="mediaId">The media id.</param>
        /// <param name="maxWidthHeight">Height of the max width.</param>
        private void CreateThumbnail(IGroupUnit<IFileStore> uow, File original, Image image, string mediaId, int maxWidthHeight)
        {
            var extension = Path.GetExtension(original.Name).ToLower(); ;
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

            using (var bitmap = new Bitmap(num2, num3))
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

        /// <summary>
        /// Creates the new content entity.
        /// </summary>
        /// <param name="docTypeData">The doc type data.</param>
        /// <param name="name">The name.</param>
        /// <param name="parentId">The parent id.</param>
        /// <returns></returns>
        private MediaEditorModel CreateNewContentEntity(EntitySchema docTypeData, string name, HiveId parentId)
        {
            Mandate.ParameterNotNull(docTypeData, "docTypeData");
            Mandate.ParameterNotEmpty(parentId, "parentId");

            //get doc type model
            var docType = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<EntitySchema, DocumentTypeEditorModel>(docTypeData);
            //map (create) content model from doc type model
            var contentModel = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<DocumentTypeEditorModel, MediaEditorModel>(docType);
            contentModel.ParentId = parentId;
            contentModel.Name = name;
            return contentModel;
        }
    }
}
