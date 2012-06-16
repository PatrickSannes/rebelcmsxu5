using System;
using System.IO;
using System.Reflection;
using Umbraco.Framework.Testing;
using log4net.Config;

using Umbraco.Framework;

namespace Umbraco.Tests.Extensions
{
    public static class TestHelper
    {
        public static void SetupLog4NetForTests()
        {
            XmlConfigurator.Configure(new FileInfo(Common.MapPathForTest("~/unit-test-log4net.config")));
        }
    }
}