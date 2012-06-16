using System.Collections.Generic;
using System.Configuration;

namespace Sandbox.Hive.Foundation.Configuration
{
  [ConfigurationCollection(typeof(PersistenceReadWriterElement), AddItemName = "repository")]
  public class PersistenceReadWriterElementCollection : Configuration.ConfigurationElementCollection<string, PersistenceReadWriterElement>
  {
    protected override PersistenceReadWriterElement Get(int index)
    {
      var element = base.Get(index);
      return SetParent(element);
    }

    protected override PersistenceReadWriterElement Get(string key)
    {
      var element = base.Get(key);
      return SetParent(element);
    }

    public override void Add(PersistenceReadWriterElement path)
    {
      base.Add(SetParent(path));
    }

    private PersistenceReadWriterElement SetParent(PersistenceReadWriterElement element)
    {
      element.Parent = Parent;
      element.Container = this;
      return element;
    }

    private void SetParentToAll()
    {
      foreach (PersistenceReadWriterElement readWriterElement in this)
      {
        SetParent(readWriterElement);
      }
    }

    private PersistenceProviderElement _parent;
    public PersistenceProviderElement Parent
    {
      get { return _parent; }
      set
      {
        _parent = value;
        SetParentToAll();
      }
    }

    public override ConfigurationElementCollectionType CollectionType
    {
      get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
    }

    protected override string ElementName
    {
      get { return "read-writers"; }
    }

    protected override string GetElementKey(PersistenceReadWriterElement element)
    {

      return element.Key;
    }
  }
}