using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.database;
using System;
using System.Collections.Generic;

namespace qshine.Tests
{
    [TestClass()]
    public class DatabaseTests
    {
        [TestMethod()]
        public void Database_New_With_ProviderName_ConnectionString()
        {
            var database = new Database("System.Data.SQLite", "connectionString args;");
            Assert.AreEqual("System.Data.SQLite", database.ProviderName);
            Assert.AreEqual("connectionString args;", database.ConnectionString);
            Assert.AreEqual("", database.DataSource);
            Assert.IsFalse(database.IsAlive);

            database = new Database("System.Data.SQLite", "Data Source = sample.db;");
            Assert.AreEqual("System.Data.SQLite", database.ProviderName);
            Assert.AreEqual("Data Source = sample.db;", database.ConnectionString);
            Assert.AreEqual("sample.db", database.DataSource);

            //Assert.IsTrue(database.IsAlive);
        }

        [TestMethod()]
        public void Database_New_Default()
        {
            var database = new Database();
            Assert.IsFalse(database.IsValid);
        }

        [TestMethod()]
        public void SqlDDLBuilder_all()
        {
            var provider = new FakeDialectProvider();
            var database = new SqlDDLDatabase(new Database("System.Data.SQLite", "Data Source=unitTest.db"));

            var table = new SqlDDLTable("t1", "unitTEST", "Test only", "TS1", "INDEX_TS1", 1, "TEST_SCHEMA");
            table.AddColumn("C1", System.Data.DbType.String, 10);
            database.AddTable(table);

            var builder = new SqlDDLBuilder(database, provider);
            //builder.Build();

        }

    }

    public class FakeDialectProvider : ISqlDialectProvider
    {
        public ISqlDialect GetSqlDialect(string connectionString)
        {
            return  new FakeDialect(connectionString);
        }
    }
    public class FakeDialect : SqlDialectStandard
    {
        public FakeDialect(string connectionString) : base(connectionString)
        {
        }

        public override bool CanCreate
        {
            get
            {
                return false;
            }
        }

        public override bool DatabaseExists()
        {
            return true;
        }

        public override string TableExistSql(string tableName)
        {
            return string.Format("is table {0} exists", tableName); 
        }

        public override string ToNativeDBType(string dbType, int size, int scale)
        {
            return dbType;
        }

        public override string ToNativeValue(object value)
        {
            return value.ToString();
        }
    }
}
