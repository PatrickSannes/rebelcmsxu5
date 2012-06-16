using System;
using System.Linq.Expressions;

namespace Umbraco.Framework
{
    /// <summary>
    /// An interface that defines that the object is tracking property changes and if it is Dirty
    /// </summary>
    public interface ICanBeDirty
    {
        bool IsDirty();
        bool IsPropertyDirty(string propName);        
    }
}