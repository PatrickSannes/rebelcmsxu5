using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Testing.PartialTrust;
using Umbraco.Tests.Extensions;
using Umbraco.Tests.Extensions.ModelForCloneTests;



namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class ObjectExtensionsTests : AbstractPartialTrustFixture<ObjectExtensionsTests>
    {
        protected override void FixtureSetup()
        {
            base.FixtureSetup();
            TestHelper.SetupLog4NetForTests();
        }

        [Test]
        public void ObjectExtensions_Object_To_Dictionary()
        {
            //Arrange

            var obj = new { Key1 = "value1", Key2 = "value2", Key3 = "value3" };

            //Act

            var d = obj.ToDictionary<string>();

            //Assert

            Assert.IsTrue(d.Keys.Contains("Key1"));
            Assert.IsTrue(d.Keys.Contains("Key2"));
            Assert.IsTrue(d.Keys.Contains("Key3"));
            Assert.AreEqual(d["Key1"], "value1");
            Assert.AreEqual(d["Key2"], "value2");
            Assert.AreEqual(d["Key3"], "value3");
        }

		[Test]
		[TestOnlyInFullTrust]
		public void CanConvertIntToNullableInt()
		{
			var i = 1;
			var result = i.TryConvertTo<int?>();
			Assert.That(result.Success, Is.True);
		}

		[Test]
		[TestOnlyInFullTrust]
		public void CanConvertNullableIntToInt()
		{
			int? i = 1;
			var result = i.TryConvertTo<int>();
			Assert.That(result.Success, Is.True);
		}

        [Test]
        public void WhenCopyingAnObject_UsingBinaryFormatter_CannotWorkInPartialTrust()
        {
            try
            {
                // Arrange
                var myObject = new CloneTest("root", 1, "protected-value", "private-value", new CloneTest("child", 2, "child-protected-value", "child-private-value"), new CloneTest("public-child", 3, "publicchild-protected-value", "publicchild-private-value"));
            
                // Act
                var myCopy = myObject.Clone();

                // Assert
                AssertClonedObject(myCopy, myObject);
            }
            catch (SecurityException)
            {
                if (AppDomain.CurrentDomain.FriendlyName.StartsWith(PartialTrustHelper<ObjectExtensionsTests>.PartialTrustAppDomainName)) Assert.Pass();
            }
            if (AppDomain.CurrentDomain.FriendlyName.StartsWith(PartialTrustHelper<ObjectExtensionsTests>.PartialTrustAppDomainName)) Assert.Fail("Did not throw an exception");
        }

        [Test]
        public void WhenCopyingAnObject_ObjectIsCloned()
        {
            // Arrange
            var myObject = new CloneTest("root", 1, "protected-value", "private-value", new CloneTest("child", 2, "child-protected-value", "child-private-value"), new CloneTest("public-child", 3, "publicchild-protected-value", "publicchild-private-value"));

            // Act
            CloneTest myCopy = myObject.DeepCopy(); // We don't use var because the return type is CloneOf<T>
            AssertClonedObject(myCopy, myObject);

            // Assert
            myCopy.Integer = 5;
            myCopy.ProtectedDeep.Integer = 6;
            myCopy.PublicDeep = new CloneTest("brand-new", 2, "new-protected-value", "new-private-value");
            myCopy.AssertTestSetPrivateFieldBackedProperty("new-private-value");
            myCopy.AssertTestSetProtectedFieldBackedProperty("new-protected-value");

            AssertClonedObjectIsDifferent(myCopy, myObject);
        }

        [Test]
        [TestOnlyInFullTrust]
        public void WhenCopyingAnObject_ObjectIsCloned_Exception_InFullTrust() // Random framework type
        {
            // Arrange
            var myObject = new Exception("blah");

            // Act
            var myCopyResult = myObject.DeepCopy();
            Exception myCopy = myCopyResult; // Compile-check test of dynamic cast operator

            // Assert
            Assert.False(myCopyResult.PartialTrustCausedPartialClone);
            Assert.That(myObject.Message, Is.EqualTo(myCopy.Message));

            myCopy.Data.Add("blah", "ok");
            Assert.That(myCopy.Data.Keys, Has.Some.EqualTo("blah"));
            Assert.That(myObject.Data.Keys, Has.None.EqualTo("blah"));
        }

        [Test]
        [Description("Since the private fields on an Exception object cannot be accessed in partial trust, this clone option should not throw but result in a half-copied object")]
        public void WhenCopyingAnException_InPartialTrust_SucceedsButWithSomeFieldsDefault() // Random framework type
        {
            // Arrange
            var myObject = new Exception();

            // Act
            var myCopyResult = myObject.DeepCopy();
            Exception myCopy = myCopyResult; // Compile-check test of dynamic cast operator

            // Assert
            Assert.True(myCopyResult.PartialTrustCausedPartialClone);
            Assert.That(myObject.Message, Is.EqualTo(myCopy.Message));

            myCopy.Data.Add("blah", "ok");
            Assert.That(myCopy.Data.Keys, Has.Some.EqualTo("blah"));
            Assert.That(myObject.Data.Keys, Has.None.EqualTo("blah"));
        }

        [Test]
        [Description("Since the private fields on an Exception object cannot be accessed in partial trust, but we're asking for a clone without hiding errors, " +
                     "this clone option should throw")]
        public void WhenCopyingAnException_InPartialTrust_WithoutHidingErrors_Throws() // Random framework type
        {
            // Arrange
            var myObject = new Exception();

            // Act
            Assert.Throws<FieldAccessException>(() => myObject.DeepCopy(false));
        }

        [Test]
        [TestOnlyInFullTrust]
        public void WhenCopyingAnObject_ObjectIsCloned_RazorViewEngine_InFullTrust() // Random framework type
        {
            AssertCloneRazorViewEngine();
        }

        // Cannot actually even instantiate RazorViewEngine in partial trust domain without HttpContext.Current being non-null [Test]
        public void WhenCopyingAnObject_ObjectIsCloned_RazorViewEngine() // Random framework type
        {
            AssertCloneRazorViewEngine();
        }

        private static void AssertCloneRazorViewEngine()
        {
            // Arrange
            var myObject = new RazorViewEngine();

            // Act
            RazorViewEngine myCopy = myObject.DeepCopy();

            // Assert
            Assert.That(myObject.AreaMasterLocationFormats, Is.EqualTo(myCopy.AreaMasterLocationFormats));
        }

        private static IEnumerable<Func<CloneTest, object>> GetComparisons()
        {
            yield return x => x.String;
            yield return x => x.Integer;
            yield return x => x.ProtectedFieldBackedProperty;
            yield return x => x.PrivateFieldBackedProperty;

            yield return x => x.ProtectedDeep.String;
            yield return x => x.ProtectedDeep.Integer;
            yield return x => x.ProtectedDeep.ProtectedFieldBackedProperty;
            yield return x => x.ProtectedDeep.PrivateFieldBackedProperty;

            yield return x => x.PublicDeep.String;
            yield return x => x.PublicDeep.Integer;
            yield return x => x.PublicDeep.ProtectedFieldBackedProperty;
            yield return x => x.PublicDeep.PrivateFieldBackedProperty;

            yield return x => x.MyComplexField.String;
            yield return x => x.MyComplexField.Integer;
            yield return x => x.MyComplexField.ProtectedFieldBackedProperty;
            yield return x => x.MyComplexField.PrivateFieldBackedProperty;
        }

        private static void AssertClonedObject(CloneTest myCopy, CloneTest myObject)
        {
            foreach (var expression in GetComparisons())
            {
                Assert.That(expression.Invoke(myObject), Is.EqualTo(expression.Invoke(myCopy)));                
            }
        }

        private static void AssertClonedObjectIsDifferent(CloneTest myCopy, CloneTest myObject)
        {
            var allAreSame = GetComparisons().All(expression => expression.Invoke(myObject) == expression.Invoke(myCopy));
            if (allAreSame) Assert.Fail("All properties are equal comparing a changed cloned object with its original");
        }

        /// <summary>
        /// Run once before each test in derived test fixtures.
        /// </summary>
        public override void TestSetup()
        {
            return;
        }

        /// <summary>
        /// Run once after each test in derived test fixtures.
        /// </summary>
        public override void TestTearDown()
        {
            return;
        }
    }
}