using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Framework.Configuration;

namespace Umbraco.Framework.DependencyManagement
{
    public interface IBuilderContext
    {
        IConfigurationResolver ConfigurationResolver { get; }
    }
}
