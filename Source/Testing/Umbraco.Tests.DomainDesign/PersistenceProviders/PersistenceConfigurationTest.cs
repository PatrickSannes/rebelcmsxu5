using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Framework.Persistence.Configuration;

namespace Umbraco.Tests.DomainDesign.PersistenceProviders
{
    [TestClass]
    public class PersistenceConfigurationTest
    {
        
        [TestMethod]
        public void PersistenceConfiguration_Provider_Correctly_Prefixed_And_Alias_Set()
        {
            var config = new PersistenceTypeLoaderElement()
                {
                    Key = "rw-testing"
                };

            Assert.AreEqual("testing", config.Alias);
        }
        
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void PersistenceConfiguration_Provider_Throws_When_Invalid_Prefix()
        {
            var config = new PersistenceTypeLoaderElement()
                {
                    Key = "blah-testing"
                };
            Assert.AreEqual("", config.Alias);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void PersistenceConfiguration_Provider_Throws_When_Invalid_Key_Name()
        {
            var config = new PersistenceTypeLoaderElement()
                {
                    Key = "rw-1"
                };

            Assert.AreEqual("", config.Alias);
        }

    }
}