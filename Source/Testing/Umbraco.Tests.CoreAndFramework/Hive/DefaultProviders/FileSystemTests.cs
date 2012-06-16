using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Tests.Extensions;
using File = Umbraco.Framework.Persistence.Model.IO.File;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders
{
    using Umbraco.Framework.Data;

    [TestFixture]
    public class FileSystemTests
    {
        [TestCase]
        public void Session_GetAll_FindsOnlySpecificExtensions()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            //Act
            var files = setup.EntityRepository.GetAll<File>();

            //Assert
            var count = files.Count();
            Assert.AreEqual(setup.TestDirectory.GetFiles(setup.Settings.SupportedExtensions, SearchOption.TopDirectoryOnly).OfType<FileSystemInfo>().Concat(setup.TestDirectory.GetDirectories()).Count(), count);
            Assert.IsTrue(setup.TestDirectory.GetFiles("*.*", SearchOption.TopDirectoryOnly).OfType<FileSystemInfo>().Concat(setup.TestDirectory.GetDirectories()).Count() >= count);
        }

        [TestCase]
        public void Session_GetEntityOfFile_ReturnsFoundEntity()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            //Act
            var existingFile = setup.TestDirectory.GetFiles(setup.Settings.SupportedExtensions, SearchOption.AllDirectories).First();
            var file = setup.EntityRepository.Get<File>(setup.EntityRepository.GenerateId(existingFile.FullName));

            //Assert
            Assert.IsNotNull(file);
            Assert.AreEqual(existingFile.Name, file.Name);
            Assert.IsTrue(existingFile.LastWriteTime == file.UtcModified);
            Assert.AreEqual(existingFile.FullName, file.RootedPath);
            Assert.IsFalse(file.IsContainer);

        }

        [TestCase]
        public void Session_GetEntityOfDirectory_ReturnsFoundEntity()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            //Act
            var directoryInfo = setup.TestDirectory.GetDirectories("*.*", SearchOption.TopDirectoryOnly).First();
            var file = setup.EntityRepository.Get<File>(setup.EntityRepository.GenerateId(directoryInfo.FullName));

            //Assert
            Assert.IsNotNull(file);
            Assert.AreEqual(directoryInfo.Name, file.Name);
            Assert.IsTrue(directoryInfo.LastWriteTime == file.UtcModified);
            Assert.AreEqual(directoryInfo.FullName, file.RootedPath);
            Assert.IsTrue(file.IsContainer);

        }

        [TestCase]
        public void Session_ContainerWithChildren_WillRepresentAsRelations()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            //Act
            var directoryInfo = setup.TestDirectory.GetDirectories("*.*", SearchOption.TopDirectoryOnly)
                .Where(x => x.GetDirectories().Any())
                .First();

            var file = setup.EntityRepository.Get<File>(new HiveId(directoryInfo.FullName));

            //Assert
            var children = directoryInfo.GetDirectories();
            Assert.IsTrue(children.Count() > 0);

            var childrenViaProxy = file.RelationProxies.AllChildRelations();
            Assert.AreEqual(children.Count(), childrenViaProxy.Count());

            var firstChildRelationViaProxy = childrenViaProxy.First();
            // Source & Destination should be null because we're not going via a EntityRepositoryGroup which can lazy-load relation entities
            Assert.IsNull(firstChildRelationViaProxy.Item.Source);
            Assert.IsNull(firstChildRelationViaProxy.Item.Destination);

            var loadDestination = setup.EntityRepository.Get<File>(firstChildRelationViaProxy.Item.DestinationId);
            Assert.IsNotNull(loadDestination);
            Assert.That(loadDestination, Is.InstanceOf<File>());

            Assert.AreEqual(children.First().Name, ((File)loadDestination).Name);
        }

        [TestCase]
        public void Session_DeeplySpecifiedId_WillBeResolvedFromId()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            var actualFile = setup.TestDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(setup.Settings.AbsoluteRootedPath, string.Empty).Contains(@"\"))
                .Last();

            //Act
            var file = setup.EntityRepository.Get<File>(setup.EntityRepository.GenerateId(actualFile.FullName));

            //Assert
            Assert.IsNotNull(file);
            Assert.AreEqual(actualFile.Name, file.Name);
        }

        [TestCase]
        public void Session_CreatedNonContainerEntity()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();
            var contents = "Test File Content";
            var file = IoHiveTestSetupHelper.CreateFile(contents);

            //Act

            setup.EntityRepository.AddOrUpdate(file);

            //Assert
            Assert.IsNotNull(file.Id);
            var physicalFiles = setup.TestDirectory.GetFiles(file.Name);
            Assert.AreEqual(1, physicalFiles.Count());

            var physicalFile = physicalFiles[0];
            using (var reader = physicalFile.OpenText())
            {
                Assert.AreEqual(contents, reader.ReadToEnd());
            }
        }

        [TestCase]
        public void Session_UpdateNonContainerEntity()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();
            var contents = "Test File Content";
            var file = IoHiveTestSetupHelper.CreateFile(contents);

            setup.EntityRepository.AddOrUpdate(file);

            //Act
            file = setup.EntityRepository.Get<File>(file.Id);
            contents = "Updated file content";
            file.ContentBytes = Encoding.Default.GetBytes(contents);
            setup.EntityRepository.AddOrUpdate(file);

            //Assert
            Assert.IsNotNull(file.Id);
            var physicalFiles = setup.TestDirectory.GetFiles(file.Name);
            Assert.AreEqual(1, physicalFiles.Count());

            var physicalFile = physicalFiles[0];
            using (var reader = physicalFile.OpenText())
            {
                Assert.AreEqual(contents, reader.ReadToEnd());
            }
        }

        [TestCase]
        public void Session_DeleteNonContainerFile()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();
            var contents = "Test File Content";
            var file = IoHiveTestSetupHelper.CreateFile(contents);

            setup.EntityRepository.AddOrUpdate(file);

            Assert.IsTrue(System.IO.File.Exists(file.RootedPath));

            //Act
            setup.EntityRepository.Delete<File>(file.Id);

            //Assert
            Assert.IsFalse(System.IO.File.Exists(file.RootedPath));
        }

        [TestCase]
        public void Session_GetCurrentFolder_ReturnsCorrectDirectory()
        {
            // Arrange
            var setup = new IoHiveTestSetupHelper();

            var someFile = setup.TestDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(setup.Settings.AbsoluteRootedPath, string.Empty).Contains(@"\"))
                .Last();

            // Act
            var file = setup.EntityRepository.Get<File>(setup.EntityRepository.GenerateId(someFile.FullName));

            // Assert
            Assert.AreEqual(Path.GetDirectoryName(someFile.FullName) + @"\", setup.EntityRepository.GetContainingFolder(file));
        }

        [TestCase]
        public void Session_GetParent_ReturnsContainer()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            var someFile = setup.TestDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(setup.Settings.AbsoluteRootedPath, string.Empty).Contains(@"\"))
                .Last();
            var someFileInfo = new FileInfo(someFile.FullName);
            var parentFolder = someFileInfo.Directory;

            //Act
            var fileId = setup.EntityRepository.GenerateId(someFile.FullName);
            var file = setup.EntityRepository.Get<File>(fileId);
            var parents = setup.EntityRepository.GetLazyParentRelations(fileId);

            // Assert
            Assert.AreEqual(1, parents.Count(), "No parent found for " + someFile.FullName + " with id " + fileId.ToString());
            Assert.AreEqual(parentFolder.FullName.NormaliseDirectoryPath(), ((File)parents.First().Source).RootedPath.NormaliseDirectoryPath());
        }

        [TestCase]
        public void Session_GetRelations_VerboseOverload_CanObtainParents_DefaultRelationType()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            var actualFile = setup.TestDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(setup.Settings.AbsoluteRootedPath, string.Empty).Contains(@"\"))
                .Last();
            var parentFolder = actualFile.Directory;

            //Act
            var file = setup.EntityRepository.Get<File>(setup.EntityRepository.GenerateId(actualFile.FullName));
            var parents = setup.EntityRepository.GetLazyRelations(file, Direction.Parents, FixedRelationTypes.DefaultRelationType);

            //Assert
            // Check for iterator block mistakes
            Assert.AreEqual(parents.Count(), parents.ToList().Count());
            Assert.GreaterOrEqual(1, parents.Count());
            Assert.AreEqual(parentFolder.FullName.NormaliseDirectoryPath(), ((File)parents.First().Source).RootedPath.NormaliseDirectoryPath());
        }

        [TestCase]
        public void SessionExtensions_GetParents_CanObtainParents_DefaultRelationType()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            var actualFile = setup.TestDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(setup.Settings.AbsoluteRootedPath, string.Empty).Contains(@"\"))
                .Last();
            var parentFolder = actualFile.Directory;
            var parent2ndLevelFolder = parentFolder.Parent;

            //Act
            var normalId = setup.EntityRepository.NormaliseId(new HiveId(actualFile.FullName));
            var generatedId = setup.EntityRepository.GenerateId(actualFile.FullName);
            var file = setup.EntityRepository.Get<File>(setup.EntityRepository.GenerateId(actualFile.FullName));
            var parents = setup.EntityRepository.GetLazyParentRelations(file.Id, FixedRelationTypes.DefaultRelationType);
            var firstParent = ((File)parents.First().Source);
            var secondParents = setup.EntityRepository.GetLazyParentRelations(firstParent.Id, FixedRelationTypes.DefaultRelationType);
            var secondParent = ((File)secondParents.First().Source);

            //Assert
            // Check for iterator block mistakes
            Assert.AreEqual(parents.Count(), parents.ToList().Count());
            Assert.AreEqual(normalId, generatedId);
            Assert.That(parents.Count(), Is.GreaterThanOrEqualTo(1));
            Assert.AreEqual(actualFile.FullName.NormaliseDirectoryPath(), file.RootedPath.NormaliseDirectoryPath());
            Assert.AreEqual(parentFolder.FullName.NormaliseDirectoryPath(), firstParent.RootedPath.NormaliseDirectoryPath());
            Assert.AreEqual(parent2ndLevelFolder.FullName.NormaliseDirectoryPath(), secondParent.RootedPath.NormaliseDirectoryPath());
        }

        [TestCase]
        public void SessionExtensions_GetAncestors_CanObtainAllAncestors_DefaultRelationType()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            var actualFile = setup.TestDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(setup.Settings.AbsoluteRootedPath, string.Empty).Contains(@"\"))
                .Last();
            var parentFolder = actualFile.Directory;
            var parent2ndLevelFolder = parentFolder.Parent;

            //Act
            var file = setup.EntityRepository.Get<File>(new HiveId(actualFile.FullName));
            var parents = setup.EntityRepository.GetLazyAncestorRelations(file.Id, FixedRelationTypes.DefaultRelationType);
            var firstParent = ((File)parents.First().Source);
            var secondParent = ((File)(parents.Skip(1).Take(1).FirstOrDefault()).Source);

            //Assert
            // Check for iterator block mistakes
            Assert.AreEqual(parents.Count(), parents.ToList().Count());
            Assert.That(parents.Count(), Is.GreaterThanOrEqualTo(2));
            Assert.AreEqual(actualFile.FullName.NormaliseDirectoryPath(), file.RootedPath.NormaliseDirectoryPath());
            Assert.AreEqual(parentFolder.FullName.NormaliseDirectoryPath(), firstParent.RootedPath.NormaliseDirectoryPath());
            Assert.AreEqual(parent2ndLevelFolder.FullName.NormaliseDirectoryPath(), secondParent.RootedPath.NormaliseDirectoryPath());
        }

        [TestCase]
        public void SessionExtensions_GetAncestorsOrSelf_CanObtainAllAncestors_DefaultRelationType()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            var actualFile = setup.TestDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(setup.Settings.AbsoluteRootedPath, string.Empty).Contains(@"\"))
                .Last();
            var parentFolder = actualFile.Directory;
            var parent2ndLevelFolder = parentFolder.Parent;

            //Act
            var file = setup.EntityRepository.Get<File>(new HiveId(actualFile.FullName));
            var itemsOrSelf = setup.EntityRepository.GetAncestorsOrSelf(file, FixedRelationTypes.DefaultRelationType);

            var firstItemShouldBeSelf = (File)itemsOrSelf.First();
            var firstParent = (File)(itemsOrSelf.Skip(1).Take(1).FirstOrDefault());
            var secondParent = (File)(itemsOrSelf.Skip(2).Take(1).FirstOrDefault());

            //Assert
            // Check for iterator block mistakes
            Assert.AreEqual(itemsOrSelf.Count(), itemsOrSelf.ToList().Count());
            Assert.That(itemsOrSelf.Count(), Is.GreaterThanOrEqualTo(2));
            Assert.That(file.Id, Is.EqualTo(firstItemShouldBeSelf.Id));
            Assert.That(file.RootedPath, Is.EqualTo(firstItemShouldBeSelf.RootedPath));
            Assert.That(file.IsContainer, Is.EqualTo(firstItemShouldBeSelf.IsContainer));
            Assert.That(file.ContentBytes, Is.EqualTo(firstItemShouldBeSelf.ContentBytes));
            Assert.That(file.UtcCreated, Is.EqualTo(firstItemShouldBeSelf.UtcCreated));
            Assert.That(file.UtcModified, Is.EqualTo(firstItemShouldBeSelf.UtcModified));
            Assert.That(file.UtcStatusChanged, Is.EqualTo(firstItemShouldBeSelf.UtcStatusChanged));
            Assert.That(file, Is.EqualTo(firstItemShouldBeSelf));
            Assert.AreEqual(actualFile.FullName.NormaliseDirectoryPath(), file.RootedPath.NormaliseDirectoryPath());
            Assert.AreEqual(parentFolder.FullName.NormaliseDirectoryPath(), firstParent.RootedPath.NormaliseDirectoryPath());
            Assert.AreEqual(parent2ndLevelFolder.FullName.NormaliseDirectoryPath(), secondParent.RootedPath.NormaliseDirectoryPath());
        }

        [TestCase]
        public void Session_GetChildren_CanObtainAllChildren_DefaultRelationType()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();
            var actualFile = setup.TestDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(setup.Settings.AbsoluteRootedPath, string.Empty).Contains(@"\"))
                .Last();
            var parentFolderForScanning = actualFile.Directory;

            // Act
            var parentFolderItem = setup.EntityRepository.Get<File>(new HiveId(parentFolderForScanning.FullName));
            var aChildItem = setup.EntityRepository.Get<File>(new HiveId(actualFile.FullName));
            var childRelations = setup.EntityRepository.GetLazyRelations(parentFolderItem, Direction.Children, FixedRelationTypes.DefaultRelationType);
            var parentShouldAllBeMe = childRelations.Select(x => x.Source).ToList();
            var childItems = childRelations.Select(x => x.Destination).ToList();

            // Assert
            // Check for iterator block mistakes
            Assert.AreEqual(childRelations.Count(), childRelations.ToList().Count());
            Assert.That(childItems.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(parentShouldAllBeMe.Select(x => x.Id), Is.All.EqualTo(parentFolderItem.Id));
            Assert.That(childItems.Select(x => x.Id), Has.Some.EqualTo(aChildItem.Id));
        }

        [TestCase]
        public void Session_GetDescendents_CanObtainAllDescendents_DefaultRelationType()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();
            var actualFile = setup.TestDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(setup.Settings.AbsoluteRootedPath, string.Empty).Contains(@"\"))
                .Last();
            var parentFolderForScanning = actualFile.Directory.Parent;

            // Act
            var parentFolderItem = setup.EntityRepository.Get<File>(new HiveId(parentFolderForScanning.FullName));
            var aChildItem = setup.EntityRepository.Get<File>(new HiveId(actualFile.FullName));
            var childRelations = setup.EntityRepository.GetLazyRelations(parentFolderItem, Direction.Descendents, FixedRelationTypes.DefaultRelationType);
            var childItems = childRelations.Select(x => x.Destination).ToList();

            // Assert
            // Check for iterator block mistakes
            Assert.AreEqual(childRelations.Count(), childRelations.ToList().Count());
            Assert.That(childItems.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(childItems.Select(x => x.Id), Has.Some.EqualTo(aChildItem.Id));
        }

        [TestCase]
        public void Session_CreateContainerEntity()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            //Act
            var file = IoHiveTestSetupHelper.CreateFile(string.Empty);
            setup.EntityRepository.AddOrUpdate(file);

            //Assert
            Assert.IsNotNull(file.Id);
            Assert.IsTrue(file.IsContainer);
            Assert.IsTrue(Directory.Exists(file.RootedPath));
        }

        [TestCase]
        public void Session_DeleteContainerEntity()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            var file = IoHiveTestSetupHelper.CreateFile(string.Empty);

            setup.EntityRepository.AddOrUpdate(file);

            Assert.IsTrue(System.IO.Directory.Exists(file.RootedPath));

            //Act
            setup.EntityRepository.Delete<File>(file.Id);

            //Assert
            Assert.IsFalse(System.IO.Directory.Exists(file.RootedPath));
        }

        [TestCase]
        public void Session_CannotSetContentOnContainerEntity()
        {
            //Arrange
            var file = IoHiveTestSetupHelper.CreateFile(string.Empty);
            //Act

            //Assert
            Assert.IsTrue(file.IsContainer);
            Assert.Throws<InvalidOperationException>(() => file.ContentBytes = new byte[0]);
        }

        [TestCase]
        public void Session_RootLevelEntity_ParentIsNull()
        {
            //Arrange
            var setup = new IoHiveTestSetupHelper();

            //Act
            var existingFile = setup.TestDirectory;
            var file = setup.EntityRepository.Get<File>(new HiveId(existingFile.FullName));

            //Assert
            var relationById = setup.EntityRepository.GetParentRelations(file.Id, FixedRelationTypes.DefaultRelationType).FirstOrDefault();
            Assert.IsNull(relationById);
            var relation = setup.EntityRepository.GetLazyParentRelations(file.Id, FixedRelationTypes.DefaultRelationType).FirstOrDefault();
            Assert.IsNull(relation);
            var relationByIdViaProxy = file.RelationProxies.AllParentRelations();
            Assert.That(relationByIdViaProxy.Count(), Is.EqualTo(0));
        }
    }
}
