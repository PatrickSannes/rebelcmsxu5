using System;
using System.Linq;
using log4net;

namespace Umbraco.Framework.Diagnostics
{
    ///<summary>
    /// Used for logging
    ///</summary>
    public static class LogHelper
    {
        static LogHelper()
        {
            //var appSetting = ConfigurationManager.AppSettings["log4net-config-path"];
            //if (appSetting != null && File.Exists(appSetting))
            //    XmlConfigurator.ConfigureAndWatch(new FileInfo(appSetting));
            //else
                //XmlConfigurator.Configure();
        }

        ///<summary>
        /// Returns a logger for the type specified
        ///</summary>
        ///<typeparam name="T"></typeparam>
        ///<returns></returns>
        public static ILog LoggerFor<T>()
        {
            return LogManager.GetLogger(typeof (T));
        }

        /// <summary>
        /// Returns a logger for the object's type
        /// </summary>
        /// <param name="getTypeFromInstance"></param>
        /// <returns></returns>
        public static ILog LoggerFor(object getTypeFromInstance)
        {
            Mandate.ParameterNotNull(getTypeFromInstance, "getTypeFromInstance");
            return LogManager.GetLogger(getTypeFromInstance.GetType());
        }

        /// <summary>
        /// Adds an error log
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Error<T>(string message, Exception exception)
        {
            var logger = LoggerFor<T>();
            if (logger != null)
                logger.Error(message, exception);
        }

        /// <summary>
        /// Traces a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generateMessageFormat">The generate message format.</param>
        /// <param name="formatItems">The format items.</param>
        /// <remarks></remarks>
        public static void TraceIfEnabled<T>(string generateMessageFormat, params Func<object>[] formatItems)
        {
            var logger = LoggerFor<T>();
            if (logger == null || !logger.IsInfoEnabled) return;
            var executedParams = formatItems.Select(x => x.Invoke()).ToArray();
            logger.InfoFormat(generateMessageFormat, executedParams);
        }

        /// <summary>
        /// Traces a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generateMessage">The delegate to generate a message.</param>
        /// <remarks></remarks>
        public static void TraceIfEnabled<T>(Func<string> generateMessage)
        {
            TraceIfEnabled(typeof (T), generateMessage);
        }

        public static void TraceIfEnabled(Type callingType, Func<string> generateMessage)
        {
            var logger = LogManager.GetLogger(callingType);
            if (logger == null || !logger.IsInfoEnabled) return;
            logger.Info(generateMessage.Invoke());
        }

        public static void Warn(Type callingType, string message)
        {
            var logger = LogManager.GetLogger(callingType);
            if (logger != null)
                logger.Warn(message);
        }

        public static void Warn(Type callingType, string message, params object[] format)
        {
            var logger = LogManager.GetLogger(callingType);
            if (logger != null)
                logger.WarnFormat(message, format);
        }

        /// <summary>
        /// Adds a warn log
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public static void Warn<T>(string message)
        {
            var logger = LoggerFor<T>();
            if (logger != null)
                logger.Warn(message);
        }

        /// <summary>
        /// Adds a warn log
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="format"></param>
        /// <param name="items"></param>
        public static void Warn<T>(string format, params object[] items)
        {
            var logger = LoggerFor<T>();
            if (logger != null)
                logger.WarnFormat(format, items);
        }

        /// <summary>
        /// Traces if tracing is enabled.
        /// </summary>
        /// <param name="type">The type for the logging namespace.</param>
        /// <param name="generateMessageFormat">The message format.</param>
        /// <param name="formatItems">The format items.</param>
        public static void TraceIfEnabled(Type type, string generateMessageFormat, params Func<object>[] formatItems)
        {
            var logger = LogManager.GetLogger(type);
            if (logger == null || !logger.IsInfoEnabled) return;
            var executedParams = formatItems.Select(x => x.Invoke()).ToArray();
            logger.InfoFormat(generateMessageFormat, executedParams);
        }
    }
}
