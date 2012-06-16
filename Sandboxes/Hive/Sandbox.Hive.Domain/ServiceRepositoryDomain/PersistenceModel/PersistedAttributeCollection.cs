using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel
{
  public class PersistedAttributeCollection : KeyedCollection<string, PersistedAttribute>, IDictionary<string, PersistedAttribute>  
  {
    protected override string GetKeyForItem(PersistedAttribute item)
    {
      return item.Key;
    }

    public IEnumerator<KeyValuePair<string, PersistedAttribute>> GetEnumerator()
    {
      return base.Dictionary.GetEnumerator();
    }

    public void Add(KeyValuePair<string, PersistedAttribute> item)
    {
      base.Dictionary.Add(item);
    }

    public bool Contains(KeyValuePair<string, PersistedAttribute> item)
    {
      return base.Dictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, PersistedAttribute>[] array, int arrayIndex)
    {
      base.Dictionary.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, PersistedAttribute> item)
    {
      return base.Dictionary.Remove(item);
    }

    public bool IsReadOnly
    {
      get { return base.Dictionary.IsReadOnly; }
    }

    public bool ContainsKey(string key)
    {
      return base.Dictionary.ContainsKey(key);
    }

    public void Add(string key, PersistedAttribute value)
    {
      base.Dictionary.Add(key, value);
    }

    public bool TryGetValue(string key, out PersistedAttribute value)
    {
      return base.Dictionary.TryGetValue(key, out value);
    }

    public PersistedAttribute this[string key]
    {
      get { return base.Dictionary[key]; }
      set { base.Dictionary[key] = value; }
    }

    public ICollection<string> Keys
    {
      get { return base.Dictionary.Keys; }
    }

    public ICollection<PersistedAttribute> Values
    {
      get { return base.Dictionary.Values; }
    }
  }
}