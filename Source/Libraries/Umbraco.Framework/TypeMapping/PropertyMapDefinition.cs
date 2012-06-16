namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Defines a map from a source to a target object
    /// </summary>
    public class PropertyMapDefinition
    {
        public ObjectDefinition Source { get; set; }

        public ObjectDefinition Target { get; set; }

        public PropertyDefinition SourceProp { get; set; }

        public PropertyDefinition TargetProp { get; set; }

        public PropertyMapDefinition()
        {
            this.Source = new ObjectDefinition();
            this.Target = new ObjectDefinition();
            this.SourceProp = new PropertyDefinition();
            this.TargetProp = new PropertyDefinition();
        }


    }
}