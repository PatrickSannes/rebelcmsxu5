using System;
using System.Linq.Expressions;

namespace Umbraco.Framework.DependencyManagement
{
    public interface IDependencyRegistrar<T>
    {
        IDependencyRegistrar<T> KnownAs(Type type);
        IDependencyRegistrar<T> KnownAs<TContract>();
        IDependencyRegistrar<T> Named<TContract>(string name);
        IDependencyRegistrar<T> Named(string name, Type type);
        IScopeManager<T> ScopedAs { get; }
        Type RawType { get; }
        IDependencyRegistrar<T> OnActivated(Action<IResolutionContext, T> action);
        IDependencyRegistrar<T> WithResolvedParam<TParam>(Func<IResolutionContext, TParam> callback);
        IDependencyRegistrar<T> WithMetadata<TMetadata, TProperty>(
            Expression<Func<TMetadata, TProperty>> propertyAccessor, TProperty value);

        IDependencyRegistrar<T> WithNamedParam(string paramterName, object value);
    }
}
