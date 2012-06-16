using System;
using System.Web;
using System.Web.Mvc;

namespace Umbraco.Tests.Extensions.ModelForCloneTests
{
    // Internal class so we can test such things in DeepCopy tests
    [Serializable]
    internal sealed class CloneTestBase : ICloneInterfaceTest
    {
        public string String { get; protected set; }

        public void TestSetString(string newValue)
        {
            String = newValue;
        }
    }
}
