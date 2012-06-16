using System;
using System.ComponentModel;
using NUnit.Framework;
using Umbraco.Framework;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class HiveIdValueTypeConverterTests
    {

        [TestCase]
        public void Converts_From_Guid()
        {
            //Arrange

            var guid = Guid.NewGuid();
            var id = new HiveIdValue(guid);
            var converter = TypeDescriptor.GetConverter(typeof(HiveIdValue));

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

            var id = new HiveIdValue(1234);
            var converter = TypeDescriptor.GetConverter(typeof(HiveIdValue));

            //Act


            //Assert

            Assert.IsTrue(converter.CanConvertFrom(typeof(int)));
            Assert.IsTrue(converter.CanConvertTo(typeof(string)));
            Assert.AreEqual(1234, converter.ConvertTo(id, typeof(int)));
            Assert.AreEqual(id, converter.ConvertFrom(1234));
        }

        [TestCase]
        public void Converts_From_String()
        {
            //Arrange

            var rawId = new HiveIdValue("My-Test-Template.cshtml");
            var encodedId = rawId.ToString();
            var converter = TypeDescriptor.GetConverter(typeof(HiveIdValue));

            //Act


            //Assert

            Assert.IsTrue(converter.CanConvertFrom(typeof(string)));
            Assert.IsTrue(converter.CanConvertTo(typeof(string)));
            Assert.AreEqual(encodedId, converter.ConvertTo(rawId, typeof(string)));
            Assert.AreEqual(rawId, converter.ConvertFrom(encodedId));
        }


    }
}