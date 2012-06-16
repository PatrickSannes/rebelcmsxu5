using System.Configuration;

namespace Sandbox.Hive.Foundation.Configuration
{
  public class FoundationSettingsElement : ConfigurationElement
  {
    private const string XNameApplicationTierAlias = "applicationTierAlias";

    #region Properties

    [ConfigurationProperty(XNameApplicationTierAlias)]
    public string ApplicationTierAlias
    {
      get { return (string)this[XNameApplicationTierAlias]; }

      set { this[XNameApplicationTierAlias] = value; }
    }

    #endregion
  }
}