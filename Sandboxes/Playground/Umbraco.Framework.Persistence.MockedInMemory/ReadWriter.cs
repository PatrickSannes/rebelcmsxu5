using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Persistence.Abstractions;
using Umbraco.Framework.Persistence.DataManagement;
using Umbraco.Framework.Persistence.ProviderSupport;

namespace Umbraco.Framework.Persistence.MockedInMemory
{
    public class ReadWriter : ReadWriterBase
    {
        private ReadWriter()
            : base()
        {
        }

        public ReadWriter(IReadWriteRepositoryUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }
    }
}
