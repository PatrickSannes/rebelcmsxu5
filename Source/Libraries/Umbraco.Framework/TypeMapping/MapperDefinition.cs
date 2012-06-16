namespace Umbraco.Framework.TypeMapping
{
    /// <summary>
    /// Defines a mapper definition with both the mapper instance and its metadata
    /// </summary>
    public class MapperDefinition
    {
        public MapperDefinition(ITypeMapper mapper, TypeMapperMetadata metadata)
        {
            Mapper = mapper;
            Metadata = metadata;
        }

        public ITypeMapper Mapper { get; private set; }
        public TypeMapperMetadata Metadata { get; private set; }
    }
}