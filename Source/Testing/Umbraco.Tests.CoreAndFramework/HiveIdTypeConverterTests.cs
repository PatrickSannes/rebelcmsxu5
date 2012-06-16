using System;
using System.ComponentModel;
using NUnit.Framework;
using Umbraco.Framework;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class HiveIdTypeConverterTests
    {

        [TestCase]
        public void Converts_From_Guid()
        {
            //Arrange

            var guid = Guid.NewGuid();
            var id = new HiveId(guid);
            var converter = TypeDescriptor.GetConverter(typeof(HiveId));

            //Act


            //Assert

            Assert.IsTrue(converter.CanConvertFrom(typeof(Guid)));
            Assert.IsTrue(converter.CanConvertTo(typeof(string)));
            Assert.AreEqual(guid, converter.ConvertTo(id, typeof(Guid)));
            Assert.AreEqual(id, converter.ConvertFrom(guid));

        }

        [TestCase]
        public void Converts_From_Int()
        {
            //Arrange

            var id = new HiveId(1234);
            var converter = TypeDescriptor.GetConverter(typeof(HiveId));
            var encodedId = id.ToString();

            //Assert

            Assert.IsTrue(converter.CanConvertFrom(typeof(int)));
            Assert.IsTrue(converter.CanConvertTo(typeof(string)));
            Assert.AreEqual(1234, converter.ConvertTo(id, typeof(int)));
            Assert.IsTrue(id.Equals(converter.ConvertFrom(1234)));
            Assert.IsTrue(id.Equals(converter.ConvertFrom(encodedId)));
            Assert.AreEqual(id, converter.ConvertFrom(1234));
            Assert.AreEqual(id, converter.ConvertFrom(encodedId));
        }

        [TestCase]
        public void Converts_From_String()
        {
            //Arrange
            var rawId = new HiveId("My-Test-Template.cshtml");
            var encodedId = rawId.ToString();
            var converter = TypeDescriptor.GetConverter(typeof(HiveId));

            //Act


            //Assert

            Assert.IsTrue(converter.CanConvertFrom(typeof(string)));
            Assert.IsTrue(converter.CanConvertTo(typeof(string)));
            Assert.AreEqual(encodedId, converter.ConvertTo(rawId, typeof(string)));
            Assert.IsTrue(rawId.Equals(converter.ConvertFrom(encodedId)));
            Assert.AreEqual(rawId, converter.ConvertFrom(encodedId));
        }

        [TestCase]
        public void Converts_From_HiveId()
        {
            //Arrange
            var rawId = new HiveId("test-id");
            var converter = TypeDescriptor.GetConverter(typeof(HiveId));

            //Act


            //Assert

            Assert.IsTrue(converter.CanConvertFrom(typeof(HiveId)));
            Assert.IsTrue(converter.CanConvertTo(typeof(HiveId)));
            Assert.AreEqual(rawId, converter.ConvertTo(rawId, typeof(HiveId)));
            Assert.IsTrue(rawId.Equals(converter.ConvertFrom(rawId)));
            Assert.AreEqual(rawId, converter.ConvertFrom(rawId));
        }

    }
}