using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine;
using qshine.Configuration;
using qshine.database;
using System.Data;

namespace testsqlite
{
    [TestClass]
    public class DialectTests
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            EnvironmentManager.Boot("app.config");
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
        }

        [TestMethod]
        public void CreateDatabase_BestCase()
        {

            var database = new Database("testdb");
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);

            Assert.IsTrue(dialect.CanCreate);
            Assert.IsTrue(dialect.CreateDatabase());
            Assert.IsTrue(dialect.DatabaseExists());
        }

        [TestMethod]
        public void TableNotExists()
        {
            var database = new Database("testdb");
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists
            var sql = dialect.TableExistSql("table1");
            Assert.AreEqual("select name from sqlite_master where type = 'table' and name = 'table1'", sql);

            using(var dbclient = new DbClient(database))
            {
                var result = dbclient.SqlSelect(sql);
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        public void TableCreate()
        {
            var database = new Database("testdb");
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table1", "test", "test table 1", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100)
                .AddColumn("T2", DbType.String, 10000,12,false,"ABC","TEST C2",isUnique:true,isIndex:true,version:2,oldColumnNames:"T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0,defaultValue:16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                .AddColumn("T6", DbType.Decimal, 0, defaultValue: 12.345678)
                .AddColumn("T7", DbType.Double, 0, defaultValue: 22.345678)
                .AddColumn("T8", DbType.SByte, 0, defaultValue: 8)
                .AddColumn("T9", DbType.Single,0, defaultValue: 123.456)
                .AddColumn("T10", DbType.StringFixedLength, 3, defaultValue: "ABC")
                .AddColumn("T11", DbType.Time, 0, defaultValue: SqlReservedWord.SysDate)
                .AddColumn("T12", DbType.UInt16, 0, defaultValue: 123)
                .AddColumn("T13", DbType.UInt32, 0, defaultValue: 1234)
                .AddColumn("T14", DbType.UInt64, 0, defaultValue: 12345)
                .AddColumn("T15", DbType.VarNumeric, 12, defaultValue: 12.123)
                .AddColumn("T16", DbType.AnsiString, 100, defaultValue: "abcdefg")
                .AddColumn("T17", DbType.AnsiStringFixedLength, 100, defaultValue: "abcdefgf")
                .AddColumn("T18", DbType.Binary, 100)
                .AddColumn("T19", DbType.Boolean, 0, defaultValue:true)
                .AddColumn("T20", DbType.Byte, 0, defaultValue: 1)
                .AddColumn("T21", DbType.Currency, 0, defaultValue: 12.34)
                .AddColumn("T22", DbType.Date, 0, defaultValue: SqlReservedWord.SysDate)
                .AddColumn("T23", DbType.DateTime, 0, defaultValue: SqlReservedWord.SysDate)
                .AddColumn("T24", DbType.DateTime2, 0, defaultValue: SqlReservedWord.SysDate)
                .AddColumn("T25", DbType.DateTimeOffset, 0, defaultValue: SqlReservedWord.SysDate)
                .AddColumn("T26", DbType.Guid, 0)
                .AddColumn("T27", DbType.Xml, 100)
                ;

            var sql = dialect.TableCreateSql(table);
            using (var dbclient = new DbClient(database))
            {
                var result = dbclient.Sql(sql);
                Assert.AreNotEqual(-1,result);

                dbclient.Sql("insert into table1(T1) values(@p1)", DbParameters.New.Input("p1", "AAA"));
                var data = dbclient.SqlDataTable("select * from table1 where T1='AAA'");
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("AAA", data.Rows[0]["T1"]);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());
                Assert.AreEqual("12.345678", data.Rows[0]["T6"].ToString());
                Assert.AreEqual("22.345678", data.Rows[0]["T7"].ToString());
                Assert.AreEqual("8", data.Rows[0]["T8"].ToString());
                Assert.AreEqual("123.456", data.Rows[0]["T9"].ToString());

                dbclient.Sql("drop table table1;");
            }
        }



    }
}


