
using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Tests.DependencyOperations.IoCStubs
{
	public class MyClassImplementingInterfaceWithMetadata : IMyInterface
	{
	    private IEnumerable<IMyMetadata> _instantiationMetadata;

	    public MyClassImplementingInterfaceWithMetadata()
		{
			
		}

		public MyClassImplementingInterfaceWithMetadata(string myParam)
		{
			MyStringProperty = myParam;
		}

		public MyClassImplementingInterfaceWithMetadata(IMyParamTypeInterface myService)
		{
			MyService = myService;
		}

		public MyClassImplementingInterfaceWithMetadata(IEnumerable<Lazy<IMyInterface, IMyMetadata>> metadata)
		{
		    _instantiationMetadata = metadata.Select(x=>x.Metadata);
		    MyStringProperty = _instantiationMetadata.First().StringValue;

		}

	    public string MyStringProperty { get; set; }

        public Guid MyGuidProperty { get; set; }

        public int MyIntProperty { get; set; }

	    public IMyParamTypeInterface MyService { get; set; }
	}

    public class MyClassAcceptingResolvedMetadata
    {
        private IEnumerable<IMyMetadata> _metadata;

        public MyClassAcceptingResolvedMetadata(IEnumerable<Lazy<IMyInterface, IMyMetadata>> lazyMetadata)
        {
            _metadata = lazyMetadata.Select(x => x.Metadata);
        }
    }

	public interface IMyMetadata
	{
		string StringValue { get; }
	    int IntValue { get; }
	    Guid GuidValue { get; }
	}
}