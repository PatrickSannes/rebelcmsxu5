using System.Diagnostics;

namespace Umbraco.Framework.Diagnostics
{
    /// <summary>
    /// Describes a performance counter's name, help text and type (<see cref="PerformanceCounterType"/>). This is used within an <see cref="AbstractCounterManager{T}"/> implementation to 
    /// specify the required performance counters.
    /// </summary>
    public struct CounterDescriptor
    {
        public CounterDescriptor(string name, string help, PerformanceCounterType type)
            : this()
        {
            Name = name;
            Help = help;
            Type = type;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the help.
        /// </summary>
        /// <value>The help.</value>
        public string Help { get; private set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public PerformanceCounterType Type { get; private set; }
    }
}