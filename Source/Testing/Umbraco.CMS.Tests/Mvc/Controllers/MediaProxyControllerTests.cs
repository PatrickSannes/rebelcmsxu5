using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Mvc.Controllers.BackOffice;
using Umbraco.Framework;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.SerializationTypes;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.IO;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Cms.Editors;
using Umbraco.Tests.Cms.Stubs;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.Cms.Mvc.Controllers
{
    [TestFixture]
    public class MediaProxyControllerTests
    {
        private IBackOfficeRequestContext _backOfficeRequestContext;

        [Test]
        [Category(TestOwner.CmsBackOffice.Default)]
        public void MediaProxyController_Proxy_Returns_File_Content()
        {
            //Arrange
            var controller = new MediaProxyController(_backOfficeRequestContext);
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), _backOfficeRequestContext, false);

            var result = controller.Proxy("umbracoFile", "0A647849-BF5C-413B-9420-7AB4C9521505", 0, "test.jpg");
            
            //Assert
            Assert.IsTrue(result is FileContentResult);
            Assert.IsTrue(Encoding.UTF8.GetString(((FileContentResult)result).FileContents) == "test");
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.Default)]
        public void MediaProxyController_Proxy_Returns_File_Content_For_Thumbnail()
        {
            //Arrange
            var controller = new MediaProxyController(_backOfficeRequestContext);
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), _backOfficeRequestContext, false);

            var result = controller.Proxy("umbracoFile", "0A647849-BF5C-413B-9420-7AB4C9521505", 100, "test.jpg");

            //Assert
            Assert.IsTrue(result is FileContentResult);
            Assert.IsTrue(Encoding.UTF8.GetString(((FileContentResult)result).FileContents) == "test_100");
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.Default)]
        public void MediaProxyController_Proxy_Returns_404_For_Empty_Id()
        {
            //Arrange
            var controller = new MediaProxyController(_backOfficeRequestContext);
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), _backOfficeRequestContext, false);

            var result = controller.Proxy("umbracoFile", "", 0, "test.jpg");

            //Assert
            Assert.IsTrue(result is HttpNotFoundResult);
        }

        [Test]
        [Category(TestOwner.CmsBackOffice.Default)]
        public void MediaProxyController_Proxy_Returns_404_For_Invalid_Property_Alias()
        {
            //Arrange
            var controller = new MediaProxyController(_backOfficeRequestContext);
            controller.InjectDependencies(new Dictionary<string, string>(), new Dictionary<string, string>(), _backOfficeRequestContext, false);

            var result = controller.Proxy("invalidPropertyAlias", "0A647849-BF5C-413B-9420-7AB4C9521505", 0, "test.jpg");

            //Assert
            Assert.IsTrue(result is HttpNotFoundResult);
        }

        /// <summary>
        /// initialize all tests, this puts required data into Hive
        /// </summary>
        [SetUp]
        public void Initialize()
        {
            #region Vars

            IReadonlyEntityRepositoryGroup<IContentStore> readonlyContentStoreRepository;
            IReadonlySchemaRepositoryGroup<IContentStore> readonlyContentStoreSchemaRepository;
            IEntityRepositoryGroup<IContentStore> contentStoreRepository;
            ISchemaRepositoryGroup<IContentStore> contentStoreSchemaRepository;

            IReadonlyEntityRepositoryGroup<IFileStore> readonlyFileStoreRepository;
            IReadonlySchemaRepositoryGroup<IFileStore> readonlyFileStoreSchemaRepository;
            IEntityRepositoryGroup<IFileStore> fileStoreRepository;
            ISchemaRepositoryGroup<IFileStore> fileStoreSchemaRepository;

            #endregion

            var hive = MockHiveManager.GetManager()
                .MockContentStore(out readonlyContentStoreRepository, out readonlyContentStoreSchemaRepository, out contentStoreRepository, out contentStoreSchemaRepository)
                .MockFileStore(out readonlyFileStoreRepository, out readonlyFileStoreSchemaRepository, out fileStoreRepository, out fileStoreSchemaRepository);

            //Setup file store
            var fileId = new HiveId("storage", "file-uploader", new HiveIdValue("test.jpg"));
            var file = new File
            {
                Id = fileId,
                Name = "test.jpg",
                ContentBytes = Encoding.UTF8.GetBytes("test")
            };

            readonlyFileStoreRepository
                .Get<File>(true, Arg.Any<HiveId[]>())
                .Returns(new[] { file });

            var thumbnailId = new HiveId("storage", "file-uploader", new HiveIdValue("test_100.jpg"));
            var thumbnail = new File
            {
                Id = thumbnailId,
                Name = "test_100.jpg",
                ContentBytes = Encoding.UTF8.GetBytes("test_100")
            };

            var relation = Substitute.For<IReadonlyRelation<IRelatableEntity, IRelatableEntity>>();
            relation.MetaData.Returns(new RelationMetaDataCollection(new[] { new RelationMetaDatum("size", "100") }));
            relation.Source.Returns(file);
            relation.SourceId.Returns(fileId);
            relation.Destination.Returns(thumbnail);
            relation.DestinationId.Returns(thumbnailId);

            readonlyFileStoreRepository.GetLazyChildRelations(fileId, FixedRelationTypes.ThumbnailRelationType)
                .Returns(new[]{ relation });

            //Setup media store
            var mediaPickerAttributeDefType = new AttributeType { RenderTypeProvider = CorePluginConstants.FileUploadPropertyEditorId };
            var mediaPickerAttributeDef = new AttributeDefinition("umbracoFile", "") { Id = FixedHiveIds.FileUploadAttributeType, AttributeType = mediaPickerAttributeDefType };
            var mediaPickerProperty = new TypedAttribute(mediaPickerAttributeDef, fileId.ToString());

            var mediaId = new HiveId("0A647849-BF5C-413B-9420-7AB4C9521505");
            var mediaEntity = new TypedEntity { Id = mediaId };
            mediaEntity.Attributes.Add(mediaPickerProperty);

            //readonlyContentStoreRepository
            //    .Get<TypedEntity>(true, Arg.Any<HiveId[]>())
            //    .Returns(new[] { mediaEntity });

            //readonlyContentStoreRepository
            //    .SingleOrDefault<TypedEntity>(Arg.Any<Expression<Func<TypedEntity, bool>>>())
            //    .Returns(mediaEntity);

            var mediaEntityList = new List<TypedEntity> { mediaEntity };
            readonlyContentStoreRepository
                .Provider
                .Returns(mediaEntityList.AsQueryable().Provider);

            // Setup application
            var appContext = Substitute.For<IUmbracoApplicationContext>();
            appContext.Hive.Returns(hive);

            // Setup back office request
            _backOfficeRequestContext = Substitute.For<IBackOfficeRequestContext>();
            _backOfficeRequestContext.Application.Returns(appContext);
        }
    }
}
