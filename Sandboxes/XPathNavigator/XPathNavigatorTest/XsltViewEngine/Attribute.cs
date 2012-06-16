using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XsltViewEngine
{
    public class Property : IElementDescriptorAdaptable
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public IElementDescriptor GetElementDescriptor()
        {
            var descr = new ElementDescriptor(this, "Property", null,
                (attr) =>
                {
                    attr("Name", () => Name);
                    attr("Value", () => Value);
                });
            return descr;
        }
    }
}
