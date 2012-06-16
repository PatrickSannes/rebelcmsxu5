using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class LocalizedStringTest
    {
        [Test]
        [Category(TestOwner.Framework)]
        public void LocalizedStringTest_Is_Implicitly_String()
        {
            //Arrange
            var s = new LocalizedString("Hello World");
            //Act

            //Assert
            Assert.IsTrue(s == "Hello World");
        }

        [Test]
        [Category(TestOwner.Framework)]
        public void LocalizedStringTest_Current_Culture_Stored()
        {
            //Arrange
            var s = new LocalizedString("Hello World");
            //Act

            //Assert
            Assert.IsTrue(s.GetValue(Thread.CurrentThread.CurrentCulture) == "Hello World");
        }

        [Test]
        [Category(TestOwner.Framework)]
        public void LocalizedStringTest_Non_Set_Culture_Returns_Null()
        {
            //Arrange
            var threadCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-AU");

            var s = new LocalizedString("Hello World");
            var culture = new System.Globalization.CultureInfo("es-ES");
            //Act

            //Assert
            Assert.IsNull(s.GetValue(culture));

            Thread.CurrentThread.CurrentCulture = threadCulture;
        }

        [Test]
        [Category(TestOwner.Framework)]
        public void LocalizedStringTest_Not_Equals_Operator_Works()
        {
            //Arrange
            var s = new LocalizedString("Hello World");
            //Act

            //Assert
            Assert.IsTrue(s != "Twitter");
        }

        [Test]
        [Category(TestOwner.Framework)]
        public void LocalizedStringTest_Concattination_Operator_Works()
        {
            //Arrange
            var s = new LocalizedString("Hello");
            //Act

            //Assert
            Assert.IsTrue((s + " World") == "Hello World");
        }

        [Test]
        [Category(TestOwner.Framework)]
        public void LocalizedStringTest_New_Cultures_Can_Be_Added()
        {
            //Arrange
            var threadCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-AU");

            var s = new LocalizedString("Hello World");
            var culture = new CultureInfo("es-ES");
            s.Add(culture, "Hola a todos");
            //Act

            //Assert
            Assert.IsNotNull(s.GetValue(culture));
            Assert.IsNotNull(s.GetValue(Thread.CurrentThread.CurrentCulture));

            Thread.CurrentThread.CurrentCulture = threadCulture;
        }

        [Test]
        [Category(TestOwner.Framework)]
        public void LocalizedStringTest_New_Instance_Creatable_By_Casting()
        {
            //Arrange
            LocalizedString s = "Hello World";
            //Act

            //Assert
            Assert.IsTrue(s == "Hello World");
            Assert.AreEqual(Thread.CurrentThread.CurrentCulture, s.DefaultCulture);
        }
    }
}
