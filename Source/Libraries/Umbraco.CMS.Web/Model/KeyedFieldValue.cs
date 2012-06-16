namespace Umbraco.Cms.Web.Model
{
    public class KeyedFieldValue
    {
        public KeyedFieldValue(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public object Value { get; set; }
    }
}