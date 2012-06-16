using System.Collections.Generic;


namespace Umbraco.Framework.Persistence.Model.Versioning
{
    public class RevisionCollection<T> : HashSet<Revision<T>>
        where T : class, IVersionableEntity
    {
        public RevisionCollection(IEnumerable<Revision<T>> collection) : base(collection)
        {
        }

        public RevisionCollection()
        {
        }

        public void AddRange(IEnumerable<Revision<T>> items)
        {
            items.ForEach(x => Add(x));
        }
    }
}