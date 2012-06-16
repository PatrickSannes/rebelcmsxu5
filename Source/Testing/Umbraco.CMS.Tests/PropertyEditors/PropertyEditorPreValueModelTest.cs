using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.PropertyEditors;
using NUnit.Framework;
using System;

namespace Umbraco.Tests.Cms.PropertyEditors
{
    
    
    /// <summary>
    ///This is a test class for PropertyEditorPreValueModelTest and is intended
    ///to contain all PropertyEditorPreValueModelTest Unit Tests
    ///</summary>
    [TestFixture]
    public class PropertyEditorPreValueModelTest
    {


        /// <summary>
        ///A test for PreValueModel Constructor
        ///</summary>
        [Test]
        public void PropertyEditorPreValueModel_Set_Model_Value_From_Serialized_String()
        {
            //Arrange
            
            var serialized =
                @"<preValues>
  <preValue name=""String1"" type=""System.String""><![CDATA[hello]]></preValue>
  <preValue name=""String2"" type=""System.String""><![CDATA[world]]></preValue>
  <preValue name=""String3"" type=""System.String""><![CDATA[blah]]></preValue>
  <preValue name=""Int1"" type=""System.Int32""><![CDATA[1]]></preValue>
  <preValue name=""Int2"" type=""System.Int32""><![CDATA[2]]></preValue>
  <preValue name=""DateTime1"" type=""System.DateTime""><![CDATA[2011-01-02T00:00:00]]></preValue>
  <preValue name=""DateTime2"" type=""System.DateTime""><![CDATA[2012-02-01T00:00:00]]></preValue>
</preValues>";

            //Act

            var preValModel = new TestPreValueModel();
            preValModel.SetModelValues(serialized);

            //Assert

            Assert.AreEqual("hello", preValModel.String1);
            Assert.AreEqual("world", preValModel.String2);
            Assert.AreEqual("blah", preValModel.String3);
            Assert.AreEqual(1, preValModel.Int1);
            Assert.AreEqual(2, preValModel.Int2);
            Assert.AreEqual(DateTime.Parse("2011-01-02"), preValModel.DateTime1);
            Assert.AreEqual(DateTime.Parse("2012-02-01"), preValModel.DateTime2);

        }

        /// <summary>
        ///A test for GetSerializedValue
        ///</summary>
        [Test]
        public void PropertyEditorPreValueModel_Get_Serialized_Value()
        {
            //Arrange 

            var preValModel = new TestPreValueModel()
                                  {
                                      String1 = "hello",
                                      String2 = "world",
                                      String3 = "blah",
                                      Int1 = 1,
                                      Int2 = 2,
                                      DateTime1 = DateTime.Parse("2011-01-02"),
                                      DateTime2 = DateTime.Parse("2012-02-01")
                                  };

            //Act

            var serialized = preValModel.GetSerializedValue();

            //Assert

            Assert.AreEqual(
                @"<preValues>
  <preValue name=""String1"" type=""System.String""><![CDATA[hello]]></preValue>
  <preValue name=""String2"" type=""System.String""><![CDATA[world]]></preValue>
  <preValue name=""String3"" type=""System.String""><![CDATA[blah]]></preValue>
  <preValue name=""Int1"" type=""System.Int32""><![CDATA[1]]></preValue>
  <preValue name=""Int2"" type=""System.Int32""><![CDATA[2]]></preValue>
  <preValue name=""DateTime1"" type=""System.DateTime""><![CDATA[2011-01-02T00:00:00]]></preValue>
  <preValue name=""DateTime2"" type=""System.DateTime""><![CDATA[2012-02-01T00:00:00]]></preValue>
</preValues>",
                serialized);

        }

        public class TestPreValueModel : PreValueModel
        {           
            public string String1 { get; set; }
            public string String2 { get; set; }
            public string String3 { get; set; }

            public int Int1 { get; set; }
            public int Int2 { get; set; }

            public DateTime DateTime1 { get; set; }
            public DateTime DateTime2 { get; set; }

        }
    }
}
