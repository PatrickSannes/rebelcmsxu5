namespace Umbraco.Framework.Expressions.Remotion
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ExpressionNodeModifierRegistry
    {
        private readonly ConcurrentDictionary<MethodInfo, Type> _customRegistry = new ConcurrentDictionary<MethodInfo, Type>();
        private static readonly Lazy<ExpressionNodeModifierRegistry> Singleton = new Lazy<ExpressionNodeModifierRegistry>(() => new ExpressionNodeModifierRegistry());

        public static ExpressionNodeModifierRegistry Current
        {
            get
            {
                return Singleton.Value;
            }
        }

        public void EnsureRegistered(MethodInfo methodInfo, Type nodeType)
        {
            _customRegistry.AddOrUpdate(methodInfo, x => nodeType, (x,y) => nodeType);
        }

        public IEnumerable<MethodInfoRegistration> Registrations
        {
            get
            {
                return _customRegistry.Select(x => new MethodInfoRegistration(x.Key, x.Value));
            }
        }

        public struct MethodInfoRegistration
        {
            public MethodInfoRegistration(MethodInfo methodInfo, Type type)
                : this()
            {
                MethodInfo = methodInfo;
                Type = type;
            }

            public MethodInfo MethodInfo { get; private set; }

            public Type Type { get; private set; }
        }
    }
}