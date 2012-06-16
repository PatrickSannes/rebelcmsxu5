using System;

namespace Sandbox.PersistenceProviders.NHibernate.Logging
{
    public interface ILog
    {
        void Info(string message);

        void Warning(string message);

        void Error(string message);

        void Exception(Exception exception);
    }
}