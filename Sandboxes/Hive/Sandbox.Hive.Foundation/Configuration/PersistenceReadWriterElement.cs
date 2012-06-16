using System.Configuration;

namespace Sandbox.Hive.Foundation.Configuration
{
  public class PersistenceReadWriterElement : PersistenceTypeLoaderElementBase
  {
    public PersistenceProviderElement Parent { get; set; }

    public PersistenceReadWriterElementCollection Container { get; set; }
  }
}