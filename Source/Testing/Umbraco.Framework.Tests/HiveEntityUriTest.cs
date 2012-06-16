using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbraco.Framework;
using Umbraco.Framework.DataManagement;

namespace Umbraco.Tests.Framework
{
    /// <summary>
    ///This is a test class for HiveEntityUriTest and is intended
    ///to contain all HiveEntityUriTest Unit Tests
    ///</summary>
    [TestClass()]
    public class HiveEntityUriTest
    {
        [TestMethod]
        public void HiveEntityUriTests_Valid_Id_Is_Not_Equal_To_Empty_Id()
        {
            //Arrange
            var dummyId1 = new HiveEntityUri(1);
            var dummyId2 = HiveEntityUri.Empty;

            //Act

            //Assert
            Assert.IsFalse(dummyId1 == dummyId2);
        }

        [TestMethod()]
        public void HiveEntityUri_Is_System()
        {
            var sys = HiveEntityUri.ConvertIntToGuid(-345);
            var nonSys1 = HiveEntityUri.ConvertIntToGuid(1);
            var nonSys2 = new HiveEntityUri(Guid.NewGuid());
            var nonSys3 = new HiveEntityUri("hello");

            var fromFullUri = new HiveEntityUri(sys.ToFriendlyString());
            var nonSysFullUri = new HiveEntityUri(Guid.NewGuid());

            Assert.IsTrue(sys.IsSystem());
            Assert.IsTrue(!nonSys1.IsSystem());
            Assert.IsTrue(!nonSys2.IsSystem());
            Assert.IsTrue(!nonSys3.IsSystem());
            Assert.IsTrue(fromFullUri.IsSystem());
            Assert.IsFalse(nonSysFullUri.IsSystem());
        }

   
        /// <summary>
        ///A test for HiveEntityUri Constructor
        ///</summary>
        [TestMethod()]
        public void HiveEntityUri_EntityAsGuidTest()
        {
            Guid guid = Guid.NewGuid();
            HiveEntityUri target = new HiveEntityUri(guid);
            Assert.AreEqual(target.AsGuid, guid);
        }

        [TestMethod]
        public void HiveEntityUri_StringPartsCorrect()
        {
            var id = new HiveEntityUri("/News/About-us");

            Assert.IsTrue(id.StringParts.SequenceEqual(new[] { "~", "News", "About-us" }), "StringParts has {0}", id.StringParts.ToDebugString());
        }

