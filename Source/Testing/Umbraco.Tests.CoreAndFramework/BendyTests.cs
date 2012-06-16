using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics;

namespace Umbraco.Tests.CoreAndFramework
{
   

    /// <summary>
    /// Summary description for BendyTests
    /// </summary>
    [TestFixture]
    public class BendyTests
    {
        [Test]
        public void Bendy_CanSetAndGetFirstLevelPropertiesFromObject_Lazily()
        {
            // Arrange
            var testComplexObject = DateTimeOffset.UtcNow;
            var testComplexObject2 = new KeyValuePair<string, int>("blah", 5);

            // Act
            dynamic bendyObject = new BendyObject(testComplexObject);
            dynamic bendyObject2 = new BendyObject(testComplexObject2);

            // Assert
            Assert.AreEqual(testComplexObject.Date, bendyObject.Date);
            Assert.AreEqual(testComplexObject.Day, bendyObject.Day);
            Assert.AreEqual(testComplexObject.Date.Minute, bendyObject.Date.Minute);

            Assert.AreEqual(testComplexObject2.Key, bendyObject2.Key);
            Assert.AreEqual(testComplexObject2.Value, bendyObject2.Value);

            // Act again, test overwriting property values
            bendyObject.Day = 8;
            Assert.AreEqual(8, bendyObject.Day);

            bendyObject2.Value = 6; // Test overwriting value
            Assert.AreEqual(6, bendyObject2.Value);
        }


        [Test]
        public void Bendy_CanSetAndGetLazyProperty()
        {
            // Arrange
            var bendyObject = new BendyObject();
            var delegateTest = 0;

            // Act
            bendyObject.AddLazy("MyProperty", () => "this is my value");
            bendyObject.AddLazy("FirstDelegate", () =>
                {
                    delegateTest = 1;
                    return "value 1";
                });
            bendyObject.AddLazy("SecondDelegate", () =>
                {
                    delegateTest = 2;
                    return "value 2";
                });
            dynamic bendyDynamic = bendyObject.AsDynamic();

            // Assert
            Assert.AreEqual("this is my value", bendyDynamic.MyProperty);
            Assert.AreEqual("value 2", bendyDynamic.SecondDelegate);
            Assert.AreEqual("value 1", bendyDynamic.FirstDelegate);
            Assert.AreEqual(1, delegateTest);
        }

        [Test]
        public void Bendy_CanSetAndGetEnumerableProperty()
        {
            // Arrange
            dynamic bendyObject = new BendyObject();
            var numbers = new[] { 1, 2, 3, 4, 5 };

            dynamic bendy1 = new BendyObject();
            bendy1.Blah = "blah1";
            dynamic bendy2 = new BendyObject();
            bendy2.Blah = "blah2";
            var alreadyBendy = new[] { bendy1, bendy2 };

            var bendyEnumWithLazy = new BendyObject();

            // Act
            bendyObject.Numbers = numbers;
            bendyObject.AlreadyBendy = alreadyBendy;
            bendyEnumWithLazy.AddLazy("Lazy", () => alreadyBendy);

            // Assert
            int count = 0;
            foreach (var number in bendyObject.Numbers)
            {
                count++;
                Assert.AreEqual(count, number);
            }
            Assert.AreEqual(5, count);

            int alreadyBendyCount = 0;
            foreach (var already in bendyObject.AlreadyBendy)
            {
                alreadyBendyCount++;
                //Assert.AreEqual("blah" + alreadyBendyCount, already.Blah);
            }
            Assert.AreEqual(2, alreadyBendyCount);

            dynamic lazyBendy = bendyEnumWithLazy;
            int alreadyBendyLazyCount = 0;
            foreach (var already in lazyBendy.Lazy)
            {
                alreadyBendyLazyCount++;
                Assert.AreEqual("blah" + alreadyBendyLazyCount, already.Blah);
            }
            Assert.AreEqual(2, alreadyBendyLazyCount);
        }

        [Test]
        public void Bendy_CanSetAndGetProperty()
        {
            // Arrange
            dynamic bendyObject = new BendyObject();

            // Act
            bendyObject.MyProperty = "this is my value";

            // Assert
            Assert.AreEqual("this is my value", bendyObject.MyProperty);
        }

        [Test]
        public void Bendy_ReturnsExplicitNull_IfPropertyNotFound()
        {
            // Arrange
            dynamic bendyObject = new BendyObject();

            // Act & Assert
            Assert.IsTrue(bendyObject.MyUnsetProperty == null, "MyUnsetProperty is not null; explicit null check should be true");
        }

