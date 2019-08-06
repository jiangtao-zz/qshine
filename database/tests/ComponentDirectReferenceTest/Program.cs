//using qshine.database.oracle;
using qshine.database.sqlite;
using System;
using qshine.Logger;
using qshine.Configuration;
using qshine;
using qshine.Configuration.ConfigurationStore;

namespace ComponentDirectReferenceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var options = new EnvironmentInitializationOption {
                OverwriteConnectionString = true
            };
            var builder = new ApplicationEnvironmentBuilder();

            builder.Configure(
                (appContext, config) =>
                {
                    config.LoadConfigFile("app.config", options);
                }
                )
                .Build();

            //var connectionString = "user id=sampledb;password=royal1;data source=sampledb";
            var connectionString = "Data Source=testsqlite.db";
            var provider = new SqlDialectProvider();
            var dialect = new SqlDialect(connectionString);
            dialect.CreateDatabase();
        }
    }
}
