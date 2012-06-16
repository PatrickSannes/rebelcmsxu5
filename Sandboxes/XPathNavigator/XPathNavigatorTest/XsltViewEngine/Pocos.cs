using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XsltViewEngine
{
    class Foo
    {
        public string Name { get; set; }

        public string Taste { get; set; }

        public IEnumerable<Bar> Bars
        {
            get
            {
                yield return new Bar { Name = "Bar 1" };
                yield return new Bar { Name = "Bar 2" };
            }
        }
    }

    class Bar
    {
        public string Name { get; set; }
    }
}
