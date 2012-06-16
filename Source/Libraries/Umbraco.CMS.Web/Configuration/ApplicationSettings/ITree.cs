using System;

namespace Umbraco.Cms.Web.Configuration.ApplicationSettings
{
    public interface ITree
    {
        string ApplicationAlias { get; }
        Type ControllerType { get; }
    }
}