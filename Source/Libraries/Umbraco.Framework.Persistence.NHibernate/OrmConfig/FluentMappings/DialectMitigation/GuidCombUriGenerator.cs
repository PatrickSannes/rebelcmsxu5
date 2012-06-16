using System;
using NHibernate.Engine;
using NHibernate.Id;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation
{
    public class GuidCombUriGenerator : IIdentifierGenerator
    {
        public object Generate(ISessionImplementor session, object obj)
        {
            //return new GuidCombGenerator().Generate(session, obj);

            var existingId = session.GetContextEntityIdentifier(obj);
            Guid existingGuid = Guid.Empty;
            if (existingId != null)
                existingGuid = (Guid)existingId;
            else
            {
                var castAsRef = obj as IReferenceByGuid;
                if (castAsRef != null)
                    existingGuid = castAsRef.Id;
            }

            if (existingGuid != Guid.Empty && existingGuid != null)
                return existingGuid;

            var newGuid = (Guid)new GuidCombGenerator().Generate(session, obj);
            // return new HiveId(newGuid);
            return newGuid;
        }

    }
}