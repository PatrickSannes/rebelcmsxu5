using System;
using System.Configuration;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Umbraco.Framework.Configuration;
using Umbraco.Framework.ProviderSupport;
using Umbraco.Framework.Testing.PartialTrust;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework.Config
{
    [TestFixture]
    public class DeepConfigManagerTests : AbstractPartialTrustFixture<DeepConfigManagerTests>
    {
        [Test]
        public void DeepConfigManager_Serialize_Provider_Config_Section()
        {
            //Arrange

            var file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "web.config"));
            var config = DeepConfigManager.CreateNewConfigFile(file, true);
            var section = new TestConfigSection {Address = "address1", Name = "name1", Age = 123};

            //Act

            DeepConfigManager.SerializeProviderConfigSection(config, section, "umbraco/persistenceProviderSettings/nhibernate", true);

            //Assert

            Assert.AreEqual("configuration", config.Root.Name.LocalName);
            Assert.AreEqual("configSections", config.Root.Elements().First().Name.LocalName);

            Assert.AreEqual("sectionGroup", config.Root.Elements().First().Elements().First().Name.LocalName);
            Assert.AreEqual("name", config.Root.Elements().First().Elements().First().Attributes().First().Name.LocalName);
            Assert.AreEqual("umbraco", config.Root.Elements().First().Elements().First().Attributes().First().Value);

            Assert.AreEqual("sectionGroup", config.Root.Elements().First().Elements().First().Elements().First().Name.LocalName);
            Assert.AreEqual("name", config.Root.Elements().First().Elements().First().Elements().First().Attributes().First().Name.LocalName);
            Assert.AreEqual("persistenceProviderSettings", config.Root.Elements().First().Elements().First().Elements().First().Attributes().First().Value);

            Assert.AreEqual("section", config.Root.Elements().First().Elements().First().Elements().First().Elements().First().Name.LocalName);
            Assert.AreEqual("name", config.Root.Elements().First().Elements().First().Elements().First().Elements().First().Attributes().First().Name.LocalName);
            Assert.AreEqual("nhibernate", config.Root.Elements().First().Elements().First().Elements().First().Elements().First().Attributes().First().Value);
            Assert.AreEqual("requirePermission", config.Root.Elements().First().Elements().First().Elements().First().Elements().First().Attributes().Last().Name.LocalName);
            Assert.AreEqual(section.GetType().AssemblyQualifiedName, config.Root.Descendants("section").Where(x => x.Attributes("type").Any()).FirstOrDefault().Attribute("type").Value);

            Assert.AreEqual("umbraco", config.Root.Elements().Last().Name.LocalName);
            Assert.AreEqual("persistenceProviderSettings", config.Root.Elements().Last().Elements().First().Name.LocalName);
            Assert.AreEqual("nhibernate", config.Root.Elements().Last().Elements().First().Elements().First().Name.LocalName);

            Assert.AreEqual("name1", config.Root.Elements().Last().Elements().First().Elements().First().Attribute("name").Value);
            Assert.AreEqual("address1", config.Root.Elements().Last().Elements().First().Elements().First().Attribute("address").Value);
            Assert.AreEqual(123, (int)config.Root.Elements().Last().Elements().First().Elements().First().Attribute("age"));
        }

        private class TestConfigSection : AbstractProviderConfigurationSection
        {
            [ConfigurationProperty("name", IsRequired = true)]
            public string Name
            {
                get
                {
                    return (string)this["name"];
                }
                set
                {
                    this["name"] = value;
                }
            }

            [ConfigurationProperty("address", IsRequired = true)]
            public string Address
            {
                get
                {
                    return (string)this["address"];
                }
                set
                {
                    this["address"] = value;
                }
            }

            [ConfigurationProperty("age", IsRequired = true)]
            public int Age
            {
                get
                {
                    return (int)this["age"];
                }
                set
                {
                    this["age"] = value;
                }
            }
        }

        /// <summary>
        /// Run once before each test in derived test fixtures.
        /// </summary>
        public override void TestSetup()
        {
            return;
        }

        /// <summary>
        /// Run once after each test in derived test fixtures.
        /// </summary>
        public override void TestTearDown()
        {
            return;
        }
    }
}
