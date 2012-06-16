
using Umbraco.Framework.Context;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.Persistence.ProviderSupport;

namespace Umbraco.Framework.IO
{
    public class DataContext : AbstractDataContext
    {
        private readonly IFrameworkContext _frameworkContext;
        private readonly string _supportedExtensions;
        private readonly string _rootPath;
        private readonly string _applicationRelativeRoot;
        private readonly string _excludeExtensions;

        public DataContext(IHiveProvider hiveProvider, string supportedExtensions, string rootPath, string applicationRelativeRoot, string excludeExtensions)
            : base(hiveProvider)
        {
            _frameworkContext = hiveProvider.FrameworkContext;
            _supportedExtensions = supportedExtensions;
            _rootPath = rootPath;
            _applicationRelativeRoot = applicationRelativeRoot;
            _excludeExtensions = excludeExtensions;
        }

        public string ApplicationRelativeRoot
        {
            get { return _applicationRelativeRoot; }
        }

        public string RootPath
        {
            get { return _rootPath; }
        }

        public string SupportedExtensions
        {
            get { return _supportedExtensions; }
        }

        protected override void DisposeResources()
        {
            return;
        }

        public IFrameworkContext FrameworkContext
        {
            get { return _frameworkContext; }
        }

        public override ITransaction BeginTransaction()
        {
            return _currentTransaction = new Transaction();
        }

        private ITransaction _currentTransaction;
        public override ITransaction CurrentTransaction
        {
            get { return _currentTransaction; }
        }

        public override void Flush()
        {
            return;
        }
    }
}
