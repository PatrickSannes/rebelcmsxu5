using System;
using System.Linq;
using System.Xml.Linq;
using Examine;
using Examine.Providers;
using Umbraco.Framework.Context;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Persistence.Examine.Config;
using Umbraco.Framework.ProviderSupport;
using Umbraco.Hive;

namespace Umbraco.Framework.Persistence.Examine.Hive
{
    public class ProviderBootstrapper : AbstractProviderBootstrapper
    {
        private readonly ProviderConfigurationSection _existingConfig;
        private readonly ExamineManager _examineManager;
        private readonly IFrameworkContext _frameworkContext;
        private readonly InstallStatus _installStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderBootstrapper"/> class if insufficient configuration information is yet available.
        /// </summary>
        /// <param name="installStatus">The install status.</param>
        /// <remarks></remarks>
        public ProviderBootstrapper(InstallStatus installStatus)
        {
            _installStatus = installStatus;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderBootstrapper"/> class if sufficient configuration information has been supplied by the user.
        /// </summary>
        /// <param name="existingConfig">The existing config.</param>
        /// <param name="examineManager"></param>
        /// <param name="frameworkContext"></param>
        /// <remarks></remarks>
        public ProviderBootstrapper(ProviderConfigurationSection existingConfig, ExamineManager examineManager, IFrameworkContext frameworkContext)
        {
            _existingConfig = existingConfig;
            _examineManager = examineManager;
            _frameworkContext = frameworkContext;

            //bind to all of the Examine events
            foreach (var i in _examineManager.IndexProviderCollection.OfType<BaseIndexProvider>())
            {
                i.IndexingError += (sender, e) => LogHelper.Error<Exception>("[Examine] " + e.Message, e.InnerException);
                i.IndexDeleted += (sender, e) => LogHelper.TraceIfEnabled<ExamineManager>("[Examine] Item {0} has been removed from the index", () => e.DeletedTerm.Value);
                i.NodeIndexed += (sender, e) => LogHelper.TraceIfEnabled<ExamineManager>("[Examine] Item {0} has been indexed", () => e.Item.Id);
            }
        }

        public override void ConfigureApplication(string providerKey, XDocument configXml, BendyObject installParams)
        {
            //TODO: Setup config
        }

        public override InstallStatus GetInstallStatus()
        {
            if (_installStatus != null) return _installStatus;
            if (_existingConfig == null) return new InstallStatus(InstallStatusType.RequiresConfiguration);
            
            var systemRoot = _examineManager.Search(
                _examineManager.CreateSearchCriteria().Must().HiveId(new HiveId("ExamineInstalled".EncodeAsGuid()))
                    .Compile());
            return !systemRoot.Any() 
                ? new InstallStatus(InstallStatusType.Pending) 
                : new InstallStatus(InstallStatusType.Completed);
        }

        public override InstallStatus TryInstall()
        {
            
            _examineManager.PerformIndexing(new IndexOperation()
                {
                    Item = new IndexItem()
                        {
                            Id = "ExamineInstalled".EncodeAsGuid().ToString("N"),
                            ItemCategory = "InstallRecord"
                        },
                    Operation = IndexOperationType.Add 
                });

            return new InstallStatus(InstallStatusType.Completed);
        }
    }
}