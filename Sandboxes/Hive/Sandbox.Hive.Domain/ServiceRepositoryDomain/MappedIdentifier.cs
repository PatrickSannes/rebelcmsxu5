namespace Sandbox.Hive.Domain.ServiceRepositoryDomain
{
  public struct MappedIdentifier
  {
    public string ProviderKey { get; set; }
    public dynamic Value { get; set; }

    public static MappedIdentifier Empty
    {
      get { return new MappedIdentifier() { ProviderKey = "unknown", Value = string.Empty }; }
    }
  }
}