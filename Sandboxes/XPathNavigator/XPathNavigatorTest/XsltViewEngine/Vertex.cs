using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XsltViewEngine
{
    
    public class Vertex : IElementDescriptorAdaptable
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public Dictionary<string, Property> Properties { get; set; }

        public Vertex()
        {            
            Children = new List<Vertex>();
            Properties = new Dictionary<string, Property>();
        }

        public Vertex Parent { get; set; }
        public List<Vertex> Children { get; set; }

        public IElementDescriptor GetElementDescriptor()
        {
            var descriptor = new ElementDescriptor(this, "Vertex",
                () => ((IEnumerable<object>)Properties.Values)
                    .Concat(Children)
                    .Concat(new [] {
                          new Foo { Name = "A foo"},
                          new Foo { Name = "Another foo"}  
                    }),
                (attr) =>
                {
                    attr("ID", () => ID);
                    attr("Name", () => Name);
                });            

            return descriptor;
        }
    }
}
