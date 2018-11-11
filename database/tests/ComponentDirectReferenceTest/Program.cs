//using qshine.database.oracle;
using qshine.database.sqlite;
using System;
using qshine;

namespace ComponentDirectReferenceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //var connectionString = "user id=sampledb;password=royal1;data source=sampledb";
            var connectionString = "Data Source=testsqlite.db";
            var provider = new SqlDialectProvider();
            var dialect = new SqlDialect(connectionString);
            dialect.CreateDatabase();
        }
    }
}
