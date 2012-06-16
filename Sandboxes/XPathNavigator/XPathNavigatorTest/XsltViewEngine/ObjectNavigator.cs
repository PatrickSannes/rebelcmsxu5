using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using System.Xml;

namespace XsltViewEngine
{
    


    public class ObjectNavigator : XPathNavigator
    {

        public object For
        {
            get { return _state.Element.For; }
        }

        NavigatorState _state;

        NameTable _nameTable = new NameTable();

        public ObjectNavigator(IElementDescriptorAdaptable obj)
            : this(obj.GetElementDescriptor())
        {

        }

        public ObjectNavigator(IElementDescriptor element)
        {
            _state = new NavigatorState(element);
            _nameTable.Add(element.Name);
        }        

        
        public override string BaseURI
        {
            get { return ""; }
        }

        public override XPathNavigator Clone()
        {            
            var clone = (ObjectNavigator)this.MemberwiseClone();
            clone._state = _state.Clone();
            return clone;
        }

        public override bool IsEmptyElement
        {
            get { return string.IsNullOrEmpty(_state.Element.Text) && _state.Element.FirstChild == null; }
        }

        public override bool IsSamePosition(XPathNavigator nav)
        {
            var other = nav as ObjectNavigator;
            if (other != null)
            {
                return other._state.Equals(_state);
            }
            return false;
        }

        public override string Name
        {
            get
            {
                switch (_state.NodeType)
                {
                    case XPathNodeType.Element:
                        return _state.Element.Name;
                    case XPathNodeType.Attribute:
                        return _state.Attributes.Current.Key;
                    case XPathNodeType.Text:
                        return _state.Element.Text;
                    default:
                        return "";
                }
            }
        }


        public override string LocalName
        {
            get
            {
                return Name;
            }
        }    

        public override bool MoveTo(XPathNavigator nav)
        {
            var other = nav as ObjectNavigator;
            if (other != null)
            {
                _state = other._state.Clone();
            }
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            _state.Attributes.Reset();
            if (_state.Attributes.MoveNext())
            {
                _state.NodeType = XPathNodeType.Attribute;
                return true;
            }
            return false;
        }

        public override bool MoveToFirstChild()
        {
            if (_state.Element.FirstChild != null)
            {
                return MoveToElement(_state.Element.FirstChild());
            }
            return false;
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToId(string id)
        {
            return false;
        }

        public override bool MoveToNext()
        {
            return MoveToElement(_state.Element.NextSibling);
        }

        public override bool MoveToNextAttribute()
        {
            return _state.Attributes.MoveNext();
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToParent()
        {
            if (_state.Element.Parent != null)
            {
                return MoveToElement(_state.Element.Parent());
            }
            return false;
        }

        public override bool MoveToPrevious()
        {
            return MoveToElement(_state.Element.PreviousSibling);
        }

        
        public override System.Xml.XmlNameTable NameTable
        {
            get { return _nameTable; }
        }

        public override string NamespaceURI
        {
            get { return ""; }
        }

        public override XPathNodeType NodeType
        {
            get { return _state.NodeType; }
        }

        public override string Prefix
        {
            get { return ""; }
        }

        public override string Value
        {
            get
            {
                if (_state.Attributes.HasCurrent)
                {
                    return _state.Attributes.Current.Value();
                }

                return _state.Element.Text;
            }
        }
        

        bool MoveToElement(IElementDescriptor element)
        {
            if (element != null)
            {
                _state = new NavigatorState(element);
                return true;
            }
            return false;
        }

        class NavigatorState
        {
            public IElementDescriptor Element { get; set; }

            private StateAwareEnumerator<IElementDescriptor, KeyValuePair<string, Func<string>>> _attributes;
            public StateAwareEnumerator<IElementDescriptor, KeyValuePair<string, Func<string>>> Attributes {
                get
                {
                    if (_attributes == null)
                    {
                        _attributes = new StateAwareEnumerator<IElementDescriptor, KeyValuePair<string, Func<string>>>(
                            Element, Element.Attributes());
                    }
                    return _attributes;
                }
            }

            public XPathNodeType NodeType { get; set; }

            public NavigatorState(IElementDescriptor element)
            {
                Element = element;
                NodeType = XPathNodeType.Element;
                             
            }

            public NavigatorState Clone()
            {
                var clone = (NavigatorState)this.MemberwiseClone();
                if (_attributes != null)
                {
                    clone._attributes = _attributes.Clone();
                }
                return clone;
            }

            public override bool Equals(object obj)
            {
                var other = obj as NavigatorState;
                if (other != null)
                {                    
                    return other.Element.Equals(other.Element) &&  
                        other.Attributes.Equals(Attributes) &&                        
                        other.NodeType == other.NodeType;
                }
                return base.Equals(obj);
            }
                  
        }

        public class StateAwareEnumerator<TEqualRef, TItem>
        {
            int _position = -1;
            List<TItem> _items;
            TEqualRef _equalRef;
            public TItem Current
            {
                get { return HasCurrent ? _items[_position] : default(TItem); }
            }

            public bool HasCurrent
            {
                get { return _position >= 0 && _position < _items.Count; }
            }

            public StateAwareEnumerator(TEqualRef equalRef, IEnumerable<TItem> items)
            {
                _equalRef = equalRef;
                _items = items.ToList();
            }
           

            public void Reset()
            {
                _position = -1;
            }

            public bool MovePrevious()
            {
                if (_position > 0)
                {
                    return --_position > 0;
                }
                return false;
            }
            public bool MoveNext()
            {
                if (_position < _items.Count)
                {
                    return _position++ < _items.Count;
                }
                return false;
            }

            public StateAwareEnumerator<TEqualRef, TItem> Clone()
            {
                return (StateAwareEnumerator<TEqualRef, TItem>) this.MemberwiseClone();
            }

            public override bool Equals(object obj)
            {
                var other = obj as StateAwareEnumerator<TEqualRef, TItem>;
                if (other != null)
                {
                    return other._equalRef.Equals(_equalRef) &&
                        other._position == _position;
                }
                return base.Equals(obj);
            }
        }
    }
}
