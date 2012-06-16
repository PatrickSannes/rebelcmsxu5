using System;
using NUnit.Framework;
using Umbraco.Framework;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture] // Some NUnit tests in here too
    public class HiveIdValueTest
    {
        [TestCase]
        public void ValidValue_NotEqualTo_Empty()
        {
            Assert.IsFalse(HiveIdValue.Empty == new HiveIdValue(1));
            Assert.AreNotEqual(HiveIdValue.Empty, new HiveIdValue(1));
        }

        [TestCase]
        public void ConvertIntToGuid()
        {
            // Arrange
            var val1 = HiveIdValue.ConvertIntToGuid(345);

            Assert.AreEqual(HiveIdValueTypes.Guid, val1.Type);
            Assert.AreEqual(Guid.Parse("00000000-0000-0000-0000-000000000345"), (Guid)val1.Value);
            Assert.IsFalse(val1.IsSystem);
        }

        [TestCase]
        public void ConvertIntToGuid_AsSystem()
        {
            // Arrange
            var val1 = HiveIdValue.ConvertIntToGuid(-345);

            Assert.AreEqual(HiveIdValueTypes.Guid, val1.Type);
            Assert.AreEqual(Guid.Parse("10000000-0000-0000-0000-000000000345"), (Guid)val1.Value);
            Assert.IsTrue(val1.IsSystem);
        }

        [TestCase()]
        public void ToString_ShouldShowValueOrNull()
        {
            // Arrange
            var val1 = new HiveIdValue("my-string-id");

            // Assert
            Assert.AreEqual("my-string-id", val1.ToString());
            Assert.AreEqual("(null)", HiveIdValue.Empty.ToString());
        }

        [TestCase()]
        public void Hashcode_ShouldBeEqualAcrossInstancesOfSameValue()
        {
            // Arrange
            var val1 = new HiveIdValue("my-string-id");
            var val2 = new HiveIdValue("my-string-id");

            // Assert
            Assert.AreEqual(val1.GetHashCode(), val2.GetHashCode());
            Assert.AreNotEqual(val1.GetHashCode(), HiveIdValue.Empty.GetHashCode());
        }

        [TestCase]
        public void Hashcode_ShouldNotBeEqualAcrossInstancesOfDifferentValue()
        {
            // Arrange
            var val1 = new HiveIdValue("my-string-id");
            var val2 = new HiveIdValue("my-string-id2");

            // Assert
            Assert.AreNotEqual(val1.GetHashCode(), val2.GetHashCode());
            Assert.AreNotEqual(val1.GetHashCode(), HiveIdValue.Empty.GetHashCode());
        }

        [TestCase]
        public void Equals_ShouldBeEqual()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var stringVal = new HiveIdValue("my-string-id");
            var stringVal2 = new HiveIdValue("my-string-id");
            var guidVal = new HiveIdValue(guid);
            var guidVal2 = new HiveIdValue(guid);
            var intVal = new HiveIdValue(5);
            var intVal2 = new HiveIdValue(5);

            // Assert
            Assert.IsTrue(stringVal.Equals(stringVal2));
            Assert.IsTrue(guidVal.Equals(guidVal2));
            Assert.IsTrue(intVal.Equals(intVal2));
            Assert.IsTrue(stringVal == stringVal2);
            Assert.IsTrue(guidVal == guidVal2);
            Assert.IsTrue(intVal == intVal2);
            Assert.IsFalse(stringVal != stringVal2);
            Assert.IsFalse(guidVal != guidVal2);
            Assert.IsFalse(intVal != intVal2);
        }

        [TestCase("this-is-my-string")]
        [TestCase(@"C:\this-is-my-string\hiya")]
        public void Ctor_FromString_ToStringEqualsOriginal(string input)
        {
            // Arrange
            var val = new HiveIdValue(input);

            NUnit.Framework.Assert.AreEqual(val.ToString(), input);
        }

        [TestCase]
        public void Equals_ShouldNotBeEqual()
        {
            // Arrange
            var stringVal = new HiveIdValue("my-string-id");
            var stringVal2 = new HiveIdValue("my-string-id2");
            var guidVal = new HiveIdValue(Guid.NewGuid());
            var guidVal2 = new HiveIdValue(Guid.NewGuid());
            var intVal = new HiveIdValue(5);
            var intVal2 = new HiveIdValue(6);

            // Assert
            Assert.IsFalse(stringVal.Equals(stringVal2));
            Assert.IsFalse(guidVal.Equals(guidVal2));
            Assert.IsFalse(intVal.Equals(intVal2));
            Assert.IsFalse(stringVal == stringVal2);
            Assert.IsFalse(guidVal == guidVal2);
            Assert.IsFalse(intVal == intVal2);
            Assert.IsTrue(stringVal != stringVal2);
            Assert.IsTrue(guidVal != guidVal2);
            Assert.IsTrue(intVal != intVal2);
        }

        [TestCase]
        public void Equals_ShouldNotBeEqual_DueToDifferentTypes()
        {
            // Arrange
            var val = new HiveIdValue("my-string-id");
            var val2 = new HiveIdValue(Guid.NewGuid());

            // Assert
            Assert.AreNotEqual(val, val2);
            Assert.IsFalse(val == val2);
            Assert.IsTrue(val != val2);
        }

        [TestCase]
        public void Constructed_As_String_Sets_Type()
        {
            // Arrange
            var val = new HiveIdValue("my-string-id");

            // Act

            // Assert
            Assert.AreEqual(HiveIdValueTypes.String, val.Type);
            Assert.AreEqual("my-string-id", val.Value);
            Assert.AreEqual("my-string-id", (string)val);
        }

        [TestCase]
        public void Constructed_As_Guid_Sets_Type()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var val = new HiveIdValue(guid);

            // Act

            // Assert
            Assert.AreEqual(HiveIdValueTypes.Guid, val.Type);
            Assert.AreEqual(guid, val.Value);
            Assert.AreEqual(guid, (Guid)val);
        }

        [TestCase]
        public void Constructed_As_Int_Sets_Type()
        {
            // Arrange
            var val = new HiveIdValue(5);

            // Act

            // Assert
            Assert.AreEqual(HiveIdValueTypes.Int32, val.Type);
            Assert.AreEqual(5, val.Value);
            Assert.AreEqual(5, (int)val);
        }
    }
}