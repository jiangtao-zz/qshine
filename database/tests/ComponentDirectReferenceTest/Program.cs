﻿//using qshine.database.oracle;
using qshine.database.sqlite;
using System;
using qshine;
using qshine.Configuration;

namespace ComponentDirectReferenceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Log.SysLoggerProvider = new TraceLoggerProvider();
            Log.SysLogger.EnableLogging(System.Diagnostics.TraceEventType.Verbose);

            //This is only running once. Ignore subsequently call ApplicationEnvironment.Boot().
            ApplicationEnvironment.Build("app.config");
            //var connectionString = "user id=sampledb;password=royal1;data source=sampledb";
            var connectionString = "Data Source=testsqlite.db";
            var provider = new SqlDialectProvider();
            var dialect = new SqlDialect(connectionString);
            dialect.CreateDatabase();
        }
    }
}
