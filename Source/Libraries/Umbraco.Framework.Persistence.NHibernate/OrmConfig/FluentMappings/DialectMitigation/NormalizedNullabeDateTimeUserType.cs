using System;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation
{
    public class NormalizedNullabeDateTimeUserType : NormalizedDateTimeUserType
    {
        public override Type ReturnedType
        {
            get { return typeof (DateTimeOffset?); }
        }
    }
}