using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UmbracoFramework
{
    public interface IVersionable
    {
        public IMemento CreateMemento;
        public void SetMemento(IMemento memento)
    }

    public interface IMemento<T>
    {
        public T State;
    }
}
