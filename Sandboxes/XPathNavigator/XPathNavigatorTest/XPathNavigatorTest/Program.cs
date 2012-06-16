using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using XsltViewEngine;

namespace XPathNavigatorTest
{
    class Program
    {
        static Random  _random = new Random(1337);

        static void Main(string[] args)
        {
            var v1 = new Vertex();

            AddSomeProperties(v1);
                        
            for (int i = 0; i < 5; i++)
            {                
                v1.Children.Add(
                    AddSomeProperties(new Vertex { Name = "Child " + i, ID = "10" }));
            }            

            var xslt = new XslCompiledTransform();

            //You need to change the path
            xslt.Load("Test.xslt");

            

            var nav = new ObjectNavigator(v1.GetElementDescriptor());
            

            var xsltOld = new XslTransform();
            xsltOld.Load("Test.xslt");

            /*xslt.Transform(nav,
                new XsltArgumentList(), Console.Out);*/

            var xsltArgs = new XsltArgumentList();
            xsltArgs.AddExtensionObject("urn:test", new Loader());

            xsltOld.Transform(nav, xsltArgs, Console.Out);


            Console.In.ReadLine();
        }

        static Vertex AddSomeProperties(Vertex v)
        {
            for (int i = 0, n = _random.Next(0, 5); i < n; i++)
            {
                var prop = new Property { Name = "Prop " + i, Value = "Value " + i };
                v.Properties.Add(prop.Name, prop);
            }
            return v;
        }
        
        class Loader
        {
            public XPathNodeIterator Load(XPathNodeIterator o, string couldBeXPath)
            {

                var refVertex = (Vertex) ((ObjectNavigator)o.Current).For;                
                var vertex = new Vertex { Name = "Vertex for " + couldBeXPath };                
                for (int i = 0; i < 10; i++)
                {
                    var child = new Vertex { Name = "Child " + i + " for " + couldBeXPath };
                    vertex.Children.Add(child);
                    child.Properties.Add("Reference", new Property { Name = "Reference", Value = refVertex.Name });
                }


                return new ObjectNavigator(vertex.GetElementDescriptor()).Select("Vertex");
            }
        }
    }
}
