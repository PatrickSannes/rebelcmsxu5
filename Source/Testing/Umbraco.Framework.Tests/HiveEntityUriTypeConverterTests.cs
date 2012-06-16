using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Framework;

namespace Umbraco.Tests.Framework
{
    [TestClass()]
    public class HiveEntityUriTypeConverterTests
    {

        [TestMethod]
        public void HiveEntityUriTypeConverter_Converts_From_Guid()
        {
            //Arrange

            var guid = Guid.NewGuid();
            var id = new HiveEntityUri(guid);
            var converter = TypeDescriptor.GetConverter(typeof(HiveEntityUri));

            //Act


            //Assert

            Assert.IsTrue(converter.CanConvertFrom(typeof(Guid)));
            Assert.AreEqual(guid, converter.ConvertTo(id, typeof(Guid)));
            Assert.AreEqual(id, converter.ConvertFrom(guid));

        }

        [TestMethod]
        public void HiveEntityUriTypeConverter_Converts_From_Int()
        {
            //Arrange

            var id = new HiveEntityUri(1234);
            var converter = TypeDescriptor.GetConverter(typeof(HiveEntityUri));

            //Act


            //Assert

            Assert.IsTrue(converter.CanConvertFrom(typeof(int)));
            Assert.AreEqual(1234, converter.ConvertTo(id, typeof(int)));
            Assert.AreEqual(id, converter.ConvertFrom(1234));
        }

        [TestMethod]
        public void HiveEntityUriTypeConverter_Converts_From_String()
        {
            //Arrange

            var rawId = new HiveEntityUri("My-Test-Template.cshtml");
            var encodedId = rawId.ToString();
            var converter = TypeDescriptor.GetConverter(typeof(HiveEntityUri));

            //Act


            //Assert

            Assert.IsTrue(converter.CanConvertFrom(typeof(string)));
            Assert.AreEqual(encodedId, converter.ConvertTo(rawId, typeof(string)));
            Assert.AreEqual(rawId, converter.ConvertFrom(encodedId));
        }


    }
}