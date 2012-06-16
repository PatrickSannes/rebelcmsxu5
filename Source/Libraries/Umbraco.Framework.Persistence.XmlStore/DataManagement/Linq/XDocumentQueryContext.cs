using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.DataManagement.Linq;

namespace Umbraco.Framework.Persistence.XmlStore.DataManagement.Linq
{
    public class XDocumentQueryContext<T> : AbstractQueryContext<T>
    {
        public XDocumentQueryContext(IQueryableDataSource queryableDataSource) : base(queryableDataSource)
        {
        }
    }
}
