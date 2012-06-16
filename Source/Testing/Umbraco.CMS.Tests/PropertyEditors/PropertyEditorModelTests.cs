using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;

namespace Umbraco.Tests.Cms.PropertyEditors
{
    [TestFixture]
    public class PropertyEditorModelTests
    {
        [Test]
        public void PropertyEditorModelTests_GetSerializedValue()
        {
            // Arrange
            var model = new TestEditorModel
            {
                TestHiveId = new HiveId("test"),
                TestString = "test",
                TestInt = 1,
                TestDouble = 1.2,
                TestDateTime = DateTime.Parse("2011-01-01"),
                TestReadOnly = "readonly"
            };

            // Act
            var serialized = model.GetSerializedValue();

            // Assert
            Assert.IsTrue(serialized.Keys.Count == 5);

            Assert.IsTrue(serialized.ContainsKey("TestHiveId"));
            Assert.IsTrue(serialized.ContainsKey("TestString"));
            Assert.IsTrue(serialized.ContainsKey("TestInt"));
            Assert.IsTrue(serialized.ContainsKey("TestDouble"));
            Assert.IsTrue(serialized.ContainsKey("TestDateTime"));
            Assert.IsFalse(serialized.ContainsKey("TestReadOnly"));

            Assert.IsTrue(serialized["TestHiveId"] is HiveId);
            Assert.IsTrue(serialized["TestString"] is string);
            Assert.IsTrue(serialized["TestInt"] is int);
            Assert.IsTrue(serialized["TestDouble"] is double);
            Assert.IsTrue(serialized["TestDateTime"] is DateTime);

            Assert.IsTrue((HiveId)serialized["TestHiveId"] == new HiveId("test"));
            Assert.IsTrue((string)serialized["TestString"] == "test");
            Assert.IsTrue((int)serialized["TestInt"] == 1);
            Assert.IsTrue((double)serialized["TestDouble"] == 1.2);
            Assert.IsTrue((DateTime)serialized["TestDateTime"] == DateTime.Parse("2011-01-01"));
        }

        [Test]
        public void PropertyEditorModelTests_SetModelValues()
        {
            // Arrange
            var serialized = new Dictionary<string, object>
            {
                {"TestHiveId", new HiveId("test")},
                {"TestString", "test"},
                {"TestInt", 1},
                {"TestDouble", 1.2},
                {"TestDateTime", DateTime.Parse("2011-01-01")},
                {"TestReadOnly", "readonly"}
            };

            // Act
            var model = new TestEditorModel();
            model.SetModelValues(serialized);

            // Assert
            Assert.IsTrue(model.TestHiveId == new HiveId("test"));
            Assert.IsTrue(model.TestString == "test");
            Assert.IsTrue(model.TestInt == 1);
            Assert.IsTrue(model.TestDouble == 1.2);
            Assert.IsTrue(model.TestDateTime == DateTime.Parse("2011-01-01"));
            Assert.IsTrue(model.TestReadOnly == null);

        }
        public class TestEditorModel : EditorModel
        {
            public HiveId TestHiveId { get; set; }
            public string TestString { get; set; }
            public int TestInt { get; set; }
            public double TestDouble { get; set; }
            public DateTime TestDateTime { get; set; }

            [ReadOnly(true)]
            public string TestReadOnly { get; set; }
        }
    }
}
