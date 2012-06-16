using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Umbraco.Framework.EntityGraph.Domain.Entity;
using System.Collections.Generic;

namespace Umbraco.Framework.EntityGraph.Domain.ObjectModel
{
    internal sealed class LazyMappedIdentifierCollection<T> : Dictionary<IMappedIdentifier, Lazy<T>>
             where T : IEntity
    {
    }
}