        [Test]
        public void Bendy_ReturnsImplicitString_IfPropertyNotFound_DuringMethodCall()
        {
            // Arrange
            dynamic bendyObject = new BendyObject();

            // Act & Assert
            Assert.That(bendyObject.MyUnsetProperty, Is.EqualTo(string.Empty), "MyUnsetProperty is not equal to string.Empty");
            Assert.That(bendyObject.MyUnsetProperty, Is.Not.Null, "MyUnsetProperty is null during impicit check, should be string.Empty");
        }

        [Test]
        public void Bendy_CanImplicitlyCastToString_IfPropertyNotFound()
        {
            // Arrange
            dynamic bendyObject = new BendyObject();

            // Act
            bendyObject.MyProperty = "this is my value";

            // Assert
            Assert.AreEqual("this is my value", bendyObject.MyProperty);
            var myUnsetProperty = bendyObject.MyUnsetProperty;
            Assert.IsTrue(myUnsetProperty == null, "MyUnsetProperty is not null");
            Assert.IsTrue(IrrelevantTestMethod(myUnsetProperty), "MyUnsetProperty with variable reference is not null");
        }

        [Test]
        public void Bendy_CanImplicitlyCastToString_OnMethodCall_IfPropertyNotFound()
        {
            // Arrange
            dynamic bendyObject = new BendyObject();

            // Act & Assert
            Assert.IsTrue(IrrelevantTestMethod(bendyObject.MyUnsetProperty), "MyUnsetProperty as direct method call is not null");
        }

        [Test]
        public void MultipleRequestsForSameUnsetPropertyReturnSameValue()
        {
            // Arrange
            dynamic bendy = new BendyObject();

            // Act
            string myUnsetProperty = bendy.MyUnsetProperty;
            string myUnsetPropertySecondAccess = bendy.MyUnsetProperty;

            // Assert
            Assert.That(myUnsetProperty, Is.EqualTo(myUnsetPropertySecondAccess));
        }

        [Test]
        public void MultipleRequestsForSameSetPropertyReturnSameValue()
        {
            // Arrange
            dynamic bendy = new BendyObject();

            // Act
            bendy.MySetProperty = "a value";
            var mySetProperty = bendy.MySetProperty;
            var mySetPropertySecondAccess = bendy.MySetProperty;

            // Assert
            Assert.That(mySetProperty, Is.EqualTo(mySetPropertySecondAccess));
            Assert.That(mySetProperty, Is.SameAs(mySetPropertySecondAccess));
        }

        //[Test]
        //public void Bendy_CanImplicitlyCastToString_AsVariable_ThenOnMethodCall_IfPropertyNotFound()
        //{
        //    // Arrange
        //    dynamic bendyObject = new BendyObject();

        //    // Act & Assert

        //    // Due to the way the Microsoft.CSharp.RuntimeBinder resolves casts and conversions,
        //    // assigning MyUnsetProperty to a variable first (even if implicitly typed) will correctly
        //    // use the implicit string conversion operator on BendyObject; passing in a method call to another method will not
        //    var myUnsetProperty = bendyObject.MyUnsetProperty;
        //    var myUnsetPropertySecondAccess = bendyObject.MyUnsetProperty;

        //    Assert.That(myUnsetProperty, Is.EqualTo(myUnsetPropertySecondAccess));

        //    Assert.IsTrue(myUnsetProperty == null, "MyUnsetProperty is not null");
        //    Assert.IsTrue(IrrelevantTestMethod(myUnsetProperty), "MyUnsetProperty with variable reference is not null");
        //    Assert.IsTrue(IrrelevantTestMethod(bendyObject.MyUnsetProperty), "MyUnsetProperty as direct method call is not null");
        //}

        private static bool IrrelevantTestMethod(string aStringParameter)
        {
            return aStringParameter != null && aStringParameter == string.Empty;
        }

        [Test]
        public void Bendy_CanSetAndGetProperty_ByIndexer()
        {
            // Arrange
            dynamic bendyObject = new BendyObject();

            // Act
            bendyObject["MyProperty"] = "this is my value";

            // Assert
            Assert.AreEqual("this is my value", bendyObject.MyProperty);
            Assert.AreEqual("this is my value", bendyObject["MyProperty"]);
        }

