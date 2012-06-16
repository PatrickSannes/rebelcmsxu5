using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Framework;

using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Tests.Extensions;
using File = Umbraco.Framework.Persistence.Model.IO.File;

namespace Umbraco.Tests.Framework.IO
{
    [TestClass]
    public class RepositoryReadWriterTests
    {

        [TestMethod]
        public void RepositoryReadWriterTests_GenerateId()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            //Act
            var id1 = repo.GenerateId(dir.GetFiles().First().FullName);
            var id2 = repo.GenerateId(dir.FullName);

            //Assert
            Assert.AreEqual("storage$empty_root$$_p__test-provider$_v__string$_$!0bada205d2e54eee824112464227ada4", id1.ToString());
            Assert.AreEqual("storage$empty_root$$_p__test-provider$_v__string$_$$", id2.ToString());
        }

        [TestMethod]
        public void RepositoryReadWriterTests_GetAll_FindsOnlySpecificExtensions()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            //Act
            var files = repo.GetEntities<File>();

            //Assert
            var count = files.Count();
            Assert.AreEqual(dir.GetFiles(searchPattern, SearchOption.TopDirectoryOnly).OfType<FileSystemInfo>().Concat(dir.GetDirectories()).Count(), count);
            Assert.IsTrue(dir.GetFiles("*.*", SearchOption.TopDirectoryOnly).OfType<FileSystemInfo>().Concat(dir.GetDirectories()).Count() >= count);
        }

        [TestMethod]
        public void RepositoryReadWriterTests_GetEntityOfFile_ReturnsFoundEntity()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            //Act
            var existingFile = dir.GetFiles(searchPattern, SearchOption.AllDirectories).First();
            var file =
                repo.GetEntity<File>(repo.GenerateId(existingFile.FullName));

            //Assert
            Assert.IsNotNull(file);
            Assert.AreEqual(existingFile.Name, file.Name);
            Assert.AreEqual(existingFile.LastWriteTime, file.UtcModified);
            Assert.AreEqual(existingFile.FullName, file.Location);
            Assert.IsFalse(file.IsContainer);

        }

        [TestMethod]
        public void RepositoryReadWriterTests_GetEntityOfDirectory_ReturnsFoundEntity()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            //Act
            var directoryInfo = dir.GetDirectories("*.*", SearchOption.TopDirectoryOnly).First();
            var file =
                repo.GetEntity<File>(repo.GenerateId(directoryInfo.FullName));

            //Assert
            Assert.IsNotNull(file);
            Assert.AreEqual(directoryInfo.Name, file.Name);
            Assert.AreEqual(directoryInfo.LastWriteTime, file.UtcModified);
            Assert.AreEqual(directoryInfo.FullName, file.Location);
            Assert.IsTrue(file.IsContainer);

        }

        [TestMethod]
        public void RepositoryReadWriterTests_ContainerWithChildren_WillRepresentAsRelations()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            //Act
            var directoryInfo = dir.GetDirectories("*.*", SearchOption.TopDirectoryOnly)
                .Where(x => x.GetDirectories().Any())
                .First();

            var file =
                repo.GetEntity<File>(repo.GenerateId(directoryInfo.FullName));

            //Assert
            var children = directoryInfo.GetDirectories();
            Assert.AreEqual(children.Count(), file.Relations.ChildrenAsFile().Count());
            var relatableEntity = file.Relations.First().Destination;
            Assert.IsInstanceOfType(relatableEntity, typeof (File));

            Assert.AreEqual(children.First().Name, ((File) relatableEntity).Name);
        }

        [TestMethod]
        public void RepositoryReadWriterTests_DeeplySpecifiedId_WillBeResolvedFromId()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            var actualFile = dir.GetFiles(searchPattern, SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(repo.RootFolder, string.Empty).Contains(@"\"))
                .Last();

            //Act
            var file =
                repo.GetEntity<File>(repo.GenerateId(actualFile.FullName));

            //Assert
            Assert.IsNotNull(file);
            Assert.AreEqual(actualFile.Name, file.Name);
        }

        [TestMethod]
        public void RepositoryReadWriterTests_CreatedNonContainerEntity()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            var contents = "Test File Content";
            var file = CreateFile(contents);
            
            //Act

            repo.AddOrUpdate(file);

            //Assert
            Assert.IsNotNull(file.Id);
            var physicalFiles = dir.GetFiles(file.Name);
            Assert.AreEqual(1, physicalFiles.Count());

            var physicalFile = physicalFiles[0];
            using (var reader = physicalFile.OpenText())
            {
                Assert.AreEqual(contents, reader.ReadToEnd());
            }
        }

        [TestMethod]
        public void RepositoryReadWriterTests_UpdateNonContainerEntity()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            var contents = "Test File Content";
            var file = CreateFile(contents);

            repo.AddOrUpdate(file);

            //Act
            file = repo.GetEntity<File>(file.Id);
            contents = "Updated file content";
            file.ContentBytes = Encoding.Default.GetBytes(contents);
            repo.AddOrUpdate(file);

            //Assert
            Assert.IsNotNull(file.Id);
            var physicalFiles = dir.GetFiles(file.Name);
            Assert.AreEqual(1, physicalFiles.Count());

            var physicalFile = physicalFiles[0];
            using (var reader = physicalFile.OpenText())
            {
                Assert.AreEqual(contents, reader.ReadToEnd());
            }
        }

        [TestMethod]
        public void RepositoryReadWriterTests_DeleteNonContainerFile()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            var contents = "Test File Content";
            var file = CreateFile(contents);

            repo.AddOrUpdate(file);

            Assert.IsTrue(System.IO.File.Exists(file.Location));

            //Act
            repo.Delete<File>(file.Id);
            
            //Assert
            Assert.IsFalse(System.IO.File.Exists(file.Location));
        }

        [TestMethod]
        public void RepositoryReadWriterTests_DeepEntity_CanWalkBackThroughParents()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            var actualFile = dir.GetFiles(searchPattern, SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(repo.RootFolder, string.Empty).Contains(@"\"))
                .Last();

            //Act
            var file =
                repo.GetEntity<File>(repo.GenerateId(actualFile.FullName));

            //Assert
            Assert.IsNotNull(file.Relations.ParentAsFile());
        }

        [TestMethod]
        public void RepositoryReadWriterTests_DeepEntity_CanWalkBackThroughAncestors()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            var actualFile = dir.GetFiles(searchPattern, SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(repo.RootFolder, string.Empty).Contains(@"\"))
                .Last();

            //Act
            var file = repo.GetEntity<File>(repo.GenerateId(actualFile.FullName));

            //Assert
            var ancestors = file.Relations.AncestorsAsFile().ToArray();
            Assert.AreEqual(1, ancestors.Count());
        }

        [TestMethod]
        public void RepositoryReadWriterTests_DeepEntity_CanWalkBackThroughAncestorsOrSelf()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            var actualFile = dir.GetFiles(searchPattern, SearchOption.AllDirectories)
                .Where(x => x.FullName.Replace(repo.RootFolder, string.Empty).Contains(@"\"))
                .Last();

            //Act
            var file = repo.GetEntity<File>(repo.GenerateId(actualFile.FullName));

            //Assert
            var ancestors = file.Relations.AncestorsOrSelfAsFile().ToArray();
            Assert.AreEqual(2, ancestors.Count());
        }

        [TestMethod]
        public void RepositoryReadWriterTests_RootLevelEntity_Parent_Is_Root_Container()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            //Act
            var existingFile = dir.GetDirectories().First();
            var file =
                repo.GetEntity<File>(repo.GenerateId(existingFile.FullName));

            //Assert
            var parent = file.Relations.ParentAsFile();
            Assert.IsTrue(parent.IsContainer);
            Assert.AreEqual(parent.Location, repo.RootFolder);
        }

        [TestMethod]
        public void RepositoryReadWriterTests_CreateContainerEntity()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            //Act
            var file = CreateFile(string.Empty);
            repo.AddOrUpdate(file);

            //Assert
            Assert.IsNotNull(file.Id);
            Assert.IsTrue(file.IsContainer);
            Assert.IsTrue(Directory.Exists(file.Location));
        }

        [TestMethod]
        public void RepositoryReadWriterTests_DeleteContainerEntity()
        {
            //Arrange
            const string searchPattern = "*.dll";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            var file = CreateFile(string.Empty);

            repo.AddOrUpdate(file);

            Assert.IsTrue(System.IO.Directory.Exists(file.Location));

            //Act
            repo.Delete<File>(file.Id);

            //Assert
            Assert.IsFalse(System.IO.Directory.Exists(file.Location));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RepositoryReadWriterTests_CannotSetContentOnContainerEntity()
        {
            //Arrange
            var file = CreateFile(string.Empty);
            //Act

            //Assert
            Assert.IsTrue(file.IsContainer);

            file.ContentBytes = new byte[0];
        }

        [TestMethod]
        public void RepositoryReadWriterTests_CreateEntityWithRelations_CreatesRelationFiles()
        {
            //Arrange
            const string searchPattern = "*.dll,*.xml";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            var sourceFile = CreateFile("Source File");
            var destinationFile = CreateFile("Destination File");
            destinationFile.Relations.Add(new Relation(new RelationType("TestRelation"), sourceFile, destinationFile, new[] {
                new RelationMetaDatum("size", "100")
            }));

            repo.AddOrUpdate(sourceFile);
            repo.AddOrUpdate(destinationFile);

            var sourceMd5Hash = sourceFile.Id.ToString().ToMd5();
            var destinationMd5Hash = destinationFile.Id.ToString().ToMd5();
            var relationPath = Path.Combine(repo.RootFolder, "Relations\\"+ sourceMd5Hash + "-" + destinationMd5Hash + ".xml");

            Assert.IsTrue(System.IO.File.Exists(relationPath));
        }

        [TestMethod]
        public void RepositoryReadWriterTests_DeleteEntityWithRelations_DeletesRelationFiles()
        {
            //Arrange
            const string searchPattern = "*.dll,*.xml";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            var file1 = CreateFile("File1");
            var file2 = CreateFile("File2");
            file2.Relations.Add(new Relation(new RelationType("TestRelation"), file1, file2, new[] {
                new RelationMetaDatum("size", "100")
            })); 
            var file3 = CreateFile("File3");
            file3.Relations.Add(new Relation(new RelationType("TestRelation"), file3, file1, new[] {
                new RelationMetaDatum("size", "200")
            }));

            repo.AddOrUpdate(file1);
            repo.AddOrUpdate(file2);
            repo.AddOrUpdate(file3);

            var file1Md5 = file1.Id.ToString().ToMd5();
            var fileSearchPattern = "*" + file1Md5 + "*.xml";
            var relationsDir = Path.Combine(repo.RootFolder, "Relations\\");

            Assert.IsTrue(System.IO.Directory.GetFiles(relationsDir, fileSearchPattern).Count() == 2);

            repo.Delete<File>(file1.Id);

            Assert.IsTrue(System.IO.Directory.GetFiles(relationsDir, fileSearchPattern).Count() == 0);
        }

        [TestMethod]
        public void RepositoryReadWriterTests_GetEntityWithRelations_ReturnsWithRelations()
        {
            //Arrange
            const string searchPattern = "*.dll,*.xml";

            var repo = StubIORepositoryReadWriter.CreateRepositoryReadWriter(searchPattern);
            var dir = new DirectoryInfo(repo.RootFolder);

            var relationType = new RelationType("TestRelation");

            var file1 = CreateFile("File1");
            var file2 = CreateFile("File2");
            file2.Relations.Add(new Relation(relationType, file1, file2, new[] {
                new RelationMetaDatum("size", "100")
            }));

            repo.AddOrUpdate(file1);
            repo.AddOrUpdate(file2);

            var lookedUpFile = repo.GetEntity<File>(file1.Id);
            var childRelations = lookedUpFile.Relations.Children<File>(relationType);

            Assert.IsTrue(childRelations.Count() == 1);

            var file4 = repo.GetEntity<File>(file2.Id);
            var relations2 = file4.Relations.Parent<File>(relationType);

            Assert.IsTrue(relations2 != null);
            Assert.IsTrue(relations2.Id.ToString(HiveIdFormatStyle.AsUri) == file1.Id.ToString(HiveIdFormatStyle.AsUri));
        }

        private static File CreateFile(string contents)
        {
            var file = new File
            {
                Name = Guid.NewGuid().ToString("N"),
            };

            if (!string.IsNullOrEmpty(contents))
                file.ContentBytes = Encoding.Default.GetBytes(contents);
            else
                file.IsContainer = true;

            return file;
        }
    }
}
