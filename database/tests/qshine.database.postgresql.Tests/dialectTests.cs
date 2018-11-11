using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine;
using qshine.Configuration;
using qshine.database;
using System.Data;

namespace qshine.database.postgresql.Tests
{
    [TestClass]
    public class DialectTests
    {
        private static Database _testDb;
        private void DropTable(string tableName)
        {
            using (var dbClient = new DbClient(_testDb))
            {
                //Try to drop a table
                try
                {
                    dbClient.Sql("drop table " + tableName);
                }
                catch (Exception ex)
                {
                    Log.DevDebug(ex.Message);
                }

                //Try to drop an auto sequence for the table PK
                try
                {
                    dbClient.Sql("drop sequence " + tableName + "_aid");
                }
                catch (Exception ex)
                {
                    Log.DevDebug(ex.Message);
                }

                //Try to drop an auto trigger for the table PK
                try
                {
                    dbClient.Sql("drop trigger " + tableName + "_aid");
                }
                catch (Exception ex)
                {
                    Log.DevDebug(ex.Message);
                }

            }
        }


        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            Log.SysLoggerProvider = new TraceLoggerProvider();
            Log.SysLogger.EnableLogging(System.Diagnostics.TraceEventType.Verbose);

            ApplicationEnvironment.Build("app.config");
            _testDb = new Database("testdb");
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
        }

        [TestMethod]
        public void CreateDatabase_BestCase()
        {

            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            Assert.IsTrue(dialect.CanCreate);

            //Need create database before start the test
            Assert.IsTrue(dialect.DatabaseExists());
        }

