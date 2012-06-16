using System.Collections.Generic;

namespace Umbraco.Cms.Web.Configuration.ApplicationSettings
{
    public interface IApplicationCollection
    {
        IEnumerable<IApplication> Applications { get; }
    }
}