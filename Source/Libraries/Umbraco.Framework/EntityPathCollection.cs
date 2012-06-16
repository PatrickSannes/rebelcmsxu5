using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Framework
{
    public class EntityPathCollection : IEnumerable<EntityPath>
    {
        private IEnumerable<EntityPath> _paths;

        public EntityPathCollection(HiveId destinationId, IEnumerable<EntityPath> paths)
        {
            DestinationId = destinationId;

            _paths = paths;
        }

        public HiveId DestinationId { get; set; }

        public EntityPath this[int i]
        {
            get
            {
                return _paths.ElementAt(i);
            }
        }

        public IEnumerator<EntityPath> GetEnumerator()
        {
            return _paths.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
