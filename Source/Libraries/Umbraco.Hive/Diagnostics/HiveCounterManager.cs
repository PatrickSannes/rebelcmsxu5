using System.Collections.Generic;
using System.Diagnostics;
using Umbraco.Framework.Diagnostics;

namespace Umbraco.Hive.Diagnostics
{
    public class HiveCounterManager : AbstractCounterManager<HivePreDefinedCounters>
    {
        public HiveCounterManager(string category, string instance, string tier)
            : base(category, string.Format("[{0}] {1}", tier, instance))
        {

        }

        protected override IDictionary<HivePreDefinedCounters, CounterDescriptor> GenerateCounterNames()
        {
            return
                new Dictionary<HivePreDefinedCounters, CounterDescriptor>()
                    {
                        //{
                        //    PreDefinedCounters.AppStartupIncremental,
                        //    new CounterDescriptor("Total # App Startups", "", PerformanceCounterType.NumberOfItems32)
                        //    },
                        //{
                        //    PreDefinedCounters.RenderContextFactoryIncremental,
                        //    new CounterDescriptor("Total # RenderContextFactory invocations", "", PerformanceCounterType.NumberOfItems32)
                        //    },
                        //{
                        //    PreDefinedCounters.RenderContextFactoryRate,
                        //    new CounterDescriptor("RenderContextFactory invocations/s", "", PerformanceCounterType.RateOfCountsPerSecond32)
                        //    },
                        {
                            HivePreDefinedCounters.AverageHiveQueryDuration,
                            new CounterDescriptor("Avg Hive query duration", "", PerformanceCounterType.AverageTimer32)
                            },
                        {
                            HivePreDefinedCounters.AverageHiveQueryDurationBase,
                            new CounterDescriptor("Avg Hive query duration (iteration tracker)", "", PerformanceCounterType.AverageBase)
                            }
                    };
        }
    }
}