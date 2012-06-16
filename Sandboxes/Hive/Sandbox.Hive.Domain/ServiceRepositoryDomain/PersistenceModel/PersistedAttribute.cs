using System;

namespace Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel
{
  public class PersistedAttribute //: IValidatableObject
  {
    //TODO: Separate Dto from DomainModel
    public virtual Guid Id { get; set; }

    public virtual string Key { get; set; }

    public virtual string Value { get; set; }

    public virtual PersistedAttributeTypeDefinition Type { get; set; }

    //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    //{
    //  //TypeDescriptor.AddAttributes(this, new Attribute[]{new })
    //  //var a = new AssociatedMetadataTypeTypeDescriptionProvider(typeof (PersistedAttribute));
    //  //a.
    //}
  }
}