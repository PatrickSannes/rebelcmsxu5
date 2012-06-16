using System;

namespace Umbraco.Hive.ProviderSupport
{
    public class TransactionCompletedException : Exception
    {
        public TransactionCompletedException(string transactionIsNotActive)
            : base(transactionIsNotActive)
        { }
    }
}