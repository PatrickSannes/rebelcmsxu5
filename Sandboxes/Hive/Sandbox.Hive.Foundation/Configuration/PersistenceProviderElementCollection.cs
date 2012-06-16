using System.Configuration;

namespace Sandbox.Hive.Foundation.Configuration
{
    [ConfigurationCollection(typeof(PersistenceProviderElement), AddItemName = "provider")]
  public class PersistenceProviderElementCollection : Configuration.ConfigurationElementCollection<string, PersistenceProviderElement>
  {
    public override ConfigurationElementCollectionType CollectionType
    {
      get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
    }

    protected override string ElementName
    {
      get { return "persistence-providers"; }
    }

    protected override string GetElementKey(PersistenceProviderElement element)
    {
      return element.Key;
    }
  }
}