        [TestMethod]
        public void TableNotExists()
        {
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var sql = dialect.TableExistSql("table0");

            using (var dbclient = new DbClient(_testDb))
            {
                var result = dbclient.SqlSelect(sql);
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        public void TableCreate()
        {
            var testTable = "table1";

            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 1", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 1000)
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                .AddColumn("T6", DbType.Decimal, 0, defaultValue: 12.345678)
                .AddColumn("T7", DbType.Double, 0, defaultValue: 22.345678)
                .AddColumn("T8", DbType.SByte, 0, defaultValue: 8)
                .AddColumn("T9", DbType.Single, 0, defaultValue: 123.456)
                .AddColumn("T10", DbType.StringFixedLength, 5, defaultValue: "ABC")
                .AddColumn("T11", DbType.Time, 0, defaultValue: SqlReservedWord.SysDate)
                .AddColumn("T12", DbType.UInt16, 0, defaultValue: 123)
                .AddColumn("T13", DbType.UInt32, 0, defaultValue: 1234)
                .AddColumn("T14", DbType.UInt64, 0, defaultValue: 12345)
                .AddColumn("T15", DbType.VarNumeric, 12, 5, defaultValue: 12.123)
                .AddColumn("T16", DbType.AnsiString, 100, defaultValue: "abcdefg")
                .AddColumn("T17", DbType.AnsiStringFixedLength, 10, defaultValue: "abcdefgf")
                .AddColumn("T18", DbType.Binary, 100)
                .AddColumn("T19", DbType.Boolean, 0, defaultValue: true)
                .AddColumn("T20", DbType.Byte, 0, defaultValue: 1)
                .AddColumn("T21", DbType.Currency, 0, defaultValue: 12.34)
                .AddColumn("T22", DbType.Date, 0, defaultValue: SqlReservedWord.SysDate)
                .AddColumn("T23", DbType.DateTime, 0, defaultValue: SqlReservedWord.SysDate)
                .AddColumn("T24", DbType.DateTime2, 0, defaultValue: SqlReservedWord.SysDate)
                .AddColumn("T25", DbType.DateTimeOffset, 0, defaultValue: SqlReservedWord.SysDate)
                .AddColumn("T26", DbType.Guid, 0)
                .AddColumn("T27", DbType.Xml, 100)
                .AddColumn("T28", DbType.String, 10000, defaultValue: "CLOB Data")

                ;

            var sql = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                DropTable(testTable);

                var result = dbclient.Sql(false, sql);
                Assert.IsTrue(result);

                dbclient.Sql(string.Format("insert into {0}(T1) values(@p1)", testTable), DbParameters.New.Input("p1", "AAA"));
                var data = dbclient.SqlDataTable("select * from table1 where T1='AAA'");
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("AAA", data.Rows[0]["T1"]);
                Assert.AreEqual((short)16, data.Rows[0]["T3"]);
                Assert.AreEqual(32, data.Rows[0]["T4"]);
                Assert.AreEqual(1234567890L, data.Rows[0]["T5"]);
                Assert.AreEqual(12.345678M, data.Rows[0]["T6"]);
                Assert.AreEqual(22.345678D, data.Rows[0]["T7"]);
                Assert.AreEqual("8", data.Rows[0]["T8"].ToString());
                Assert.AreEqual("123.456", data.Rows[0]["T9"].ToString());
                Assert.AreEqual("ABC  ", data.Rows[0]["T10"].ToString());
                TimeSpan ticket = DateTime.Now - data.Rows[0]["T11"].ToDateTime();
                Assert.IsTrue(ticket.Seconds < 10);
                Assert.AreEqual("123", data.Rows[0]["T12"].ToString());
                Assert.AreEqual("1234", data.Rows[0]["T13"].ToString());
                Assert.AreEqual("12345", data.Rows[0]["T14"].ToString());
                Assert.AreEqual(12.123M, data.Rows[0]["T15"]);
                Assert.AreEqual("abcdefg", data.Rows[0]["T16"].ToString());
                Assert.AreEqual("abcdefgf  ", data.Rows[0]["T17"].ToString());
                //Assert.AreEqual("123.456", data.Rows[0]["T18"].ToString());
                Assert.AreEqual(true, DbClient.ToBoolean(data.Rows[0]["T19"]));
                Assert.AreEqual("1", data.Rows[0]["T20"].ToString());
                Assert.AreEqual("12.34", data.Rows[0]["T21"].ToString());
                ticket = DateTime.Now.Date - (DateTime)data.Rows[0]["T22"];
                Assert.IsTrue(ticket.Seconds < 60);
                ticket = DateTime.Now - (DateTime)data.Rows[0]["T23"];
                Assert.IsTrue(ticket.Seconds < 10);
                ticket = DateTime.Now - (DateTime)data.Rows[0]["T24"];
                Assert.IsTrue(ticket.Seconds < 10);
                ticket = DateTime.Now - (DateTime)data.Rows[0]["T25"];
                Assert.IsTrue(ticket.Seconds < 10);
                //                Assert.AreEqual("123.456", data.Rows[0]["T26"].ToString());
                //                Assert.AreEqual("123.456", data.Rows[0]["T27"].ToString());

                Assert.AreEqual("CLOB Data", data.Rows[0]["T28"].ToString());


                DropTable(testTable);
            }
        }

        [TestMethod]
        public void TableUpdate_Rename_column_add_new_column()
        {
            var testTable = "table2";
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 2", "testspace1", "testindex1", 2, "NewTest");

            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var sqls = dialect.TableCreateSqls(table);
            DropTable(testTable);

            using (var dbclient = new DbClient(_testDb))
            {

                dbclient.Sql(false, sqls);

                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "AAA"));

            }

            var trackingTable = new TrackingTable(table);

            table = new SqlDDLTable(testTable, "test", "test table 2", "testspace1", "testindex1", 3, "NewTest");

            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1x", DbType.String, 120, defaultValue: "X123", version: 2)
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                .AddColumn("T6", DbType.Decimal, 0, defaultValue: 12.345678);

            //Analyse the table change
            dialect.AnalyseTableChange(table, trackingTable);

            sqls = dialect.TableUpdateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                var result = dbclient.Sql(false, sqls);
                Assert.IsTrue(result);