        [TestMethod]
        public void HiveEntityUri_CtorFromString_ExposesSchemeAndProvider()
        {
            var id = new HiveEntityUri("content://provider-name/5");

            Assert.AreEqual(id.HiveEntityType, "content");
            Assert.AreEqual(id.HiveOwnerProvider, "provider-name");
            Assert.AreEqual(id.SerializationType, DataSerializationTypes.LargeInt);
            Assert.AreEqual(id.AsInt, 5);

            var id2 = new HiveEntityUri("content://provider-name/C54AB6D9-B679-4AAC-A3D6-4F274700094B");

            Assert.AreEqual(id2.HiveEntityType, "content");
            Assert.AreEqual(id2.HiveOwnerProvider, "provider-name");
            Assert.AreEqual(id2.SerializationType, DataSerializationTypes.Guid);
            Assert.AreEqual(id2.AsGuid, Guid.Parse("C54AB6D9-B679-4AAC-A3D6-4F274700094B"));

            var id3 = new HiveEntityUri("C54AB6D9-B679-4AAC-A3D6-4F274700094B");

            Assert.AreEqual(id3.HiveEntityType, "hive");
            Assert.AreEqual(id3.HiveOwnerProvider, "root");
            Assert.AreEqual(id3.SerializationType, DataSerializationTypes.Guid);
            Assert.AreEqual(id3.AsGuid, Guid.Parse("C54AB6D9-B679-4AAC-A3D6-4F274700094B"));

            var id5 = new HiveEntityUri(Guid.Parse("C54AB6D9-B679-4AAC-A3D6-4F274700094B"));

            Assert.AreEqual(id5.HiveEntityType, "hive");
            Assert.AreEqual(id5.HiveOwnerProvider, "root");
            Assert.AreEqual(id5.SerializationType, DataSerializationTypes.Guid);
            Assert.AreEqual(id5.AsGuid, Guid.Parse("C54AB6D9-B679-4AAC-A3D6-4F274700094B"));

            var id6 = new HiveEntityUri("5");

            Assert.AreEqual(id6.HiveEntityType, "hive");
            Assert.AreEqual(id6.HiveOwnerProvider, "root");
            Assert.AreEqual(id6.SerializationType, DataSerializationTypes.LargeInt);
            Assert.AreEqual(id6.AsInt, 5);

            var id7 = new HiveEntityUri("Templates/Blah.cshtml");

            Assert.AreEqual(id7.HiveEntityType, "hive");
            Assert.AreEqual(id7.HiveOwnerProvider, "root");
            Assert.AreEqual(id7.SerializationType, DataSerializationTypes.String);
            Assert.AreEqual(id7.GetAllStringParts(), "Templates/Blah.cshtml");

            var id7b = new HiveEntityUri("/Templates/Blah.cshtml");

            Assert.AreEqual(id7b.HiveEntityType, "hive");
            Assert.AreEqual(id7b.HiveOwnerProvider, "root");
            Assert.AreEqual(id7b.SerializationType, DataSerializationTypes.String);
            Assert.AreEqual(id7b.GetAllStringParts(), "~/Templates/Blah.cshtml");

            var id8 = new HiveEntityUri("template://default/Blah.cshtml");

            Assert.AreEqual(id8.HiveEntityType, "template");
            Assert.AreEqual(id8.HiveOwnerProvider, "default");
            Assert.AreEqual(id8.SerializationType, DataSerializationTypes.String);
            Assert.AreEqual(id8.GetAllStringParts(), "Blah.cshtml");

            var id9 = new HiveEntityUri("Blah.cshtml");

            Assert.AreEqual(id9.HiveEntityType, "hive");
            Assert.AreEqual(id9.HiveOwnerProvider, "root");
            Assert.AreEqual(id9.SerializationType, DataSerializationTypes.String);
            Assert.AreEqual(id9.GetAllStringParts(), "Blah.cshtml");
        }

        [TestMethod]
        public void HiveEntityUri_ParseFromString_ExposesSchemeAndProvider()
        {
            var id = HiveEntityUri.Parse("content://provider-name/5");

            Assert.AreEqual(id.HiveEntityType, "content");
            Assert.AreEqual(id.HiveOwnerProvider, "provider-name");
            Assert.AreEqual(id.SerializationType, DataSerializationTypes.LargeInt);
            Assert.AreEqual(id.AsInt, 5);
        }

        [TestMethod]
        public void HiveEntityUri_StaticFromUriString_ExposesSchemeAndProvider()
        {
            var id = HiveEntityUri.FromUriString("content://provider-name/5");

            Assert.AreEqual(id.HiveEntityType, "content");
            Assert.AreEqual(id.HiveOwnerProvider, "provider-name");
            Assert.AreEqual(id.SerializationType, DataSerializationTypes.LargeInt);
            Assert.AreEqual(id.AsInt, 5);
        }

        [TestMethod]
        public void HiveEntityUri_With_String_Serialization_Equals_Overload_Does_Not_Match_Guid()
        {
            var id = new HiveEntityUri("TemplatePath/TemplateName.cshtml");
            var id2 = new HiveEntityUri(Guid.NewGuid());

            Assert.IsFalse(id == id2);
        }

        [TestMethod]
        public void HiveEntityUri_Equals_ToString_With_String_Serialization()
        {
            var id = new HiveEntityUri("TemplatePath/TemplateName.cshtml");
            var id2 = new HiveEntityUri("Home-Page.cshtml");

            var output = id.ToString();
            var output2 = id2.ToString();

            Assert.AreEqual("TemplatePath/TemplateName.cshtml", HiveEntityUri.Parse(output.FromUrlBase64()).GetAllStringParts());
            Assert.AreEqual("Home-Page.cshtml", HiveEntityUri.Parse(output2.FromUrlBase64()).GetAllStringParts());
        }

