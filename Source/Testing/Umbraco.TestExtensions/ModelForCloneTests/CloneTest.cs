using System;
using System.Web.Mvc;

namespace Umbraco.Tests.Extensions.ModelForCloneTests
{
    [Serializable]
    public class CloneTest : ICloneInterfaceTest
    {
        private CloneTest _myComplexField;
        protected string _myField = String.Empty;
        protected string _myPrivateField = String.Empty;

        public CloneTest()
        {
            StringArray = new[] {"Hello", "Two"};
        }

        public CloneTest(
            string myStringProperty,
            int myIntProperty,
            string protectedBackedProperty,
            string privateBackedProperty,
            CloneTest myDeepProperty = null,
            CloneTest publicDeep = null)
        {
            String = myStringProperty;
            Integer = myIntProperty;
            ProtectedDeep = myDeepProperty;
            PublicDeep = publicDeep;
            _myField = protectedBackedProperty;
            _myPrivateField = privateBackedProperty;
            var cloneTestBase = new CloneTestBase();
            cloneTestBase.TestSetString("clone-test-base");
            ObjByInterface = cloneTestBase;

            ViewLocationCache = null; // Leave null on purpose to test DeepCopy
        }

        public string[] StringArray { get; protected set; }
        public int Integer { get; set; }
        public CloneTest ProtectedDeep { get; protected set; }
        public CloneTest PublicDeep { get; set; }
        public ICloneInterfaceTest ObjByInterface { get; set; }
        public IViewLocationCache ViewLocationCache { get; set; }

        public CloneTest MyComplexField
        {
            get
            {
                return _myComplexField ??
                       (_myComplexField =
                        new CloneTest("is-private-field", 1, "protected-field-value", "private-field-value"));
            }
        }

        public string ProtectedFieldBackedProperty { get { return _myField; } }

        public string PrivateFieldBackedProperty { get { return _myPrivateField; } }

        #region ICloneInterfaceTest Members

        public string String { get; protected set; }

        #endregion

        public void AssertTestSetProtectedFieldBackedProperty(string newValue)
        {
            _myField = newValue;
        }


        public void AssertTestSetPrivateFieldBackedProperty(string newValue)
        {
            _myPrivateField = newValue;
        }
    }
}