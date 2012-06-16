using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandbox.Hive.Foundation
{
  public interface IDependencyResolver
  {
    T Resolve<T>();

    T Resolve<T>(string name);

    object Resolve(Type type);

    object Resolve(Type type, string name);

    /// <summary>
    /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
    /// </summary>
    /// <remarks>The <typeparamref name="T"/> type parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <returns>A tuple indicating success and the resolved value, if any.</returns>
    ResolutionAttemptTuple<T> TryResolve<T>() where T : class;

    /// <summary>
    /// Attempts to resolve a type, failing silently if the resolution cannot be performed.
    /// </summary>
    /// <remarks>The <typeparamref name="T"/> type parameter must be a reference (class) object in order to assess null resolutions predictably.</remarks>
    /// <param name="name">The name of the service to resolve.</param>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <returns>A tuple indicating success and the resolved value, if any.</returns>
    ResolutionAttemptTuple<T> TryResolve<T>(string name) where T : class;
  }

  public class ResolutionAttemptTuple<T> where T : class
  {
    private readonly Tuple<bool, T> _tuple = null;

    public ResolutionAttemptTuple(bool item1, T item2)
    {
      _tuple = new Tuple<bool, T>(item1, item2);
    }

    /// <summary>
    /// The success of the resolution attempt.
    /// </summary>
    public bool Success { get { return _tuple.Item1; } }

    /// <summary>
    /// The resulting value of the resolution attempt, if any.
    /// </summary>
    public T Value
    {
      get { return _tuple.Item2; }
    }

    public override string ToString()
    {
      return _tuple.ToString();
    }
  }
}