        [TestMethod]
        public void HiveEntityUri_Equals_Overload()
        {
            var nodeIdInt1 = new HiveEntityUri(1);
            var nodeIdInt2 = new HiveEntityUri(2);
            var nodeIdInt3 = new HiveEntityUri(1);
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var nodeIdGuid1 = new HiveEntityUri(guid1);
            var nodeIdGuid2 = new HiveEntityUri(guid2);
            var nodeIdGuid3 = new HiveEntityUri(guid1);

            var manual1 = new HiveEntityUri("content://provider-name/D200290D-011B-429D-82DD-52D48BF33B60");
            var manual2 = new HiveEntityUri("hive://root/D200290D-011B-429D-82DD-52D48BF33B60");
            var manual3 = new HiveEntityUri("D200290D-011B-429D-82DD-52D48BF33B60");
            var manual4 = new HiveEntityUri("hive://provider-name/D200290D-011B-429D-82DD-52D48BF33B60");
            var manual5 = new HiveEntityUri("hive://different-provider-name/D200290D-011B-429D-82DD-52D48BF33B60");
            var manual6 = new HiveEntityUri("content://different-provider-name/D200290D-011B-429D-82DD-52D48BF33B60");

            Assert.AreEqual(manual1, manual2);
            Assert.AreEqual(manual1, manual3);
            Assert.AreEqual(manual1, manual4);
            Assert.AreNotEqual(manual1, manual5);
            Assert.AreNotEqual(manual1, manual6);
            Assert.AreNotEqual(manual4, manual5);
            Assert.AreEqual(manual2, manual5);
            Assert.AreEqual(manual3, manual5);
            Assert.AreEqual(manual5, manual6);
            Assert.AreEqual((HiveEntityUri)null, (HiveEntityUri)null); // Check null is handled by Equals overload
            Assert.AreNotEqual((HiveEntityUri)null, manual1); // Check null is handled by Equals overload

            Assert.AreEqual(nodeIdInt1, nodeIdInt1);
            Assert.AreNotEqual(nodeIdInt1, nodeIdInt2);
            Assert.AreEqual(nodeIdInt1, nodeIdInt3);

            Assert.AreEqual(nodeIdGuid1, nodeIdGuid1);
            Assert.AreNotEqual(nodeIdGuid1, nodeIdGuid2);
            Assert.AreEqual(nodeIdGuid1, nodeIdGuid3);

            Assert.AreEqual((int)nodeIdInt1, 1);
            Assert.AreNotEqual((int)nodeIdInt1, 2);
            Assert.AreNotEqual(nodeIdInt1, nodeIdGuid1);
            Assert.AreEqual(nodeIdGuid1, guid1);
            Assert.AreNotEqual(nodeIdGuid1, guid2);

        }

        [TestMethod]
        public void HiveEntityUri_Inequality_Operator_Overload()
        {
            var nodeIdInt1 = new HiveEntityUri(1);
            var nodeIdInt2 = new HiveEntityUri(2);
            var nodeIdInt3 = new HiveEntityUri(1);
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var nodeIdGuid1 = new HiveEntityUri(guid1);
            var nodeIdGuid2 = new HiveEntityUri(guid2);
            var nodeIdGuid3 = new HiveEntityUri(guid1);

            Assert.IsTrue(nodeIdInt1 != nodeIdInt2);

            Assert.IsTrue(nodeIdGuid1 != nodeIdGuid2);

            Assert.IsTrue(nodeIdInt1 != 2);
            Assert.IsTrue(nodeIdInt1 != nodeIdGuid1);
            Assert.IsTrue(nodeIdGuid1 != guid2);
        }

        [TestMethod]
        public void HiveEntityUri_Cast_To_String()
        {
            var id = new HiveEntityUri(1);

            var output = (string)id;

            Assert.AreEqual(id.ToString(), output);
        }

        [TestMethod]
        public void HiveEntityUri_Null_Cast_To_String()
        {
            HiveEntityUri id = null;

            var output = (string)id;

            Assert.AreEqual(string.Empty, output);
        }

        [TestMethod]
        public void HiveEntityUri_Guid_As_String_Serialization_Equals_Guid_As_Guid_Serialization()
        {
            var guid = Guid.NewGuid();
            var id = new HiveEntityUri(guid) {SerializationType = DataSerializationTypes.String};
            var id2 = new HiveEntityUri(guid);

            Assert.AreEqual(DataSerializationTypes.String, id.SerializationType);
            Assert.AreEqual(DataSerializationTypes.Guid, id2.SerializationType);

            Assert.IsTrue(id == id2);
            Assert.IsTrue(id.Equals(id2));
        }
    }
}