                var data = dbclient.SqlDataTable(string.Format("select * from {0} where T2='AAA'", testTable));
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());
                Assert.AreEqual("12.345678", data.Rows[0]["T6"].ToString());
                Assert.AreEqual("A", data.Rows[0]["T1x"]);

                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "BBB"));

                data = dbclient.SqlDataTable(string.Format("select * from {0} where T2='BBB'", testTable));
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());
                Assert.AreEqual("12.345678", data.Rows[0]["T6"].ToString());
                Assert.AreEqual("X123", data.Rows[0]["T1x"]);
            }

            DropTable(testTable);
        }

        [TestMethod]
        public void TableUpdate_Rename_table()
        {
            var testTable = "table3";
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 3", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var sqls = dialect.TableCreateSqls(table);
            DropTable(testTable);
            DropTable(testTable + "_1");

            using (var dbclient = new DbClient(_testDb))
            {
                var result = dbclient.Sql(false, sqls);
                Assert.IsTrue(result);


                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "AAA"));

                var sql = dialect.TableRenameClause(table.TableName, testTable + "_1");

                dbclient.Sql(sql);


                var data = dbclient.SqlDataTable(string.Format("select * from {0} where T2='AAA'", testTable + "_1"));
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());

            }
            DropTable(testTable);
            DropTable(testTable + "_1");

        }

        [TestMethod]
        public void TableUpdate_default_remove()
        {
            var testTable = "table4";
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 4", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            DropTable(testTable);

            var trackingTable = new TrackingTable(table);


            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                var result = dbclient.Sql(false, sqls);
                Assert.IsTrue(result);


                //insert data for compare
                dbclient.Sql(string.Format(
                    "insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "AAA"));

                table = new SqlDDLTable(testTable, "test", "test table 4", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddAuditColumn()
                    .AddColumn("T3", DbType.Int16, 0, version: 2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);


                dbclient.Sql(string.Format(
                    "insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "BBB"));


                var data = dbclient.SqlDataTable(string.Format("select * from {0} where T2='AAA'", testTable));
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());

                data = dbclient.SqlDataTable(string.Format("select * from {0} where T2='BBB'", testTable));
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());

            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_index_add_and_notnull()
        {
            var testTable = "table5";
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 5", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            DropTable(testTable);

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {

                //create a new table
                dbclient.Sql(false, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));

                table = new SqlDDLTable(testTable, "test", "test table 5", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddAuditColumn()
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, isIndex: true, allowNull: false, version: 3)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                var count = dbclient.SqlSelect(string.Format(
                    "select count(*) from pg_indexes  where tablename = '{0}'", testTable.ToLower()));

                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);


                var data = dbclient.SqlDataTable(string.Format("select * from {0} where T2='AAA'", testTable));
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());

                try
                {
                    dbclient.SqlDataTable(string.Format("insert into {0}(T3) values(NULL)", testTable));
                    Assert.Fail("Not Null constraint failed.");
                }
                catch { }


                //check for index
                var count1 = dbclient.SqlSelect(string.Format(
                    "select count(*) from pg_indexes  where tablename = '{0}'", testTable.ToLower()));

                Assert.AreEqual(int.Parse(count.ToString()) + 1, int.Parse(count1.ToString()));

            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_CheckConstraint_add()
        {
            var testTable = "table6";
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 6", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            DropTable(testTable);

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);

            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(false, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable(testTable, "test", "test table 6", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddAuditColumn()
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, checkConstraint: "T3>10", version: 2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);


                try
                {
                    dbclient.SqlDataTable(string.Format("insert into {0}(T3) values(5)", testTable));
                    Assert.Fail("Check constraint failed.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("violates check constraint"));
                }

            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_not_null_remove()
        {
            var testTable = "table7";
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 7", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, allowNull: false)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            DropTable(testTable);

            var trackingTable = new TrackingTable(table);


            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(false, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));

                try
                {
                    dbclient.Sql(string.Format("insert into {0}(T2,T3) values({1}p1,null)", testTable, dialect.ParameterPrefix)
                        , DbParameters.New.Input("p1", "AAA2"));
                    Assert.Fail("Not null constraint.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("null value in column"));
                }


                table = new SqlDDLTable(testTable, "test", "test table 7", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddAuditColumn()
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, version: 2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);


                var c = dbclient.Sql(string.Format("insert into {0}(T2, T3) values('BBB',null)", testTable));
                Assert.AreEqual("1", c.ToString());
            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_CheckConstraint_remove()
        {
            var testTable = "table8";
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 8", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 1)
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, checkConstraint: "T3>10")
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            DropTable(testTable);

            var trackingTable = new TrackingTable(table);


            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {

                //create a new table
                dbclient.Sql(false, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "AAA"));

                try
                {
                    dbclient.Sql(string.Format(
                        "insert into {0}(T2,T3) values({1}p1,5)", testTable, dialect.ParameterPrefix)
                        , DbParameters.New.Input("p1", "AAA2"));
                    Assert.Fail("Failed Check constraint.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("violates check constraint"));
                }


                table = new SqlDDLTable(testTable, "test", "test table 8", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, version: 2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);


                var c = dbclient.Sql(string.Format("insert into {0}(T2, T3) values('BBB',5)", testTable));
                Assert.AreEqual("1", c.ToString());
            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_index_remove()
        {
            var testTable = "table9";
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 9", "testspace1", "testindex1", 1, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 1)
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, isIndex: true)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            DropTable(testTable);

            var trackingTable = new TrackingTable(table);


            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(false, sqls);


                //insert data for compare
                dbclient.Sql(string.Format(
                    "insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable(testTable, "test", "test table 5", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, isIndex: false, version: 2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                var count = dbclient.SqlSelect(string.Format(
                    "select count(*) from pg_indexes  where tablename = '{0}'", testTable.ToLower()));

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);


                //check for index
                var count1 = dbclient.SqlSelect(string.Format(
                    "select count(*) from pg_indexes  where tablename = '{0}'", testTable.ToLower()));

                Assert.AreEqual(int.Parse(count.ToString()), int.Parse(count1.ToString()) + 1);
            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_unique_index_remove()
        {
            var testTable = "table10";
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 10", "testspace1", "testindex1", 1, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 1)
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, isIndex: true)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            DropTable(testTable);

            var trackingTable = new TrackingTable(table);


            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(false, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable(testTable, "test", "test table 10", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 0, false, "ABC", "TEST C2", isUnique: false, isIndex: false, version: 2)
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, isIndex: true)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                var count = dbclient.SqlSelect(string.Format(
                    "select count(*) from pg_indexes where tablename = '{0}'", testTable.ToLower()));

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);


                //check for index (Oracle unique constraint create unique index automatically. we need remove unique constraint to remove the index.
                var count1 = dbclient.SqlSelect(string.Format(
                    "select count(*) from pg_indexes where tablename = '{0}'", testTable.ToLower()));

                Assert.AreEqual(int.Parse(count.ToString()), int.Parse(count1.ToString()) + 1);
            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_unique_add()
        {
            var testTable = "table11";

            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 11", "testspace1", "testindex1", 1, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            DropTable(testTable);

            var trackingTable = new TrackingTable(table);


            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(false, sqls);


                //insert data for compare
                dbclient.Sql(string.Format(
                    "insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable(testTable, "test", "test table 11", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: "", isUnique: true, version: 2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);


                try
                {
                    dbclient.SqlDataTable(string.Format("insert into {0}(T2, T3) values('BBB',16)", testTable));
                    Assert.Fail("Unique constraint failed.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("unique constraint"));
                }
            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_default_add()
        {
            var testTable = "table12";

            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 12", "testspace1", "testindex1", 1, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                .AddColumn("T3", DbType.Int16, 0)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            DropTable(testTable);

            var trackingTable = new TrackingTable(table);


            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(false, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable(testTable, "test", "test table 12", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 12, version: 2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);


                dbclient.Sql(string.Format("insert into {0}(T2) values('BBB')", testTable));

                var v = dbclient.SqlSelect(string.Format("select T3 from {0} where T2='AAA'", testTable));
                Assert.AreEqual("", v.ToString());

                v = dbclient.SqlSelect(string.Format("select T3 from {0} where T2='BBB'", testTable));
                Assert.AreEqual("12", v.ToString());
            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_CheckConstraint_modify()
        {
            var testTable = "table13";
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 13", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, checkConstraint: "T3>10 and T3<20")
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            DropTable(testTable);

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);

            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(false, sqls);

                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2,T3) values({1}p1,15)", testTable, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));

                try
                {
                    //insert data for compare
                    dbclient.Sql(string.Format("insert into {0}(T2,T3) values({1}p1,-1)", testTable, dialect.ParameterPrefix)
                        , DbParameters.New.Input("p1", "AAA"));
                    Assert.Fail("Check constraint failed.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("violates check constraint"));
                }

                table = new SqlDDLTable(testTable, "test", "test table 6", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddAuditColumn()
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, checkConstraint: "T3<1", version: 2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                //clean data before change the constraint.
                dbclient.Sql(string.Format("delete from {0}", testTable));

                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2,T3) values({1}p1,-1)", testTable, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));

                try
                {
                    //insert data for compare
                    dbclient.Sql(string.Format("insert into {0}(T2,T3) values({1}p1,15)", testTable, dialect.ParameterPrefix)
                        , DbParameters.New.Input("p1", "AAA"));
                    Assert.Fail("Check constraint failed.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("violates check constraint"));
                }

            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_default_modify()
        {
            var testTable = "table14";

            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 14", "testspace1", "testindex1", 1, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC")
                ;

            DropTable(testTable);

            var trackingTable = new TrackingTable(table);


            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(false, sqls);

                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable(testTable, "test", "test table 12", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "B", version: 2)
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC")
                    ;

                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);


                dbclient.Sql(string.Format("insert into {0}(T2) values('BBB')", testTable));

                var v = dbclient.SqlSelect(string.Format("select T1 from {0} where T2='AAA'", testTable));
                Assert.AreEqual("A", v.ToString());

                v = dbclient.SqlSelect(string.Format("select T1 from {0} where T2='BBB'", testTable));
                Assert.AreEqual("B", v.ToString());
            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_reference_create_remove()
        {
            var testTable = "table15";
            var testTable2 = "table15_1";

            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 15", "testspace1", "testindex1", 1, "NewTest");
            table.AddPKColumn("Id", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                .AddColumn("T3", DbType.Int16, 0)
                .AddColumn("T4", DbType.UInt64, 0)
                ;

            var table2 = new SqlDDLTable(testTable2, "test", "test table 15_1", "testspace1", "testindex1", 1, "NewTest");
            table2.AddPKColumn("Id", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                .AddColumn("T3", DbType.Int16, 0)
                .AddColumn("T4", DbType.UInt64, 0, reference: "table15:Id")
                ;

            DropTable(testTable2);
            DropTable(testTable);

            var trackingTable = new TrackingTable(table2);


            var sqls = dialect.TableCreateSqls(table);
            var sqls2 = dialect.TableCreateSqls(table2);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(false, sqls);
                dbclient.Sql(false, sqls2);


                //insert data with id
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));

                var id = dbclient.SqlSelect(string.Format("select id from {0} where T2='AAA'", testTable));

                //insert data and reference to fk
                dbclient.Sql(string.Format("insert into {0}(T2,T4) values('BBB',{1}p1)", testTable2, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", id, DbType.Int64));

                try
                {
                    //Failed to insert record without fk
                    dbclient.Sql(string.Format("insert into {0}(T2,T4) values('CCC',{1}p1)", testTable2, dialect.ParameterPrefix)
                        , DbParameters.New.Input("p1", 1234567654, DbType.Int64));

                    Assert.Fail("FK test failed.");

                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("violates foreign key constraint"));
                }

                table2 = new SqlDDLTable(testTable2, "test", "test table 15_1", "testspace1", "testindex1", 3, "NewTest");
                table2.AddPKColumn("Id", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                    .AddColumn("T3", DbType.Int16, 0)
                    .AddColumn("T4", DbType.UInt64, 0, version: 2)
                    ;

                //Analyse the table change
                dialect.AnalyseTableChange(table2, trackingTable);

                sqls = dialect.TableUpdateSqls(table2);
                //update table remove the default
                dbclient.Sql(false, sqls);

                //No issue to insert record without fk
                dbclient.Sql(string.Format("insert into {0}(T2,T4) values('DDD',{1}p1)", testTable2, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", 1234567654, DbType.Int64));


            }
            DropTable(testTable2);
            DropTable(testTable);

        }


        [TestMethod]
        public void TableUpdate_reference_add()
        {
            var testTable = "table16";
            var testTable2 = "table16_1";

            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 16", "testspace1", "testindex1", 1, "NewTest");
            table.AddPKColumn("Id", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                .AddColumn("T3", DbType.Int16, 0)
                .AddColumn("T4", DbType.UInt64, 0)
                ;

            var table2 = new SqlDDLTable(testTable2, "test", "test table 16_1", "testspace1", "testindex1", 1, "NewTest");
            table2.AddPKColumn("Id", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                .AddColumn("T3", DbType.Int16, 0)
                .AddColumn("T4", DbType.UInt64, 0)
                ;

            DropTable(testTable2);
            DropTable(testTable);

            var trackingTable = new TrackingTable(table2);


            var sqls = dialect.TableCreateSqls(table);
            var sqls2 = dialect.TableCreateSqls(table2);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(false, sqls);
                dbclient.Sql(false, sqls2);


                //insert data with id
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable, dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));

                var id = dbclient.SqlSelect(string.Format("select id from {0} where T2='AAA'", testTable));

                //insert data and reference to fk
                dbclient.Sql(string.Format("insert into {0}(T2,T4) values('BBB',{1}p1)", testTable2, dialect.ParameterPrefix)
                    //, DbParameters.New.Input("p1", id)); (bug in new net core postgresql has issue for type convert
                    , DbParameters.New.Input("p1",id,DbType.Int64)); //Explictly specifies a type due to bug in net core postgresql


                table2 = new SqlDDLTable(testTable2, "test", "test table 15_1", "testspace1", "testindex1", 3, "NewTest");
                table2.AddPKColumn("Id", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                    .AddColumn("T3", DbType.Int16, 0)
                    .AddColumn("T4", DbType.UInt64, 0, reference: "table16:Id", version: 2)
                    ;

                //Analyse the table change
                dialect.AnalyseTableChange(table2, trackingTable);

                sqls = dialect.TableUpdateSqls(table2);
                //update table remove the default
                dbclient.Sql(false, sqls);

                try
                {
                    //Failed to insert record without fk
                    dbclient.Sql(string.Format("insert into {0}(T2,T4) values('CCC',{1}p1)", testTable2, dialect.ParameterPrefix)
                        , DbParameters.New.Input("p1", 1234567654, DbType.Int64)); //Explictly specifies a type due to bug in net core postgresql

                    Assert.Fail("FK test failed.");

                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("violates foreign key constraint"));
                }

            }
            DropTable(testTable2);
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_autoIncrease_remove()
        {
            var testTable = "table17";

            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 17", "testspace1", "testindex1", 1, "NewTest");
            table.AddPKColumn("Id", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                ;

            DropTable(testTable);

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);

            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(false, sqls);


                //insert record 1
                dbclient.Sql(string.Format("insert into {0}(T1) values('AAA')", testTable));

                var id = dbclient.SqlSelect(string.Format("select id from {0} where T1='AAA'", testTable));

                //insert record 2
                dbclient.Sql(string.Format("insert into {0}(T1) values('AAA1')", testTable));

                var id2 = dbclient.SqlSelect(string.Format("select id from {0} where T1='AAA1'", testTable));

                Assert.AreEqual((Convert.ToInt32(id) + 1).ToString(), id2.ToString());

                table = new SqlDDLTable(testTable, "test", "test table 17", "testspace1", "testindex1", 2, "NewTest");
                table.AddPKColumn("Id", DbType.UInt64, autoIncrease: false, version: 2)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    ;

                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);


                try
                {
                    //insert record 3 throw exception without auto increase
                    dbclient.Sql(string.Format("insert into {0}(T1) values('AAA2')", testTable));
                    Assert.Fail("Failed to remove auto increase clause.");
                }
                catch { }

                trackingTable = new TrackingTable(table);

                table = new SqlDDLTable(testTable, "test", "test table 17", "testspace1", "testindex1", 2, "NewTest");
                table.AddPKColumn("Id", DbType.UInt64, version: 3) //add auto increase back
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    ;
                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(false, sqls);

                //insert record 3 should not throw exception
                dbclient.Sql(string.Format("insert into {0}(T1) values('AAA4')", testTable));

            }
            DropTable(testTable);

        }


    }
}



