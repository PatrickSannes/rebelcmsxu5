using System;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Represents a pair of delegates to assist in type mapping; one to map by reference to an existing destination object and one to
    /// provide a new result by return.
    /// </summary>
    /// <remarks></remarks>
    public class TypeMapDelegatePair
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMapDelegatePair"/> class.
        /// </summary>
        public TypeMapDelegatePair(Func<object, AbstractLookupHelper, AbstractMappingEngine, object> byReference, Action<object, object, AbstractLookupHelper, AbstractMappingEngine> byReturn)
        {
            ByReturn = byReference;
            ByReference = byReturn;
        }

        /// <summary>
        /// Gets or sets the delegate which maps a value by returning an instance of the destination type.
        /// </summary>
        /// <value>The by return.</value>
        /// <remarks></remarks>
        public Func<object, AbstractLookupHelper, AbstractMappingEngine, object> ByReturn { get; protected set; }

        /// <summary>
        /// Gets or sets the delegate which maps a value onto an instance of the destination type by reference.
        /// </summary>
        /// <value>The by reference.</value>
        /// <remarks></remarks>
        public Action<object, object, AbstractLookupHelper, AbstractMappingEngine> ByReference { get; protected set; }
    }
}