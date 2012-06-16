using System.Configuration;

namespace Sandbox.Hive.Foundation.Configuration
{
  public class PersistenceReaderElement : PersistenceTypeLoaderElementBase
  {
    public PersistenceProviderElement Parent { get; set; }
  }
}