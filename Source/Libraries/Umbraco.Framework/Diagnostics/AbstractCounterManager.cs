using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Umbraco.Framework.Diagnostics
{
    /// <summary>
    /// Abstract implementation of a manager which maintains <see cref="PerformanceCounter"/> objects by using a key of type <typeparamref name="TEnum"/>.
    /// In explicit implementations, <typeparamref name="TEnum"/> is typically of type <see cref="System.Enum"/> but can be any struct implementing <see cref="IComparable"/>.
    /// </summary>
    /// <typeparam name="TEnum">The type used as a key internally to track <see cref="PerformanceCounter"/> instances.</typeparam>
    public abstract class AbstractCounterManager<TEnum> : DisposableObject where TEnum : struct, IComparable
    {
        [ThreadStatic]
        private readonly Dictionary<TEnum, CounterDescriptor> _counterDescriptors = new Dictionary<TEnum, CounterDescriptor>();
        [ThreadStatic]
        private readonly Dictionary<TEnum, PerformanceCounter> _writeableCounters = new Dictionary<TEnum, PerformanceCounter>();
        [ThreadStatic]
        private static AbstractCounterManager<TEnum> _instance;

        protected AbstractCounterManager(string categoryName, string instanceName)
        {
            CategoryName = categoryName;
            InstanceName = instanceName;
            CountersAvailable = CounterAvailability.Unknown;
            IsInitialised = false;

            EnsureInitialised();
        }

        public static AbstractCounterManager<TEnum> Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }

        /// <summary>
        /// Gets or sets the name of the performance counter instance which this manager represents.
        /// </summary>
        public string InstanceName { get; private set; }

        /// <summary>
        /// Gets or sets the name of the performance counter category which this manager represents.
        /// </summary>
        public string CategoryName { get; private set; }

        /// <summary>
        /// Gets or sets the category help.
        /// </summary>
        /// <value>The category help.</value>
        public string CategoryHelp { get; private set; }

        /// <summary>
        /// An enumeration of type <see cref="CounterAvailability"/> indicating the availability of counters for writing.
        /// The value of this property is set by <see cref="EnsureCountersExist"/>.
        /// </summary>
        /// <value>The counters available.</value>
        public virtual CounterAvailability CountersAvailable { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is initialised.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is initialised; otherwise, <c>false</c>.
        /// </value>
        protected bool IsInitialised { get; set; }

        /// <summary>
        /// When implemented in a derived class, generates the set of counters which this manager should ensure are avaible in WMI.
        /// </summary>
        /// <returns></returns>
        protected abstract IDictionary<TEnum, CounterDescriptor> GenerateCounterNames();

        /// <summary>
        /// If <see cref="IsInitialised"/> is false, calls <see cref="GenerateCounterNames"/>, followed by <see cref="EnsureCountersExist"/>, and sets <see cref="IsInitialised"/> to true.
        /// </summary>
        protected void EnsureInitialised()
        {
            if (!IsInitialised)
            {
                foreach (var counterDescriptor in GenerateCounterNames())
                {
                    if (!_counterDescriptors.ContainsKey(counterDescriptor.Key))
                        _counterDescriptors.Add(counterDescriptor.Key, counterDescriptor.Value);
                }

                EnsureCountersExist();

                IsInitialised = true;
            }
        }

        /// <summary>
        /// Gets or creates an <see cref="AverageDurationCounter"/>.
        /// </summary>
        /// <remarks>
        /// For tracking the average duration of an iteration, .NET's implementation of WMI performance counters requires two physical counters, one for
        /// the incrementing elapsed time and one for the incrementing number of iterations over which the time has been spent. This method acts as a wrapper
        /// for easier access to this functionality.
        /// </remarks>
        /// <param name="durationType">The key used to find the duration counter.</param>
        /// <param name="iterationType">The key used to find the iteration counter.</param>
        /// <returns></returns>
        protected AverageDurationCounter GetDurationCounter(TEnum durationType, TEnum iterationType)
        {
            if (CountersAvailable != CounterAvailability.Available)
                return null;

            var duration = GetWriteableCounter(durationType);
            var iteration = GetWriteableCounter(iterationType);
            return new AverageDurationCounter(duration, iteration);
        }

        /// <summary>
        /// Gets or creates a writeable <see cref="PerformanceCounter"/>.
        /// </summary>
        /// <param name="type">The key used to find the counter descriptor.</param>
        /// <remarks>If <see cref="CountersAvailable"/> is anything other than <code>CounterAvailability.Available</code>, this method returns without operation.</remarks>
        protected PerformanceCounter GetWriteableCounter(TEnum type)
        {
            if (CountersAvailable != CounterAvailability.Available)
                return null;

            var meta = _counterDescriptors[type];
            if (!_writeableCounters.ContainsKey(type))
                _writeableCounters.Add(type, new PerformanceCounter()
                                                 {
                                                     CategoryName = CategoryName,
                                                     CounterName = meta.Name,
                                                     MachineName = ".",
                                                     ReadOnly = false,
                                                     InstanceName = InstanceName
                                                 });
            return _writeableCounters[type];
        }

        /// <summary>
        /// Increments the specified counter by one.
        /// </summary>
        public void Increment(TEnum type)
        {
            var writeable = GetWriteableCounter(type);
            if (writeable != null)
                writeable.Increment();
        }

        /// <summary>
        /// Increments the specified average duration counter by the <paramref name="ticks"/> provided.
        /// </summary>
        /// <param name="durationType">The key used to find the duration counter.</param>
        /// <param name="iterationType">The key used to find the iteration counter.</param>
        /// <param name="ticks">The time over which the previous iteration elapsed, in system clock ticks.</param>
        public void IncrementDuration(TEnum durationType, TEnum iterationType, long ticks)
        {
            var writeable = GetDurationCounter(durationType, iterationType);
            if (writeable != null)
                writeable.IncrementBy(ticks);
        }

        /// <summary>
        /// Increments the specified average duration counter by obtaining the elapsed ticks from the <paramref name="stopwatch"/> provided.
        /// </summary>
        /// <param name="durationType">Type of the duration.</param>
        /// <param name="iterationType">Type of the iteration.</param>
        /// <param name="stopwatch">The stopwatch.</param>
        public void IncrementDuration(TEnum durationType, TEnum iterationType, Stopwatch stopwatch)
        {
            IncrementDuration(durationType, iterationType, stopwatch.ElapsedTicks);
        }

        /// <summary>
        /// If an attempt has not already been made according to <see cref="CountersAvailable"/>, tries to ensure that the relevant performance counters exist in WMI.
        /// </summary>
        /// <returns>A <see cref="CounterAvailability"/> indicating the success of the operation.</returns>
        /// <remarks>This method will update <see cref="CountersAvailable"/> with the result.</remarks>
        protected CounterAvailability EnsureCountersExist()
        {
            if (CountersAvailable == CounterAvailability.Unknown)
            {
                try
                {
                    if (!PerformanceCounterCategory.Exists(CategoryName))
                    {
                        var createCounters = new CounterCreationDataCollection();

                        foreach (var counterDescriptor in _counterDescriptors)
                        {
                            createCounters.Add(new CounterCreationData(counterDescriptor.Value.Name,
                                                                       counterDescriptor.Value.Help,
                                                                       counterDescriptor.Value.Type));
                        }

                        // create new category with the counters above
                        PerformanceCounterCategory.Create(CategoryName, CategoryHelp,
                                                          PerformanceCounterCategoryType.MultiInstance,
                                                          createCounters);
                    }
                    return CountersAvailable = CounterAvailability.Available;
                }
                catch (UnauthorizedAccessException ex)
                {
                    return CountersAvailable = CounterAvailability.NoneDueToPermissions;
                }
                catch
                {
                    return CountersAvailable = CounterAvailability.NoneDueToError;
                }
            }
            return CountersAvailable;
        }

        protected override void DisposeResources()
        {
            if (IsDisposed) return;
            if (_writeableCounters != null)
            {
                foreach (var performanceCounter in _writeableCounters)
                {
                    performanceCounter.Value.Dispose();
                }
            }
        }
    }
}