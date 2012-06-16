using System.Diagnostics;

namespace Umbraco.Framework.Diagnostics
{
    /// <summary>
    /// Represents a pair of <see cref="PerformanceCounter"/> instances which are used to track average duration between iterations.
    /// </summary>
    public class AverageDurationCounter
    {
        public AverageDurationCounter(PerformanceCounter durationCounter, PerformanceCounter iterationCounter)
        {
            DurationCounter = durationCounter;
            IterationCounter = iterationCounter;
        }

        internal PerformanceCounter DurationCounter { get; private set; }
        internal PerformanceCounter IterationCounter { get; private set; }

        /// <summary>
        /// Increments the duration counter by the number of ticks provided, and increments the iteration counter by one.
        /// </summary>
        /// <param name="ticks">The ticks.</param>
        public void IncrementBy(long ticks)
        {
            DurationCounter.IncrementBy(ticks);
            IterationCounter.Increment();
        }
    }
}