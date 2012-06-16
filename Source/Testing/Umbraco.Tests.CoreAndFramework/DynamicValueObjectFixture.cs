using System;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using Umbraco.Framework;
using Umbraco.Framework.Dynamics;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class DynamicValueObjectFixture
    {
        [Test]
        public void CanExplicitlyConvertToEmptyString()
        {
            // Arrange
            dynamic nullHelper = new DynamicNullableValueObject();

            // Act
            string converted = nullHelper;

            // Assert
            Assert.That(converted, Is.EqualTo(string.Empty));
        }

        [Test]
        public void CanExplicitlyConvertToInteger()
        {
            // Arrange
            dynamic nullHelper = new DynamicNullableValueObject();

            // Act & Assert
            CheckExplicitTypeAndAssert<int>(nullHelper);
        }

        [Test]
        public void CanExplicitlyConvertToNullableInteger()
        {
            // Arrange
            dynamic nullHelper = new DynamicNullableValueObject();

            // Act & Assert
            CheckExplicitNonNullableTypeAndAssert<int>(nullHelper);
        }

        [Test]
        public void CanExplicitlyConvertToBool()
        {
            // Arrange
            dynamic nullHelper = new DynamicNullableValueObject();

            // Act & Assert
            CheckExplicitTypeAndAssert<bool>(nullHelper);
        }

        [Test]
        public void CanExplicitlyConvertToDecimal()
        {
            // Arrange
            dynamic nullHelper = new DynamicNullableValueObject();

            // Act & Assert
            CheckExplicitTypeAndAssert<decimal>(nullHelper);
        }

        [Test]
        public void CanConvertToAnyValueType()
        {
            // Arrange
            dynamic nullHelper = new DynamicNullableValueObject();
            var valueTypes = TypeFinder.GetAllAssemblies()
                .SelectMany(x => x.GetTypes().Where(type => type.IsPrimitive && !type.IsEnum))
                .Distinct();

            // Act & Assert
            foreach (var valueType in valueTypes)
            {
                CheckConvertTypeAndAssert(valueType, nullHelper);
            }
        }

        private static void CheckExplicitTypeAndAssert<T>(DynamicNullableValueObject dynamicNullableValueObject)
        {
            // Act
            var converted = (T)(dynamic)dynamicNullableValueObject;

            // Assert
            Assert.That(converted, Is.EqualTo(default(T)));
        }

        private static void CheckExplicitNonNullableTypeAndAssert<T>(DynamicNullableValueObject dynamicNullableValueObject)
            where T : struct
        {
            // Act
            var converted = (T?)(dynamic)dynamicNullableValueObject;

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