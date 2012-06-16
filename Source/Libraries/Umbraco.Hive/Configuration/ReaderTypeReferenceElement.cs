using System.Configuration;

namespace Umbraco.Hive.Configuration
{
    public class ReaderTypeReferenceElement : ReadWriterTypeReferenceElement
    {
        private const string XPropagateMissesKey = "propagateReadMissesToWriters";

        [ConfigurationProperty(XPropagateMissesKey)]
        public bool PropagateReadMisses
        {
            get
            {
                return (bool)this[XPropagateMissesKey];
            }

            set { this[XPropagateMissesKey] = value; }
        }
    }
}