using System;
using System.Data;
using HibernatingRhinos.Profiler.Appender;
using NHibernate;
using NHibernate.Engine;

namespace Umbraco.Framework.Persistence.NHibernate.Dependencies
{
    using HibernatingRhinos.Profiler.Appender.NHibernate;

    public static class NhProfilerLogging
    {
        public static bool Enabled { get; set; }

        public const string ProfilerLoggingPrefix = "CUSTOM PROFILE: ";

        public static DisposableTimer Start(ISession session, string message, params IDataParameter[] parameters)
        {
            return Start(session, "Start " + message, "End " + message, parameters);
        }

        public static DisposableTimer Start(ISession session, string startMessage, string endMessage, params IDataParameter[] parameters)
        {
            return Start(GetSessionId(session).ToString(), startMessage, endMessage, parameters);
        }

        public static Guid GetSessionId(ISession session)
        {
            return ((ISessionImplementor)session).SessionId;
        }

        public static DisposableTimer Start(string sessionId, string startMessage, string endMessage, params IDataParameter[] parameters)
        {
            if (Enabled)
            {
                CustomQueryReporting.ReportQuery(sessionId, ProfilerLoggingPrefix + startMessage, parameters, 0, 0, 0);
                return DisposableTimer.Start(x => CustomQueryReporting.ReportQuery(sessionId, ProfilerLoggingPrefix + endMessage, parameters, (int)x, (int)x, 0));
            }
            return DisposableTimer.Start(
                x =>
                    {
                    });
        }
    }
}