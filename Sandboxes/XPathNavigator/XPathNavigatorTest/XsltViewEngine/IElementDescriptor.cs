using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XsltViewEngine
{
    public interface IElementDescriptor
    {
        object For { get; }

        string Name { get; }
        string Text { get; }
        Func<IEnumerable<KeyValuePair<string, Func<string>>>> Attributes { get; }
        Func<IElementDescriptor> Parent { get; }
        Func<IElementDescriptor> FirstChild { get; }

        //These doesn't need to be functions. The parent's list is loaded anyway
        IElementDescriptor PreviousSibling { get; }
        IElementDescriptor NextSibling { get; }
    }

    public interface IElementDescriptorAdaptable
    {
        IElementDescriptor GetElementDescriptor();
    }
    
}
