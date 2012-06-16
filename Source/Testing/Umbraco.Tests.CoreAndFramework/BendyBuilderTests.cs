using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Testing.PartialTrust;

namespace Umbraco.Tests.CoreAndFramework
{
    [TestFixture]
    public class BendyBuilderTests
    {
        [Test]
        public void WhenCreatingBendy_HavingParameterlessVoidMethod_MethodIsInvokable()
        {
            // Arrange
            var builder = new BendyBuilder();

            // Act
            builder.AutoCreateMethod<string, string>("MyNonExistantMethodReturnsParamUnchanged", (containerBendy, x) => x);
            var bendy = builder.ToBendy();
            dynamic bendyDynamic = bendy.AsDynamic();

            // Assert
            Assert.That(bendyDynamic.MyNonExistantMethodReturnsParamUnchanged("alex"), Is.EqualTo("alex"));
            Assert.That(bendyDynamic.MyNonExistantMethodReturnsParamUnchanged("different"), Is.EqualTo("different"));
        }

        [Test]
        [Description("This tests that dynamic methods which return other BendyObjects with their own dynamic methods can be chained")]
        public void NestingBendies_WithMethodDeclarations_Behaves()
        {
            // Arrange
            var parentBuilder = new BendyBuilder();
            var childBuilder = new BendyBuilder();

            // Act
            childBuilder.AutoCreateMethod<string, string>("Hiya", (containerBendy, x) => "alex" + x);
            parentBuilder.AutoCreateMethod("GetChild", (containerBendy) => childBuilder.ToBendy());
            dynamic asDynamic = parentBuilder.ToBendy().AsDynamic();

            // Assert
            Assert.That(asDynamic.GetChild(), Is.InstanceOf<BendyObject>());
            Assert.That(asDynamic.GetChild().Hiya("bob"), Is.EqualTo("alexbob"));
        }

        [Test]
        [Description("Registers two methods against a Bendy with the same parameter count, but different parameter types. Asserts that the correct 'overload' is instantiated")]
        public void WhenCreatingBendy_HavingTwoParameterlessVoidMethod_CorrectMethodIsInvoked()
        {
            // Arrange
            var builder = new BendyBuilder();

            // Act
            builder.AutoCreateMethod<string, string>("MyNonExistantMethodReturnsParamUnchanged", (containerBendy, x) => x);
            builder.AutoCreateMethod<int, int>("MyNonExistantMethodReturnsParamUnchanged", (containerBendy, x) => 5);
            var bendy = builder.ToBendy();
            dynamic bendyDynamic = bendy.AsDynamic();


            // Assert
            Assert.That(bendyDynamic.MyNonExistantMethodReturnsParamUnchanged("alex"), Is.EqualTo("alex"));
            Assert.That(bendyDynamic.MyNonExistantMethodReturnsParamUnchanged(0), Is.EqualTo(5));
        }

        [Test]
        public void WhenRunningMethod_ContainerBendy_IsPassedToDelegate()
        {
            // Arrange
            var builder = new BendyBuilder();
            BendyObject _outer = null;

            // Act
            builder.CreateMethod("MyMethod", (containerBendy) => SetRef(ref _outer, ref containerBendy));
            dynamic bendyDynamic = builder.ToBendy().AsDynamic();
            bendyDynamic.MyMethod();

            // Assert
            Assert.That(_outer, Is.Not.Null);
        }

        [Test]
        public void WhenCreatingSignature_WithNoParameters_AndNoReturnValue_SignatureHasName()
        {
            // Arrange
            var builder = new BendyBuilder();

            // Act
            var method = builder.CreateMethod("MyMethod", (containerBendy) => DoNothing());

            // Assert
            Assert.That(method.Signature.Name, Is.EqualTo("MyMethod"));
            Assert.That(method.Signature.ReturnsValue, Is.EqualTo(false));
            Assert.That(builder.Methods.Count(), Is.EqualTo(1));
            Assert.That(builder.Methods, Has.All.SameAs(method));
        }

