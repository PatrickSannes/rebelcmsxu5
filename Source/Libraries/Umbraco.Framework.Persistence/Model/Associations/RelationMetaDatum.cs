using System;
using System.Linq;


namespace Umbraco.Framework.Persistence.Model.Associations
{
    public class RelationMetaDatum
    {
        public RelationMetaDatum(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(obj, this)) return true;

            var castObj = obj as RelationMetaDatum;
            if (ReferenceEquals(castObj, null)) return false;

            return castObj.Key.InvariantEquals(Key) && castObj.Value.InvariantEquals(Value);
        }

        public override int GetHashCode()
        {
            var key = Key ?? string.Empty;
            var value = Value ?? string.Empty;

            return (key + "_" + value).GetHashCode();
        }
    }
}