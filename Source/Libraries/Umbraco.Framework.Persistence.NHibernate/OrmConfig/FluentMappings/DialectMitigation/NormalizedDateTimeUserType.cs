using System;
using System.Data;
using System.Data.SqlTypes;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Umbraco.Framework.Diagnostics;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation
{
    // For details on this code see http://groups.google.com/group/nhusers/browse_thread/thread/c48da661f78bfad0?pli=1
    public class NormalizedDateTimeUserType : IUserType
    {
        private readonly TimeZoneInfo databaseTimeZone = TimeZoneInfo.Local;

        #region IUserType Members

        public virtual Type ReturnedType
        {
            get { return typeof (DateTimeOffset); }
        }

        public virtual bool IsMutable
        {
            get { return false; }
        }

        public virtual object Disassemble(object value)
        {
            return value;
        }

        public virtual SqlType[] SqlTypes
        {
            get { return new[] {new SqlType(DbType.DateTime)}; }
        }

        public virtual bool Equals(object x, object y)
        {
            return object.Equals(x, y);
        }

        public virtual int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public virtual object NullSafeGet(IDataReader dr, string[] names, object owner)
        {
            object r = dr[names[0]];
            if (r == DBNull.Value)
            {
                return null;
            }
            var storedTime = (DateTime) r;
            return new DateTimeOffset(storedTime,
                                      databaseTimeZone.BaseUtcOffset);
        }

        public virtual void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            if (value == null)
            {
                NHibernateUtil.DateTime.NullSafeSet(cmd, null, index);
            }
            else
            {
                var parameter = (IDataParameter) cmd.Parameters[index];
                DateTime paramVal = DateTime.MinValue;
                try
                {
                    var dateTimeOffset = (DateTimeOffset) value;
                    paramVal = dateTimeOffset.ToOffset(databaseTimeZone.BaseUtcOffset).DateTime;
                    if (paramVal < SqlDateTime.MinValue.Value) paramVal = SqlDateTime.MinValue.Value;
                    if (paramVal > SqlDateTime.MaxValue.Value) paramVal = SqlDateTime.MaxValue.Value;
                    parameter.Value = paramVal;
                }
                catch (Exception e)
                {
                    LogHelper.Warn<NormalizedDateTimeUserType>("Failure to parse DateTime '{0}' from SQL.\n{1}\n{2}", paramVal, e.Message, e);
                    parameter.Value = SqlDateTime.MinValue.Value;
                }
            }
        }

        public virtual object DeepCopy(object value)
        {
            return value;
        }

        public virtual object Replace(object original, object target, object
                                                                          owner)
        {
            return original;
        }

        public virtual object Assemble(object cached, object owner)
        {
            return cached;
        }

        #endregion
    }
}