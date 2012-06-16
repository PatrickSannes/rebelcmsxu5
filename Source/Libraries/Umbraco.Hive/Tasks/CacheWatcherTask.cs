using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Tasks;

namespace Umbraco.Hive.Tasks
{
    [Task("9933AAB2-24C9-4021-AF4C-C56D3123B1D4", TaskTriggers.Hive.PostAddOrUpdateOnUnitComplete, ContinueOnFailure = true)]
    public class CacheWatcherTask : AbstractTask 
    {
        public CacheWatcherTask(IFrameworkContext context) : base(context)
        {
        }

        #region Overrides of AbstractTask

        /// <summary>
        /// Executes this task instance.
        /// </summary>
        /// <remarks></remarks>
        public override void Execute(TaskExecutionContext context)
        {
            var eventArgs = context.EventArgs.CallerEventArgs as HiveEntityPostActionEventArgs;
            if (eventArgs != null)
            {
                OperateOnPostAction(eventArgs);
            }
        }

        #endregion

        protected virtual void OperateOnPostAction(HiveEntityPostActionEventArgs eventArgs)
        {
            eventArgs.ScopedCache.AddOrChange(eventArgs.Entity.Id.ToString(), x => eventArgs.Entity);            
        }
    }

    //public class QueryCacheWatcherTask : CacheWatcherTask
    //{
    //    public QueryCacheWatcherTask(IFrameworkContext context) : base(context)
    //    {
    //    }

    //    public override void Execute(TaskExecutionContext context)
    //    {
    //        base.Execute(context);
    //        var eventArgs = context.EventArgs.CallerEventArgs as HiveQueryResultEventArgs;
    //        if (eventArgs != null)
    //        {
    //            //eventArgs.ScopedCache.AddOrChange(new QueryDescriptionCacheKey(eventArgs.QueryDescription), key => eventArgs.Results);
    //        }
    //    }
    //}
}
