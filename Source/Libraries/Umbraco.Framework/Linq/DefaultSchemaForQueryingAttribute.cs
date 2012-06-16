namespace Umbraco.Framework.Linq
{
    using System;

    public class DefaultSchemaForQueryingAttribute : Attribute
    {
        public string SchemaAlias { get; set; }
    }
}
