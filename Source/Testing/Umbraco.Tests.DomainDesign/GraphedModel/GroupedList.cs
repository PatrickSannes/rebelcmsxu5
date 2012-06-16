using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Umbraco.Tests.DomainDesign.GraphedModel
{
    public class IndexedEntityMetaCollection : KeyedCollection<Entity, HashSet<IndexedEntityCollection.Meta>>
    {
        protected override Entity GetKeyForItem(HashSet<IndexedEntityCollection.Meta> item)
        {
            return item.First().Entity;
        }
    }

    public class TestEntityAndRelationshipCache2 : Dictionary<string, IndexedEntityMetaCollection>
    {
        public void Add(string groupKey, IndexedEntityCollection.Meta value)
        {

            var group = new IndexedEntityMetaCollection();
            if (!ContainsKey(groupKey))
            {
                Add(groupKey, group);
            }
            else
            {
                group = this[groupKey];
            }

            var metas = new HashSet<IndexedEntityCollection.Meta>();
            if (group.Contains(value.Entity))
            {
                metas = group[value.Entity];
            }
            if (!metas.Any(x => x.Equals(value)))
                metas.Add(value);

            if (!group.Contains(value.Entity))
            {
                group.Add(metas);
            }            
        }
    }

    public class TestEntityAndRelationshipCache : Dictionary<string, Dictionary<string, HashSet<IndexedEntityCollection.Meta>>>
    {
        public void Add(string groupKey, IndexedEntityCollection.Meta value)
        {
            
            var group = new Dictionary<string, HashSet<IndexedEntityCollection.Meta>>();
            if (!ContainsKey(groupKey))
            {
                Add(groupKey, group);
            }
            else
            {
                group = this[groupKey];
            }

            var metas = new HashSet<IndexedEntityCollection.Meta>();
            if (!group.ContainsKey(value.Entity.Id))
            {
                group.Add(value.Entity.Id, metas);
            }
            else
            {
                metas = group[value.Entity.Id];
            }
            IEnumerable<IndexedEntityCollection.Meta> checkForExisting = metas;
            if (!metas.Any(x=>x.Equals(value)))
                metas.Add(value);
        }
    }

    public abstract class GroupedList<TGroupKey, TListValue, TListValueKey> : Dictionary<TGroupKey, HashSet<TListValue>>
        where TListValueKey : IComparable
    {
        protected abstract TListValueKey GetKeyForListItem(TListValue listValue);

        public void Add(TGroupKey groupKey, TListValue value)
        {
            var group = new HashSet<TListValue>();
            if (!ContainsKey(groupKey))
            {
                Add(groupKey, group);
            }
            else
            {
                group = this[groupKey];
            }
            if (!group.Contains(value)) group.Add(value);
        }
    }

    public class GroupedEntityList : GroupedList<string, IndexedEntityCollection.Meta, string>
    {
        protected override string GetKeyForListItem(IndexedEntityCollection.Meta listValue)
        {
            return listValue.Entity.Id;
        }
    }
}