        [Test]
        public void WhenCreatingSignature_WithParameters_AndStringReturnValue_SignatureHasName()
        {
            // Arrange
            var builder = new BendyBuilder();

            // Act
            var method = builder.CreateMethod<string, string>("MyMethod", (containerBendy, myParameter) => DoNothing(), "myParameter");

            // Assert
            Assert.That(method.Signature.Name, Is.EqualTo("MyMethod"));
            Assert.That(method.Signature.ReturnsValue, Is.EqualTo(true));
            Assert.That(method.Signature.ReturnType, Is.EqualTo(typeof(string)));
            Assert.That(builder.Methods.Count(), Is.EqualTo(1));
            Assert.That(builder.Methods, Has.All.SameAs(method));
        }

        [Test]
        public void WhenInferringSignatureFromLambda_WithParameters_AndStringReturnValue_SignatureHasName()
        {
            // Arrange
            var builder = new BendyBuilder();

            // Act
            var method = builder.AutoCreateMethod<string, string>("MyMethod", (containerBendy, myParameter) => "return value");

            // Assert
            Assert.That(method.Signature.Name, Is.EqualTo("MyMethod"));
            Assert.That(method.Signature.ReturnsValue, Is.EqualTo(true));
            Assert.That(method.Signature.ReturnType, Is.EqualTo(typeof(string)));
            Assert.That(method.Signature.Parameters.ToArray()[0].Name, Is.EqualTo("myParameter"));
            Assert.That(method.Signature.Parameters.ToArray()[0].Type, Is.EqualTo(typeof(string)));
            Assert.That(builder.Methods.Count(), Is.EqualTo(1));
            Assert.That(builder.Methods, Has.All.SameAs(method));
        }

        [TestCase((Type)null, "MyMethod", (Type)null, null, (Type)null, null, "void MyMethod()")]
        [TestCase(typeof(string), "MyMethod", (Type)null, null, (Type)null, null, "String MyMethod()")]
        [TestCase(typeof(string), "MyMethod", typeof(string), "myString", (Type)null, null, "String MyMethod(String myString)")]
        [TestCase(typeof(string), "MyMethod", typeof(string), "myString", typeof(int), "myInt", "String MyMethod(String myString, Int32 myInt)")]
        [TestCase((Type)null, "MyMethod", typeof(string), "myString", typeof(int), "myInt", "void MyMethod(String myString, Int32 myInt)")]
        public void Signature_ToString(Type returnType, string name, Type param1type, string param1name, Type param2type, string param2name, string shouldBeString)
        {
            var paramList = new List<Parameter>();
            if (param1type != null) paramList.Add(new Parameter(param1name, param1type));
            if (param2type != null) paramList.Add(new Parameter(param2name, param2type));

            var sig = returnType != null ? new Signature(name, returnType, paramList.ToArray()) : new Signature(name, paramList.ToArray());

            var toString = sig.ToString();
            Assert.That(toString, Is.EqualTo(shouldBeString));
        }

        [Test]
        public void WhenCreatingSignature_WithNoParameters_AndStringReturnValue_SignatureHasName()
        {
            // Arrange
            var builder = new BendyBuilder();

            // Act
            var method = builder.CreateMethod<string>("MyMethod", (containerBendy) => "hello");

            // Assert
            Assert.That(method.Signature.Name, Is.EqualTo("MyMethod"));
            Assert.That(method.Signature.ReturnsValue, Is.EqualTo(true));
            Assert.That(method.Signature.ReturnType, Is.EqualTo(typeof(string)));
        }

        [Test]
        public void WhenCreatingSignature_WithStringParameter_SignatureHasNameAndParameter()
        {
            // Arrange
            var builder = new BendyBuilder();

            // Act

            var method = builder.CreateMethod<string>("MyMethod", (containerBendy, myParameter) => DoNothing(), Param.Create<string>("myParameter"));

            // Assert
            Assert.That(method.Signature.Name, Is.EqualTo("MyMethod"));
            Assert.That(method.Signature.Parameters.ToArray()[0].Name, Is.EqualTo("myParameter"));
            Assert.That(method.Signature.Parameters.ToArray()[0].Type, Is.EqualTo(typeof(string)));
            Assert.That(method.Signature.ReturnsValue, Is.EqualTo(false));
        }

        void DoNothing()
        {
            
        }

        void SetRef(ref BendyObject field, ref BendyObject inomcing)
        {
            field = inomcing;
        }
    }
}
