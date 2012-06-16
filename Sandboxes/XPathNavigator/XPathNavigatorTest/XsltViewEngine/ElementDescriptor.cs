using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace XsltViewEngine
{


    public class ElementDescriptor : IElementDescriptor
    {
        public string Name { get; set; }        
        public object For { get; set; }        


        public Func<IElementDescriptor> Parent { get; protected set; }
        public Func<IElementDescriptor> FirstChild { get; protected set; }
        public IElementDescriptor PreviousSibling { get; protected set; }
        public IElementDescriptor NextSibling { get; protected set; }


        Action<Action<string, Func<string>>> Attributes { get; set; }


        public ElementDescriptor(object descriptorFor, string name, 
            Func<IEnumerable<object>> children = null,
            Action<Action<string, Func<string>>> attributes = null)
        {
            For = descriptorFor;
            Name = name;
            Attributes = attributes;
            if (children != null)
            {
                FirstChild = () =>
                {
                    var childList = children().Select(x => CreateFor(x)).ToList();
                    if (childList.Any())
                    {
                        SetChildProperties(childList);
                        return childList[0];
                    }
                    else
                    {
                        return null;
                    }
                };
            }
        }

        void SetChildProperties( List<IElementDescriptor> childList)
        {
            for (int i = 0, n = childList.Count; i < n; i++)
            {
                var basicChild = childList[i] as ElementDescriptor;
                if (basicChild != null)
                {
                    basicChild.Parent = () => this;
                    if (i > 0) basicChild.PreviousSibling = childList[i - 1];
                    if (i < n - 1) basicChild.NextSibling = childList[i + 1];
                }
            }
        }


        public string Text
        {
            get { throw new NotImplementedException(); }
        }



        public static IElementDescriptor CreateFor(object o)
        {
            IElementDescriptorAdaptable adaptable = o as IElementDescriptorAdaptable;
            if (adaptable != null)
            {
                return adaptable.GetElementDescriptor();
            }
            else
            {
                var type = o.GetType();                

                var name = type.Name;

                ElementDescriptor ed = null;
                ed = new ElementDescriptor(o, name, () =>
                {
                    var children = new List<object>();
                     
                    var properties = type.GetProperties().Where(p => IsEnumerable(p.PropertyType));
                    foreach (var prop in properties)
                    {
                        var e = prop.GetValue(o, null) as IEnumerable;
                        if (e != null)
                        {
                            foreach (var item in e)
                            {
                                children.Add(item);
                            }
                        }              
                    }
                    return children;
                },
                (attr) =>
                {
                    var properties = type.GetProperties().Where(p=>!IsEnumerable(p.PropertyType));
                    
                    foreach (var prop in properties)
                    {
                        var _prop = prop; //for lambda reference

                        attr(_prop.Name, () => ""+_prop.GetValue(o, null));
                    }
                });

                return ed;
            }
        }

        static bool IsEnumerable(Type t)
        {
            return typeof(IEnumerable).IsAssignableFrom(t) && !typeof(string).IsAssignableFrom(t);
        }



        Func<IEnumerable<KeyValuePair<string, Func<string>>>> IElementDescriptor.Attributes
        {
            get
            {
                return () =>
                {
                    var attrs = new List<KeyValuePair<string, Func<string>>>();
                    if (Attributes != null)
                    {
                        Attributes((name, valueDelegate) =>
                            attrs.Add(new KeyValuePair<string, Func<string>>(name, valueDelegate)));
                    }
                    return attrs;
                };
            }
        }
    }
  
}
