using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;
using Umbraco.Framework;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class EnumHelperTests
    {

        public enum TestEnum
        {
            [Display(Name = "Value 1")]
            Value1 = 10,

            Value2,

            [Display(Name = "Value number 3")]
            Value3
        }

        [Test]
        public void EnumHelper_Get_Names()
        {
            var names = EnumHelper.GetNames<TestEnum>();

            Assert.AreEqual("Value1", names.ElementAt(0));
            Assert.AreEqual("Value2", names.ElementAt(1));
            Assert.AreEqual("Value3", names.ElementAt(2));
        }

        [Test]
        public void EnumHelper_Get_Display_Names()
        {
            var names = EnumHelper.GetDisplayNames<TestEnum>();

            Assert.AreEqual("Value 1", names.ElementAt(0));
            Assert.AreEqual("Value2", names.ElementAt(1));
            Assert.AreEqual("Value number 3", names.ElementAt(2));
        }

        [Test]
        public void EnumHelper_Get_Name_Value_Collection()
        {
            var names = EnumHelper.GetDisplayNameValueCollection<TestEnum>();

            Assert.AreEqual(10, names.ElementAt(0).Item1);
            Assert.AreEqual("Value1", names.ElementAt(0).Item2);
            Assert.AreEqual("Value 1", names.ElementAt(0).Item3);

            Assert.AreEqual(11, names.ElementAt(1).Item1);
            Assert.AreEqual("Value2", names.ElementAt(1).Item2);
            Assert.AreEqual("Value2", names.ElementAt(1).Item3);

            Assert.AreEqual(12, names.ElementAt(2).Item1);
            Assert.AreEqual("Value3", names.ElementAt(2).Item2);
            Assert.AreEqual("Value number 3", names.ElementAt(2).Item3);
        }
    }
}
