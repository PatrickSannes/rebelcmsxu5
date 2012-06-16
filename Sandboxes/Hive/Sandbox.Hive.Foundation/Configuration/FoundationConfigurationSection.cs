using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Sandbox.Hive.Foundation.Configuration
{
  public class FoundationConfigurationSection : ConfigurationSection, IFoundationConfigurationSection
  {
    private FoundationSettingsElement _foundationSettings;

    private PersistenceProviderElementCollection _persistenceProviderElements;

    [ConfigurationProperty("settings")]
    public FoundationSettingsElement FoundationSettings
    {
      get { return this["settings"] as FoundationSettingsElement ?? _foundationSettings ?? (_foundationSettings = new FoundationSettingsElement()); }

      set { this["settings"] = value; }
    }

    [ConfigurationProperty("persistence-providers")]
    public PersistenceProviderElementCollection PersistenceProviders
    {
      get { return this["persistence-providers"] as PersistenceProviderElementCollection ?? _persistenceProviderElements ?? (_persistenceProviderElements = new PersistenceProviderElementCollection()); }

      set { this["persistence-providers"] = value; }
    }


    public string GetXml()
    {
      return base.SerializeSection(this, "test-output", ConfigurationSaveMode.Full);
    }
  }
}
