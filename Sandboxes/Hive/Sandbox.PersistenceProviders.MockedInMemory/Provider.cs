using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel;

namespace Sandbox.PersistenceProviders.MockedInMemory
{
  public class Provider : IPersistenceProvider 
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="T:Provider"/> class.
    /// </summary>
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
    public IPersistenceRepository Reader
    {
      get { return _reader; }
      set { _reader = value; }
    }

    /// <summary>
    /// Gets or sets the repository which can read and write entities.
    /// </summary>
    /// <value>The read writer.</value>
    /// <remarks></remarks>
    public IPersistenceRepository ReadWriter
    {
      get { return _readWriter; }
      set { _readWriter = value; }
    }
  }
}
