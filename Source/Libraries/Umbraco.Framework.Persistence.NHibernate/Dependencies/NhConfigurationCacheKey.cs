using System;
using System.Reflection;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig;

namespace Umbraco.Framework.Persistence.NHibernate.Dependencies
{
    /// <summary>
    /// Represents the information needed to construct an NHibernate Configuration, in a form which is suitable for using as a dictionary key.
    /// </summary>
    public struct NhConfigurationCacheKey
    {
        public SupportedNHDrivers Driver;
        public string ConnectionString;
        public Assembly FluentMappingsAssembly;
        public bool ShowSql;
        public bool EnablePostCommitListener;
        public string SessionContextType;
        public bool OutputMappings;
        private string _toString;

        public NhConfigurationCacheKey(SupportedNHDrivers driver, string connectionString, Assembly fluentMappingsAssembly, bool showSql, bool enablePostCommitListener, string sessionContextType, bool outputMappings)
        {
            Driver = driver;
            ConnectionString = connectionString;
            FluentMappingsAssembly = fluentMappingsAssembly;
            ShowSql = showSql;
            EnablePostCommitListener = enablePostCommitListener;
            SessionContextType = sessionContextType;
            OutputMappings = outputMappings;
            _toString = null;
        }

        public override string ToString()
        {
            return _toString ??
                   (_toString =
                    String.Concat(
                        Driver,
                        ",",
                        ConnectionString,
                        ",",
                        FluentMappingsAssembly.FullName,
                        ",",
                        ShowSql,
                        ",",
                        EnablePostCommitListener,
                        ",",
                        SessionContextType,
                        ",",
                        OutputMappings));
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            var casted = (NhConfigurationCacheKey)obj;
            return casted.ToString() == ToString();
        }
    }
}