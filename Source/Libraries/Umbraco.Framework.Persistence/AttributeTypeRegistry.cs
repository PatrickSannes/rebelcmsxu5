using System.Threading;

namespace Umbraco.Framework.Persistence
{
    /// <summary>
    /// A singleton class to resolve the current IAttributeTypeFactory
    /// </summary>
    public static class AttributeTypeRegistry
    {
        /// <summary>
        /// Static constructor sets default registry
        /// </summary>
        static AttributeTypeRegistry()
        {
            _currentRegistry = new DefaultAttributeTypeRegistry();
        }

        /// <summary>
        /// Sets the passed in IAttributeTypeRegistry to be the current one resolved from the Current property
        /// </summary>
        /// <param name="attributeTypeRegistry"></param>
        public static void SetCurrent(IAttributeTypeRegistry attributeTypeRegistry)
        {
            using (new WriteLockDisposable(Locker))
            {
                _currentRegistry = attributeTypeRegistry;    
            }            
        }

        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        private static volatile IAttributeTypeRegistry _currentRegistry;

        /// <summary>
        /// Returns the currently registered IAttributeTypeRegistry
        /// </summary>
        public static IAttributeTypeRegistry Current
        {
            get
            {                
                return _currentRegistry;
            }
        }
    }
}