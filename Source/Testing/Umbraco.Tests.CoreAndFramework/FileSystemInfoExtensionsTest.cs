using System.Linq;
using System;
using System.IO;
using NUnit.Framework;
using Umbraco.Framework;

namespace Umbraco.Tests.CoreAndFramework
{
    
    
    [TestFixture]
    public class FileSystemInfoExtensionsTest
    {
        private string TestPath { get; set; }
        private DirectoryInfo TempDir { get; set; }

        #region test management
        [SetUp]
        public void InitializaTempPath()
        {
            TestPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            TempDir = Directory.CreateDirectory(TestPath);
        }

        [TearDown]
        public void RemoveTempPath()
        {
            Directory.Delete(TestPath, true);
        }
        #endregion test management


        /// <summary>
        ///A test for EnumerateFileSystemInfosRecursive
        ///</summary>
        [Test]
        public void EnumerateFileSystemInfosRecursiveTest()
        {

            ///create test data
            DirectoryInfo rootDir = new DirectoryInfo(TestPath);

            //create x files and y folders for each level, z folders deep
            int levels = 3;
            int files = 2;
            int folders = 3;
            for (int i = 0; i < levels; i++)
            {
                DirectoryInfo curDir = rootDir.CreateSubdirectory("level" + i.ToString());
                //folders
                for (int j = 0; j < folders; j++)
                {
                    curDir.CreateSubdirectory("f" + j.ToString());
                }
                //files
                for (int k = 0; k < files; k++)
                {
                    using (StreamWriter fileStream =new StreamWriter(Path.Combine(curDir.FullName,"file" + k.ToString())))
                    {
                         fileStream.Write("test file");
                    }
                }
                rootDir = curDir;
            }


            ///test based on created data
            //expect an item for each directory and file
            int expectedCount = (levels * (files + folders)) + levels;

            int actualCount = TempDir.EnumerateFileSystemInfosRecursive().Count();

            Assert.AreEqual(expectedCount, actualCount, "Received enumeration count was not expected");
        }

        /// <summary>
        ///A test for RelativePathOfItem
        ///</summary>
        [Test]
        public void RelativePathOfItemTest()
        {
            ///create test data
            string relativeDirPath = String.Concat("this",Path.DirectorySeparatorChar,"is",Path.DirectorySeparatorChar,"a",Path.DirectorySeparatorChar,"relative",Path.DirectorySeparatorChar,"dir",Path.DirectorySeparatorChar,"path");
            string fileName = "testfile.txt";

            FileInfo testFile = new FileInfo(Path.Combine(TestPath, relativeDirPath, fileName));
            
            TempDir.CreateSubdirectory(relativeDirPath);
            //file within relative dir
            using (StreamWriter fileStream = new StreamWriter(testFile.Create()))
            {
                fileStream.Write("test file");
            }

            ///test, based on created data
            string expected = relativeDirPath + Path.DirectorySeparatorChar + fileName;
            string actual = TempDir.RelativePathOfItem(testFile);
            Assert.AreEqual(expected, actual);
            
        }
    }
}
