using System;

namespace Umbraco.Tests.DependencyOperations.IoCStubs
{
    public interface IMyInterface
    {
        string MyStringProperty { get; set; }
        Guid MyGuidProperty { get; set; }
        int MyIntProperty { get; set; }
        IMyParamTypeInterface MyService { get; set; }
    }
}
