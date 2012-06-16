using Sandbox.Hive.Foundation;

namespace Sandbox.Hive.Domain.ServiceRepositoryDomain.PersistenceModel
{
  /// <summary>
  /// Represents an entity persistence provider
  /// </summary>
  /// <remarks></remarks>
  public interface IPersistenceProvider
  {
    /// <summary>
    /// Gets or sets the alias of the provider.
    /// </summary>
    /// <value>The key.</value>
    /// <remarks></remarks>
    string Alias { get; set; }

    /// <summary>
    /// Gets or sets the read-only repository.
    /// </summary>
    /// <value>The reader.</value>
    /// <remarks></remarks>
    IPersistenceRepository Reader { get; set; }

    /// <summary>
    /// Gets or sets the repository which can read and write entities.
    /// </summary>
    /// <value>The read writer.</value>
    /// <remarks></remarks>
    IPersistenceRepository ReadWriter { get; set; }
  }
}