using System;
using System.Collections.Concurrent;
using System.Threading;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;

namespace Umbraco.Framework.Persistence
{
    /// <summary>
    /// A default threadsafe IAttributeTypeRegistry
    /// </summary>
    /// <remarks>>
    /// By default, this class includes all of the system AttributeType's registered with default (blank) RenderTypeProvider information
    /// </remarks>
    public class DefaultAttributeTypeRegistry : IAttributeTypeRegistry
    {
        /// <summary>
        /// Ensures all known types are registered on construcion
        /// </summary>
        public DefaultAttributeTypeRegistry()
        {
            EnsureDefaultRegistrations();
        }

        private readonly ConcurrentDictionary<string, Func<AttributeType>> _attributeTypeCache = new ConcurrentDictionary<string, Func<AttributeType>>();
        private volatile bool _initiallyRegistered = false;
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        /// <summary>
        /// Gets the AttributeType by alias
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public AttributeType GetAttributeType(string alias)
        {
            var result = TryGetAttributeType(alias);
            if (result.Success) return result.Result;
            throw new ArgumentException("The AttributeType with alias: " + alias + " has not been registered with the current IAttributeTypeRegistry");
        }

        /// <summary>
        /// Tries to get an AttributeType by alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public AttemptTuple<AttributeType> TryGetAttributeType(string alias)
        {
            EnsureDefaultRegistrations();

            Func<AttributeType> at;
            if (_attributeTypeCache.TryGetValue(alias, out at))
            {
                return new AttemptTuple<AttributeType>(true, at());
            }
            return AttemptTuple<AttributeType>.False;
        }

        /// <summary>
        /// Registers or updates an AttributeType in the registry
        /// </summary>
        /// <param name="attributeType">the AttributeType.</param>
        public void RegisterAttributeType(Func<AttributeType> attributeType)
        {  

            //get the alias, this requires a call to the method
            var at = attributeType();

            _attributeTypeCache.AddOrUpdate(at.Alias,
                                            attributeType,
                                            (k, a) => attributeType);
        }

        /// <summary>
        /// Ensures all known types are registered
        /// </summary>
        private void EnsureDefaultRegistrations()
        {
            if (!_attributeTypeCache.IsEmpty) return;
            RegisterDefaultTypes();            
        }

        /// <summary>
        /// Called one time to register default types for the class instance before
        /// any AttributeType's get resolved or Registered.
        /// </summary>
        protected virtual void RegisterDefaultTypes()
        {
            RegisterSystemAttributeTypes();
        }

        /// <summary>
        /// Called by RegisterDefaultTypes to register all build in system types
        /// </summary>
        protected virtual void RegisterSystemAttributeTypes()
        {
            RegisterAttributeType(() => new StringAttributeType());
            RegisterAttributeType(() => new TextAttributeType());
            RegisterAttributeType(() => new IntegerAttributeType());
            RegisterAttributeType(() => new DateTimeAttributeType());
            RegisterAttributeType(() => new BoolAttributeType());
            RegisterAttributeType(() => new ReadOnlyAttributeType());
            RegisterAttributeType(() => new ContentPickerAttributeType());
            RegisterAttributeType(() => new MediaPickerAttributeType());
            RegisterAttributeType(() => new ApplicationsListPickerAttributeType());
            RegisterAttributeType(() => new NodeNameAttributeType());
            RegisterAttributeType(() => new SelectedTemplateAttributeType());
            RegisterAttributeType(() => new UserGroupUsersAttributeType());
            RegisterAttributeType(() => new FileUploadAttributeType());
            RegisterAttributeType(() => new DictionaryItemTranslationsAttributeType());
        }
    }
}