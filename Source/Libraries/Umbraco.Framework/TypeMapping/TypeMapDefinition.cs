using System;

namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Defines a type map with a Source and Destination type
    /// </summary>
    public class TypeMapDefinition
    {
        public TypeMapDefinition(Type from, Type to)
        {
            Source = from;
            Destination = to;
        }

        public Type Source { get; private set; }
        public Type Destination { get; private set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(TypeMapDefinition)) return false;
            return Equals((TypeMapDefinition)obj);
        }

        public bool Equals(TypeMapDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Source, Source) && Equals(other.Destination, Destination);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Source != null ? Source.GetHashCode() : 0) * 397) ^ (Destination != null ? Destination.GetHashCode() : 0);
            }
        }
    }
}