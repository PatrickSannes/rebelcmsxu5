using System;
using System.Data.SqlClient;
using System.IO;
using Umbraco.Framework.Testing;
using log4net.Config;

namespace Umbraco.Tests.Extensions
{
    public class DataHelper
    {
        

        public static void EnsureDbForTest(string dbFileName)
        {
            var appData = (string) AppDomain.CurrentDomain.GetData("DataDirectory");
            if (String.IsNullOrWhiteSpace(appData)) appData = Common.CurrentAssemblyDirectory;
            var filename = Path.Combine(appData, dbFileName);
            if (!File.Exists(filename))
                CreateSqlExpressDatabase(filename);
        }

        public static void CreateSqlExpressDatabase(string filename)
        {
            string databaseName = Path.GetFileNameWithoutExtension(filename);
            using (var connection = new SqlConnection(
                "Data Source=.\\sqlexpress;Initial Catalog=tempdb;" +
                "Integrated Security=true;User Instance=True;"))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "CREATE DATABASE " + databaseName +
                        " ON PRIMARY (NAME=" + databaseName +
                        ", FILENAME='" + filename + "')";
                    command.ExecuteNonQuery();

                    command.CommandText =
                        "EXEC sp_detach_db '" + databaseName + "', 'true'";
                    command.ExecuteNonQuery();
                }
            }
        }

        

        public static void SetupLog4NetForTests()
        {
            XmlConfigurator.Configure(new FileInfo(Common.MapPathForTest("~/unit-test-log4net.config")));
        }
    }
}