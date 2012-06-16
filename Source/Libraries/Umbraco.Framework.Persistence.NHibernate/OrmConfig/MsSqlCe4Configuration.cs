using FluentNHibernate.Cfg.Db;
using NHibernate.Dialect;
using NHibernate.Driver;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlServerCe;
    using System.Threading;
    using global::NHibernate.Connection;
    using global::NHibernate.SqlTypes;

    public class MsSqlCe4Configuration : PersistenceConfiguration<MsSqlCe4Configuration>
    {
        public static MsSqlCe4Configuration Standard
        {
            get
            {
                return new MsSqlCe4Configuration().Dialect<MsSqlCe40Dialect>();//.Provider<CustomSqlServerCeConnectionProvider>();
            }
        }

        protected MsSqlCe4Configuration()
        {
            this.Driver<CustomSqlServerCe4Driver>();
        }
    }

    //public class CustomSqlServerCeConnectionProvider : DriverConnectionProvider
    //{
    //    public override IDbConnection GetConnection()
    //    {
    //        IDbConnection connection = this.Driver.CreateConnection();
    //        try
    //        {
    //            switch (connection.State)
    //            {
    //                case ConnectionState.Open:
    //                case ConnectionState.Executing:
    //                case ConnectionState.Connecting:
    //                case ConnectionState.Fetching:
    //                    return connection;
    //            }
    //            if (connection.ConnectionString != ConnectionString) connection.ConnectionString = this.ConnectionString;
    //            connection.Open();
    //        }
    //        catch (Exception ex)
    //        {
    //            connection.Dispose();
    //            throw;
    //        }
    //        return connection;
    //    }

    //    public override void CloseConnection(IDbConnection conn)
    //    {
    //        return;
    //    }
    //}

    /// <summary>
    /// Overrides the default implementation to disable multi-query support, as it's actually not supported
    /// which means any usages of Futures / Multi-criteria fail on SqlCe
    /// </summary>
    public class CustomSqlServerCe4Driver : SqlServerCeDriver
    {
        class DbCommandCacheKey
        {
            public DbCommandCacheKey(string sql, IEnumerable<SqlType> parameters)
            {
                Sql = sql;
                Params = new List<SqlType>(parameters);
            }

            public string Sql = string.Empty;
            public List<SqlType> Params = new List<SqlType>();

            public override string ToString()
            {
                return Sql.ToMd5() + string.Join("|", Params.Select(x => x.DbType.ToString()));
            }
        }

        private static readonly ConcurrentDictionary<DbCommandCacheKey, IDbCommand> _commandCache = new ConcurrentDictionary<DbCommandCacheKey,IDbCommand>();


        [ThreadStatic]
        private static IDbConnection _manualConnectionPool;
        private static readonly ReaderWriterLockSlim _connectionPoolLocker = new ReaderWriterLockSlim();

        //public override IDbConnection CreateConnection()
        //{
        //    using (new WriteLockDisposable(_connectionPoolLocker))
        //    {
        //        if (_manualConnectionPool == null) _manualConnectionPool = base.CreateConnection();
        //    }
        //    return _manualConnectionPool;
        //}

        //public override IDbCommand GenerateCommand(CommandType type, global::NHibernate.SqlCommand.SqlString sqlString, SqlType[] parameterTypes)
        //{
        //    var cacheKey = new DbCommandCacheKey(sqlString.ToString(), parameterTypes);

        //    return _commandCache.GetOrAdd(cacheKey, key => new CustomCeCommand((SqlCeCommand)base.GenerateCommand(type, sqlString, parameterTypes)));
        //}

        protected override bool SupportsPreparingCommands
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsMultipleQueries
        {
            get { return false; }
        }

        public override bool SupportsMultipleOpenReaders
        {
            get
            {
                return true;
            }
        }
    }


    class CustomCeCommand : DbCommand
    {
        private SqlCeCommand _innerCommand;

        public CustomCeCommand(SqlCeCommand innerCommand)
        {
            _innerCommand = innerCommand;
        }

        #region Overrides of DbCommand

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Prepare()
        {
            _innerCommand.Prepare();
        }

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        /// <returns>
        /// The text command to execute. The default value is an empty string ("").
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override string CommandText
        {
            get
            {
                return _innerCommand.CommandText;
            }
            set
            {
                if (value != _innerCommand.CommandText) _innerCommand.CommandText = value;
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        /// <returns>
        /// The time in seconds to wait for the command to execute.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int CommandTimeout
        {
            get
            {
                return _innerCommand.CommandTimeout;
            }
            set
            {
                if (value != _innerCommand.CommandTimeout) _innerCommand.CommandTimeout = value;
            }
        }

        /// <summary>
        /// Indicates or specifies how the <see cref="P:System.Data.Common.DbCommand.CommandText"/> property is interpreted.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.CommandType"/> values. The default is Text.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override CommandType CommandType
        {
            get
            {
                return _innerCommand.CommandType;
            }
            set
            {
                if (value != _innerCommand.CommandType) _innerCommand.CommandType = value;
            }
        }

        /// <summary>
        /// Gets or sets how command results are applied to the <see cref="T:System.Data.DataRow"/> when used by the Update method of a <see cref="T:System.Data.Common.DbDataAdapter"/>.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.UpdateRowSource"/> values. The default is Both unless the command is automatically generated. Then the default is None.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return _innerCommand.UpdatedRowSource;
            }
            set
            {
                _innerCommand.UpdatedRowSource = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.Common.DbConnection"/> used by this <see cref="T:System.Data.Common.DbCommand"/>.
        /// </summary>
        /// <returns>
        /// The connection to the data source.
        /// </returns>
        protected override DbConnection DbConnection
        {
            get
            {
                return _innerCommand.Connection;
            }
            set
            {
                if (_innerCommand.Connection == null)
                    _innerCommand.Connection = (SqlCeConnection)value;
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="T:System.Data.Common.DbParameter"/> objects.
        /// </summary>
        /// <returns>
        /// The parameters of the SQL statement or stored procedure.
        /// </returns>
        protected override DbParameterCollection DbParameterCollection
        {
            get
            {
                return _innerCommand.Parameters;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="P:System.Data.Common.DbCommand.DbTransaction"/> within which this <see cref="T:System.Data.Common.DbCommand"/> object executes.
        /// </summary>
        /// <returns>
        /// The transaction within which a Command object of a .NET Framework data provider executes. The default value is a null reference (Nothing in Visual Basic).
        /// </returns>
        protected override DbTransaction DbTransaction
        {
            get
            {
                return _innerCommand.Transaction;
            }
            set
            {
                _innerCommand.Transaction = (SqlCeTransaction)value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command object should be visible in a customized interface control.
        /// </summary>
        /// <returns>
        /// true, if the command object should be visible in a control; otherwise false. The default is true.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override bool DesignTimeVisible
        {
            get
            {
                return _innerCommand.DesignTimeVisible;
            }
            set
            {
                _innerCommand.DesignTimeVisible = value;
            }
        }

        /// <summary>
        /// Attempts to cancels the execution of a <see cref="T:System.Data.Common.DbCommand"/>.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Cancel()
        {
            _innerCommand.Cancel();
        }

        /// <summary>
        /// Creates a new instance of a <see cref="T:System.Data.Common.DbParameter"/> object.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Common.DbParameter"/> object.
        /// </returns>
        protected override DbParameter CreateDbParameter()
        {
            return _innerCommand.CreateParameter();
        }

        /// <summary>
        /// Executes the command text against the connection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Common.DbDataReader"/>.
        /// </returns>
        /// <param name="behavior">An instance of <see cref="T:System.Data.CommandBehavior"/>.</param>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return _innerCommand.ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes a SQL statement against a connection object.
        /// </summary>
        /// <returns>
        /// The number of rows affected.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int ExecuteNonQuery()
        {
            return _innerCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <returns>
        /// The first column of the first row in the result set.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override object ExecuteScalar()
        {
            return _innerCommand.ExecuteScalar();
        }

        #endregion
    }
}
