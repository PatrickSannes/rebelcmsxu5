using System.Collections.Generic;

namespace Umbraco.Cms.Web
{
    /// <summary>
    /// A simple interface to resolve a list of items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IResolver<out T>
    {
        IEnumerable<T> Resolve();
    }
}