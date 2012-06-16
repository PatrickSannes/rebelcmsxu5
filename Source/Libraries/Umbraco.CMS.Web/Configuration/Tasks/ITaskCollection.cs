using System.Collections.Generic;

namespace Umbraco.Cms.Web.Configuration.Tasks
{
    public interface ITaskCollection
    {
        IEnumerable<ITask> Tasks { get; }
    }
}