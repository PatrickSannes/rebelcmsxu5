using System;

namespace Umbraco.Tests.DependencyOperations.IoCStubs
{
    public class MyClassImplementingInterface : IMyInterface
    {
        public MyClassImplementingInterface()
        {

        }

        public MyClassImplementingInterface(string myParam)
        {
            MyStringProperty = myParam;
        }

        public MyClassImplementingInterface(IMyParamTypeInterface myService)
        {
            MyService = myService;
        }

        public string MyStringProperty { get; set; }

        public Guid MyGuidProperty { get; set; }

        public int MyIntProperty { get; set; }

        public IMyParamTypeInterface MyService { get; set; }
    }
}