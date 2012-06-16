using System;

using Umbraco.Framework.DataManagement;
using Umbraco.Framework.Persistence.DataManagement;

namespace Umbraco.Framework.IO
{
    public class Transaction : AbstractTransaction
    {
        protected override bool PerformExplicitRollback()
        {
            return true;
        }

        protected override bool PerformImplicitRollback()
        {
            return true;
        }

        protected override bool PerformCommit()
        {
            return true;
        }
    }
}