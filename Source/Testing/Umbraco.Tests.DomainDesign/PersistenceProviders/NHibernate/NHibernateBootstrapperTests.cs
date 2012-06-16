using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Umbraco.Framework.Configuration;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Persistence.NHibernate;
using Umbraco.Framework.Persistence.NHibernate.Config;
using Umbraco.Framework.Persistence.NHibernate.DependencyDemandBuilders;
using Umbraco.Framework.Persistence.ProviderSupport;
using Umbraco.Framework.ProviderSupport;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.DomainDesign.PersistenceProviders.NHibernate
{
    [TestClass]
    public class NHibernateBootstrapperTests
    {
      
        [TestMethod]
        public void NHibernateBootstrapper_Requires_Configuration()
        {
            var boot = new ProviderBootstrapper(null, null, new FakeFrameworkContext());

            var status = boot.GetInstallStatus();

            Assert.AreEqual(InstallStatusType.RequiresConfiguration, status.StatusType);
        }

        [TestMethod]
        public void NHibernateBootstrapper_Pending()
        {
            var builder = new NHibernateConfigBuilder("data source=:memory:", "unit-tester", SupportedNHDrivers.SqlLite, "thread_static", false);
            var config = builder.BuildConfiguration();
            var boot = new ProviderBootstrapper(config, new ProviderConfigurationSection()
                {
                    ConnectionStringKey = "data source=:memory:",
                    Driver = SupportedNHDrivers.SqlLite,
                    SessionContext = "thread_static"
                }, new FakeFrameworkContext());
            
            var status = boot.GetInstallStatus();

            Assert.AreEqual(InstallStatusType.Pending, status.StatusType);
        }

        [TestMethod]
        public void NHibernateBootstrapper_Completed()
        {
            var builder = new NHibernateConfigBuilder("data source=:memory:", "unit-tester", SupportedNHDrivers.SqlLite, "thread_static", false);
            var config = builder.BuildConfiguration();
            var boot = new ProviderBootstrapper(config, new ProviderConfigurationSection()
            {
                ConnectionStringKey = "data source=:memory:",
                Driver = SupportedNHDrivers.SqlLite,
                SessionContext = "thread_static"
            }, new FakeFrameworkContext());

            var status = boot.TryInstall();

            Assert.AreEqual(InstallStatusType.Completed, status.StatusType);
        }

        [TestMethod]
        public void NHibernateBootstrapper_Tried_And_Failed()
        {

            var localConfig = new ProviderConfigurationSection()
                {
                    ConnectionStringKey = "This is an invalid conn string",
                    Driver = SupportedNHDrivers.MsSql2008,
                    SessionContext = "web"
                };
            var builder = new NHibernateConfigBuilder(localConfig.ConnectionStringKey, "test", localConfig.Driver, "thread_static", false);
            var config = builder.BuildConfiguration();
            var boot = new ProviderBootstrapper(config, localConfig, new FakeFrameworkContext());

            var status = boot.TryInstall();

            Assert.AreEqual(InstallStatusType.TriedAndFailed, status.StatusType);
        }

        [TestMethod]
        public void NHibernateBootstrapper_Configure_Application()
        {
            //Arrange

            //create a new web.config file in /plugins for the providers to write to
            var http = new FakeHttpContextFactory("~/test");
            var configFile = new FileInfo(Path.Combine(Environment.CurrentDirectory, "nhibernate.config"));
            var configMgr = new DeepConfigManager(http.HttpContext);
            var configXml = DeepConfigManager.CreateNewConfigFile(configFile, true);
            var installModel = new DatabaseInstallModel()
                {
                    DatabaseType = DatabaseServerType.MSSQL,
                    DatabaseName = "test",
                    Server = "testserver",
                    Username = "testuser",
                    Password = "testpass"
                };
            var boot = new ProviderBootstrapper(null, null, new FakeFrameworkContext());


            //Act

            boot.ConfigureApplication("rw-test", "test", configXml, new BendyObject(installModel));

            //Assert

            Assert.AreEqual(@"<configuration>
  <configSections>
    <sectionGroup name=""umbraco"">
      <sectionGroup name=""persistenceProviderSettings"">
        <section name=""test"" type=""Umbraco.Framework.Persistence.NHibernate.Config.ProviderConfigurationSection, Umbraco.Framework.Persistence.NHibernate, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" />
      </sectionGroup>
    </sectionGroup>
  </configSections>
  <connectionStrings>
    <add name=""test.ConnString"" connectionString=""Data Source=testserver; Initial Catalog=test;User Id=testuser;Password=testpass"" providerName=""System.Data.SqlClient"" />
  </connectionStrings>
  <appSettings />
  <umbraco>
    <persistenceProviderSettings>
      <test connectionStringKey=""test.ConnString"" sessionContext=""web"" driver=""MsSql2008"" />
    </persistenceProviderSettings>
  </umbraco>
</configuration>", configXml.ToString());

        }

        private class DatabaseInstallModel
        {
            public DatabaseServerType DatabaseType { get; set; }
            //public string ConnectionString { get; set; }
            //public string ProviderName { get; set; }
            public string Server { get; set; }
            public string DatabaseName { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private enum DatabaseServerType
        {
            MSSQL, MySQL, SQLCE, Custom
        }
    }
}
