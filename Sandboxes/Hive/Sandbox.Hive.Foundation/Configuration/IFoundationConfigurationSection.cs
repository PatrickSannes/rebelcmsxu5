namespace Sandbox.Hive.Foundation.Configuration
{
  public interface IFoundationConfigurationSection
  {
    #region Properties

    FoundationSettingsElement FoundationSettings { get; }

    PersistenceProviderElementCollection PersistenceProviders { get; }

    #endregion
  }
}