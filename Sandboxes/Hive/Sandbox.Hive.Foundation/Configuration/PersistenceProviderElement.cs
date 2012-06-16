using System;
using System.Configuration;

namespace Sandbox.Hive.Foundation.Configuration
{
  public class PersistenceProviderElement : PersistenceTypeLoaderElementBase
  {
    private const string XReaderKey = "reader";
    private const string XReadWritersKey = "read-writers";

    #region Properties

    [ConfigurationProperty(XReaderKey)]
    public PersistenceReaderElement Reader
    {
      get
      {
        var readerElement = (PersistenceReaderElement)this[XReaderKey];
        readerElement.Parent = this;
        return readerElement;
      }

      set { this[XReaderKey] = value; }
    }

    [ConfigurationProperty(XReadWritersKey)]
    public PersistenceReadWriterElementCollection ReadWriters
    {
      get
      {
        var writerElementCollection = (PersistenceReadWriterElementCollection)this[XReadWritersKey];
        writerElementCollection.Parent = this;
        return writerElementCollection;
      }

      set { this[XReadWritersKey] = value; }
    }

    #endregion


  }
}