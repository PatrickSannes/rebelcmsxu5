using System;
using Umbraco.Framework.EntityGraph.Domain.Entity.Graph;

namespace Umbraco.Framework.EntityGraph.Domain.EntityAdaptors
{
    public interface IUserEntityVertexAdaptor : ITypedEntityVertexAdaptor<IUserEntityVertexAdaptor>
    {
        string Username { get; set; }

        DateTime LastAuthenticationDate { get; set; }

        // etc.
    }
}