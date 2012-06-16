using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;
using Sandbox.Hive.Foundation;

namespace Sandbox.PersistenceProviders.NHibernate
{
  public class Provider : IPersistenceProvider 
  {
    public Provider(string @alias, IPersistenceRepository reader, IPersistenceRepository readWriter)
    {
      //TODO: Implement provider manifest so Hive bootstrapper can provider repository instances
      //based on standard configuration
      _alias = alias;
      _reader = reader;
      _readWriter = readWriter;
    }

    private string _alias;

    private IPersistenceRepository _reader;

    private IPersistenceRepository _readWriter;

    public string Alias
    {
      get { return _alias; }
      set { _alias = value; }
    }

    public IPersistenceRepository Reader
    {
      get { return _reader; }
      set { _reader = value; }
    }

    public IPersistenceRepository ReadWriter
    {
      get { return _readWriter; }
      set { _readWriter = value; }
    }
  }
}
