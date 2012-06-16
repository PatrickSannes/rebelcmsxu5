using System.IO;
using System.Reflection;

using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Extensions
{
    public class StubIORepositoryReadWriter : RepositoryReadWriter
    {
        private readonly string _rootFolder;

        private StubIORepositoryReadWriter(DataContext dataContext)
            : base(dataContext)
        {
            _rootFolder = dataContext.RootPath;
        }

        public string RootFolder
        {
            get { return _rootFolder; }
        }

        public static StubIORepositoryReadWriter CreateRepositoryReadWriter(string supportedExtensions)
        {
            var frameworkContext = new FakeFrameworkContext();
            var hiveProvider = new HiveReadWriteProvider(new HiveProviderSetup(frameworkContext, "rw-test-provider", null, null, null, null));

            var dir = new DirectoryInfo(TestHelper.AssemblyDirectory);
            //var dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            //dir = new DirectoryInfo(Path.Combine(dir.FullName, @"..\..\"));
            var dataContext = new DataContext(hiveProvider, supportedExtensions, dir.FullName, string.Empty, string.Empty);

            return new StubIORepositoryReadWriter(dataContext);
        }
    }
}