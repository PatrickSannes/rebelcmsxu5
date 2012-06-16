using System.Configuration;

namespace Sandbox.Hive.Foundation.Configuration
{
  public class PersistenceTypeLoaderElementBase : ConfigurationElement
  {
    private const string XKeyKey = "key";
    private const string XTypeKey = "type";

    [ConfigurationProperty(XKeyKey)]
    public string Key
    {
      get { return (string)this[XKeyKey]; }

      set { this[XKeyKey] = value; }
    }

    [ConfigurationProperty(XTypeKey)]
    public string Type
    {
      get { return (string)this[XTypeKey]; }

      set { this[XTypeKey] = value; }
    }

    public string InternalKey { get; set; }
  }
}