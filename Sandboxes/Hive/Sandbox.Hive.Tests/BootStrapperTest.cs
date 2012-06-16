using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sandbox.Hive.BootStrappers.AutoFac;

namespace Sandbox.Hive.Tests
{
  [TestClass]
  public class BootstrapTesting
  {
    [TestMethod]
    public void Can_Get_Container()
    {
      var container = BootStrapper.GetContainer();
      Assert.IsNotNull(container);
    }

    
  }
}