        [Test]
        public void Bendy_CanSetAndGetDeepProperty_ByIndexer()
        {
            // Arrange
            dynamic bendyObject = new BendyObject();

            // Act
            bendyObject["MyFirstProperty"] = "this is my first value";
            bendyObject["MySecondProperty"]["MyValue"] = "this is my first nested value";
            bendyObject["MyThirdProperty"] = new KeyValuePair<string, string>("fake key", "fake value");

            // Assert
            Assert.AreEqual("this is my first value", bendyObject.MyFirstProperty);
            Assert.AreEqual("this is my first value", bendyObject["MyFirstProperty"]);
            Assert.AreEqual("this is my first nested value", bendyObject.MySecondProperty.MyValue);
            Assert.AreEqual("this is my first nested value", bendyObject["MySecondProperty"]["MyValue"]);
            Assert.AreEqual("this is my first nested value", bendyObject["MySecondProperty"].MyValue);
            Assert.AreEqual("fake key", bendyObject.MyThirdProperty.Key);
            Assert.AreEqual("fake key", bendyObject["MyThirdProperty"].Key);
            Assert.AreEqual("fake value", bendyObject.MyThirdProperty.Value);
            Assert.AreEqual("fake value", bendyObject["MyThirdProperty"].Value);
        }

        [Test]
        public void Bendy_CanSetAndGetDeepProperty()
        {
            // Arrange
            dynamic bendyObject = new BendyObject();

            // Act
            bendyObject.MyFirstProperty = "this is my first value";
            bendyObject.MySecondProperty.MyValue = "this is my first nested value";
            bendyObject.MyThirdProperty = new KeyValuePair<string, string>("fake key", "fake value");

            // Assert
            Assert.AreEqual("this is my first value", bendyObject.MyFirstProperty);
            Assert.AreEqual("this is my first nested value", bendyObject.MySecondProperty.MyValue);
            Assert.AreEqual("fake key", bendyObject.MyThirdProperty.Key);
            Assert.AreEqual("fake value", bendyObject.MyThirdProperty.Value);
        }



        [Test]
        public void WhenBendyIsEmpty_CanExplicitlyConvertToEmptyString()
        {
            // Arrange
            dynamic nullHelper = new BendyObject();

            // Act
            string converted = nullHelper;

            // Assert
            Assert.That(converted, Is.EqualTo(string.Empty));
        }

        [Test]
        public void WhenBendyIsEmpty_CanExplicitlyConvertToInteger()
        {
            // Arrange
            dynamic nullHelper = new BendyObject();

            // Act & Assert
            CheckExplicitTypeAndAssert<int>(nullHelper);
        }

        [Test]
        public void WhenBendyIsEmpty_CanExplicitlyConvertToNullableInteger()
        {
            // Arrange
            dynamic nullHelper = new BendyObject();

            // Act & Assert
            CheckExplicitNonNullableTypeAndAssert<int>(nullHelper);
        }

        [Test]
        public void WhenBendyIsEmpty_CanExplicitlyConvertToBool()
        {
            // Arrange
            dynamic nullHelper = new BendyObject();

            // Act & Assert
            CheckExplicitTypeAndAssert<bool>(nullHelper);
        }

        [Test]
        public void WhenBendyIsEmpty_CanExplicitlyConvertToDecimal()
        {
            // Arrange
            dynamic nullHelper = new BendyObject();

            // Act & Assert
            CheckExplicitTypeAndAssert<decimal>(nullHelper);
        }

        [Test]
        public void WhenBendyIsEmpty_CanConvertToAnyValueType()
        {
            // Arrange
            dynamic nullHelper = new BendyObject();
            var valueTypes = TypeFinder.GetAllAssemblies()
                .SelectMany(x => x.GetTypes().Where(type => type.IsPrimitive && !type.IsEnum))
                .Distinct();

            // Act & Assert
            foreach (var valueType in valueTypes)
            {
                CheckConvertTypeAndAssert(valueType, nullHelper);
            }
        }

        private static void CheckExplicitTypeAndAssert<T>(BendyObject nullHelper)
        {
            // Act
            var converted = (T)(dynamic)nullHelper;

            // Assert
            Assert.That(converted, Is.EqualTo(default(T)));
        }

        private static void CheckExplicitNonNullableTypeAndAssert<T>(BendyObject nullHelper)
            where T : struct
        {
            // Act
            var converted = (T?)(dynamic)nullHelper;

            // Assert
            Assert.That(converted.HasValue, Is.EqualTo(false));
        }

        private static void CheckConvertTypeAndAssert(Type destinationType, dynamic nullHelper)
        {
            // Act
            var conversionResult = ObjectExtensions.TryConvertTo(nullHelper, destinationType);

            // Assert
            var defaultValue = destinationType.GetDefaultValue();
            Assert.That(defaultValue, Is.Not.Null, "Type " + destinationType.Name + " should be excluded from tests as its default value is null");
            Assert.That(conversionResult.Success, Is.EqualTo(true), "Could not convert DynamicNullableValueObject to " + destinationType.Name);
            Assert.That(conversionResult.Result, Is.EqualTo(defaultValue));
        }
    }
}