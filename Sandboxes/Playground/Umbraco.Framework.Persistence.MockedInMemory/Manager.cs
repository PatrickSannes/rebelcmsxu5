using Umbraco.Framework.Persistence.ProviderSupport;

namespace Umbraco.Framework.Persistence.MockedInMemory
{
  public class Manager : IPersistenceManager 
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:Provider"/> class.
    /// </summary>
    public Manager(string @alias, IPersistenceReadWriter reader, IPersistenceReadWriter readWriter)
    {
      //TODO: Implement provider manifest so Hive bootstrapper can provider repository instances
      //based on standard configuration
      _alias = alias;
      _reader = reader;
      _readWriter = readWriter;
    }

    private string _alias;

    private IPersistenceReadWriter _reader;

    private IPersistenceReadWriter _readWriter;

    /// <summary>
    /// Gets or sets the alias of the provider.
    /// </summary>
    /// <value>The key.</value>
    /// <remarks></remarks>
    public string Alias
    {
      get { return _alias; }
      set { _alias = value; }
    }

    /// <summary>
    /// Gets or sets the read-only repository.
    /// </summary>
    /// <value>The reader.</value>
    /// <remarks></remarks>
    public IPersistenceReadWriter Reader
    {
      get { return _reader; }
      set { _reader = value; }
    }

    /// <summary>
    /// Gets or sets the repository which can read and write entities.
    /// </summary>
    /// <value>The read writer.</value>
    /// <remarks></remarks>
    public IPersistenceReadWriter ReadWriter
    {
      get { return _readWriter; }
      set { _readWriter = value; }
    }
  }
}
