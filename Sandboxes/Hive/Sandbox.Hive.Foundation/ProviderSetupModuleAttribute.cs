using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandbox.Hive.Foundation
{
  [AttributeUsage(AttributeTargets.Class)]
  public class ProviderSetupModuleAttribute : Attribute
  {
    public ProviderSetupModuleAttribute()
    {
      
    }
  }
}
