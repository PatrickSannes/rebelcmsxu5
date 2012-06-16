using System.Collections.Generic;

namespace Umbraco.Cms.Web.Configuration.ApplicationSettings
{
    public interface ITreeCollection
    {
        IEnumerable<ITree> Trees { get; }
    }
}