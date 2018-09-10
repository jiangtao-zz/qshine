using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine;
using qshine.Configuration;
using qshine.database;
using System.Data;

namespace qshine.database.oracle.Tests
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
                    dbClient.Sql("drop table "+tableName);
                }
                catch(Exception ex)
                {
                    Log.DevDebug(ex.Message);
                }

                //Try to drop an auto sequence for the table PK
                try
                {
                    dbClient.Sql("drop sequence " + tableName+"_seq");
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
            EnvironmentManager.Boot("app.config");
            _testDb = new Database("testdb");
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
        }

        [TestMethod]
        public void CreateDatabase_BestCase()
        {

            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            Assert.IsFalse(dialect.CanCreate);
            
            //Need create database before start the test
            Assert.IsTrue(dialect.DatabaseExists());
        }

        [TestMethod]
        public void TableNotExists()
        {
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
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

            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
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
                .AddColumn("T15", DbType.VarNumeric, 12,5, defaultValue: 12.123)
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
                .AddColumn("T28", DbType.String, 10000, defaultValue:"CLOB Data")

                ;

            var sql = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                DropTable(testTable);

                var result = dbclient.Sql(true, sql);
                Assert.IsTrue(result);

                dbclient.Sql(string.Format("insert into {0}(T1) values(:p1)",testTable), DbParameters.New.Input("p1", "AAA"));
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
                Assert.AreEqual("ABC  ", data.Rows[0]["T10"].ToString()); //add spaces padding for string fix length
                TimeSpan ticket = DateTime.Now - (DateTime)data.Rows[0]["T11"];
                Assert.IsTrue(ticket.Seconds < 10);
                Assert.AreEqual("123", data.Rows[0]["T12"].ToString());
                Assert.AreEqual("1234", data.Rows[0]["T13"].ToString());
                Assert.AreEqual("12345", data.Rows[0]["T14"].ToString());
                Assert.AreEqual("12.123", data.Rows[0]["T15"].ToString());
                Assert.AreEqual("abcdefg", data.Rows[0]["T16"].ToString());
                Assert.AreEqual("abcdefgf  ", data.Rows[0]["T17"].ToString());//add spaces padding for string fix length
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
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
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

                dbclient.Sql(true, sqls);

                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable,dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "AAA"));

            }
            table.AddColumn("T6", DbType.Decimal, 0, defaultValue: 12.345678);

            var column = table.Columns.SingleOrDefault(x => x.Name == "T6");
            column.IsDirty = true;

            column = table.Columns.SingleOrDefault(x => x.Name == "T1");
            column.IsDirty = true;
            column.PreviousColumn = new TrackingColumn
            {
                ColumnName = "T1",
                ColumnType = DbType.String.ToString(),
                Size = 100
            };
            column.Name = "T1x";
            column.Size = 120;
            column.DefaultValue = "X123";

            sqls = dialect.TableUpdateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                var result = dbclient.Sql(true, sqls);
                Assert.IsTrue(result);


                var data = dbclient.SqlDataTable(string.Format("select * from {0} where T2='AAA'",testTable));
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());
                Assert.AreEqual("12.345678", data.Rows[0]["T6"].ToString());
                Assert.AreEqual("A", data.Rows[0]["T1x"]);

                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)",testTable,dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "BBB"));

                data = dbclient.SqlDataTable(string.Format("select * from {0} where T2='BBB'",testTable));
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
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
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
            DropTable(testTable+"_1");

            using (var dbclient = new DbClient(_testDb))
            {
                var result = dbclient.Sql(true, sqls);
                Assert.IsTrue(result);


                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)",testTable,dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "AAA"));

                var sql = dialect.TableRenameClause(table.TableName, testTable+"_1");

                dbclient.Sql(sql);


                var data = dbclient.SqlDataTable(string.Format("select * from {0} where T2='AAA'",testTable+"_1"));
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());

            }
            DropTable(testTable+"_1");

        }

        [TestMethod]
        public void TableUpdate_remove_default()
        {
            var testTable = "table4";
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
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

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                var result = dbclient.Sql(true, sqls);
                Assert.IsTrue(result);


                //insert data for compare
                dbclient.Sql(string.Format(
                    "insert into {0}(T2) values({1}p1)",testTable,dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "AAA"));

                var column = table.Columns.SingleOrDefault(x => x.Name == "T3");
                column.IsDirty = true;
                column.PreviousColumn = new TrackingColumn
                {
                    ColumnName = "T3",
                    ColumnType = DbType.Int16.ToString(),
                    DefaultValue = "16",
                };
                column.DefaultValue = "";

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);


                dbclient.Sql(string.Format(
                    "insert into {0}(T2) values({1}p1)",testTable,dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "BBB"));


                var data = dbclient.SqlDataTable(string.Format("select * from {0} where T2='AAA'",testTable));
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());

                data = dbclient.SqlDataTable(string.Format("select * from {0} where T2='BBB'",testTable));
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());

            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_add_index_and_notnull()
        {
            var testTable = "table5";
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
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

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {

                //create a new table
                dbclient.Sql(true, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)",testTable,dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));

                table = new SqlDDLTable(testTable, "test", "test table 5", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddAuditColumn()
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, isIndex: true, allowNull: false)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                var column = table.Columns.SingleOrDefault(x => x.Name == "T3");
                column.IsDirty = true;
                column.PreviousColumn = new TrackingColumn
                {
                    ColumnName = "T3",
                    ColumnType = DbType.Int16.ToString(),
                    DefaultValue = "16",
                    AllowNull = true,
                    CheckConstraint = "",
                    IsIndex = false,
                    IsUnique = false,
                    IsPK = false
                };

                var count = dbclient.SqlSelect(string.Format(
                    "select count(*) from sqlite_master  where type = 'index' and tbl_name = '{0}'", testTable));

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);


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
                    "select count(*) from sqlite_master  where type = 'index' and tbl_name = '{0}'", testTable));

                Assert.AreEqual(int.Parse(count.ToString()) + 1, int.Parse(count1.ToString()));

            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_add_checkConstraint()
        {
            var testTable = "table6";
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
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

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(true, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)",testTable,dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable(testTable, "test", "test table 6", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddAuditColumn()
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, checkConstraint: "Check(T3>10)")
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                var column = table.Columns.SingleOrDefault(x => x.Name == "T3");
                column.IsDirty = true;
                column.PreviousColumn = new TrackingColumn
                {
                    ColumnName = "T3",
                    ColumnType = DbType.Int16.ToString(),
                    DefaultValue = "16",
                    AllowNull = true,
                    CheckConstraint = "",
                    IsIndex = false,
                    IsUnique = false,
                    IsPK = false
                };

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);


                try
                {
                    dbclient.SqlDataTable(string.Format("insert into {0}(T3) values(5)",testTable));
                    Assert.Fail("Check constraint failed.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("CHECK"));
                }

            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_remove_not_null()
        {
            var testTable = "table7";
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
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

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(true, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable,dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));

                try
                {
                    dbclient.Sql(string.Format("insert into {0}(T2,T3) values({1}p1,null)", testTable,dialect.ParameterPrefix)
                        , DbParameters.New.Input("p1", "AAA2"));
                    Assert.Fail("Not null constraint.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("NOT NULL"));
                }


                table = new SqlDDLTable(testTable, "test", "test table 7", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddAuditColumn()
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                var column = table.Columns.SingleOrDefault(x => x.Name == "T3");
                column.IsDirty = true;
                column.PreviousColumn = new TrackingColumn
                {
                    ColumnName = "T3",
                    ColumnType = DbType.Int16.ToString(),
                    DefaultValue = "16",
                    AllowNull = false,
                    CheckConstraint = "",
                    IsIndex = false,
                    IsUnique = false,
                    IsPK = false
                };

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);


                var c = dbclient.Sql(string.Format("insert into {0}(T2, T3) values('BBB',null)", testTable));
                Assert.AreEqual("1", c.ToString());
            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_remove_CheckConstraint()
        {
            var testTable = "table8";
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(_testDb.ConnectionString);

            var table = new SqlDDLTable(testTable, "test", "test table 8", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 1)
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, checkConstraint: "Check(T3>10)")
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            DropTable(testTable);

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {

                //create a new table
                dbclient.Sql(true, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable,dialect.ParameterPrefix), 
                    DbParameters.New.Input("p1", "AAA"));

                try
                {
                    dbclient.Sql(string.Format(
                        "insert into {0}(T2,T3) values({1}p1,5)", testTable,dialect.ParameterPrefix)
                        , DbParameters.New.Input("p1", "AAA2"));
                    Assert.Fail("Not null constraint.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("CHECK"));
                }


                table = new SqlDDLTable(testTable, "test", "test table 8", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                var column = table.Columns.SingleOrDefault(x => x.Name == "T3");
                column.IsDirty = true;
                column.PreviousColumn = new TrackingColumn
                {
                    ColumnName = "T3",
                    ColumnType = DbType.Int16.ToString(),
                    DefaultValue = "16",
                    AllowNull = false,
                    CheckConstraint = "Check(T3>10)",
                    IsIndex = false,
                    IsUnique = false,
                    IsPK = false
                };

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);


                var c = dbclient.Sql(string.Format("insert into {0}(T2, T3) values('BBB',5)", testTable));
                Assert.AreEqual("1", c.ToString());
            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_remove_index()
        {
            var testTable = "table9";
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
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

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(true, sqls);


                //insert data for compare
                dbclient.Sql(string.Format(
                    "insert into {0}(T2) values({1}p1)", testTable,dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable(testTable, "test", "test table 5", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, isIndex: false)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                var column = table.Columns.SingleOrDefault(x => x.Name == "T3");
                column.IsDirty = true;
                column.PreviousColumn = new TrackingColumn
                {
                    ColumnName = "T3",
                    ColumnType = DbType.Int16.ToString(),
                    DefaultValue = "16",
                    AllowNull = true,
                    CheckConstraint = "",
                    IsIndex = true,
                    IsUnique = false,
                    IsPK = false
                };

                var count = dbclient.SqlSelect(string.Format(
                    "select count(*) from sqlite_master  where type = 'index' and tbl_name = '{0}'", testTable));

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);


                //check for index
                var count1 = dbclient.SqlSelect(string.Format(
                    "select count(*) from sqlite_master  where type = 'index' and tbl_name = '{0}'", testTable));

                Assert.AreEqual(int.Parse(count.ToString()), int.Parse(count1.ToString()) + 1);
            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_remove_unique_index()
        {
            var testTable = "table10";
            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
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

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(true, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({1}p1)", testTable,dialect.ParameterPrefix),
                    DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable(testTable, "test", "test table 10", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 0, false, "ABC", "TEST C2", isUnique: true, isIndex: false, version: 2)
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, isIndex: true)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                var column = table.Columns.SingleOrDefault(x => x.Name == "T2");
                column.IsDirty = true;
                column.PreviousColumn = new TrackingColumn
                {
                    ColumnName = "T2",
                    ColumnType = DbType.String.ToString(),
                    Size = 1000,
                    Scale = 0,
                    AllowNull = false,
                    DefaultValue = "ABC",
                    CheckConstraint = "",
                    IsIndex = true,
                    IsUnique = true,
                    IsPK = false
                };

                var count = dbclient.SqlSelect(string.Format(
                    "select count(*) from sqlite_master  where type = 'index' and tbl_name = '{0}'", testTable));

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);


                //check for index
                var count1 = dbclient.SqlSelect(string.Format(
                    "select count(*) from sqlite_master  where type = 'index' and tbl_name = '{0}'", testTable));

                Assert.AreEqual(int.Parse(count.ToString()), int.Parse(count1.ToString()) + 1);
            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_add_unique()
        {
            var testTable = "table11";

            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
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

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(true, sqls);


                //insert data for compare
                dbclient.Sql(string.Format(
                    "insert into {0}(T2) values({1}p1)", testTable,dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable(testTable, "test", "test table 11", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: "", isUnique: true)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                var column = table.Columns.SingleOrDefault(x => x.Name == "T3");
                column.IsDirty = true;
                column.PreviousColumn = new TrackingColumn
                {
                    ColumnName = "T3",
                    ColumnType = DbType.Int16.ToString(),
                    DefaultValue = "16",
                    AllowNull = true,
                    CheckConstraint = "",
                    IsIndex = false,
                    IsUnique = false,
                    IsPK = false
                };

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);


                try
                {
                    dbclient.SqlDataTable(string.Format("insert into {0}(T2, T3) values('BBB',16)", testTable));
                    Assert.Fail("Unique constraint failed.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("UNIQUE"));
                }
            }
            DropTable(testTable);

        }

        [TestMethod]
        public void TableUpdate_add_default()
        {
            var testTable = "table12";

            var dialectProvider = EnvironmentManager.GetProvider<ISqlDialectProvider>();
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

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(_testDb))
            {
                //create a new table
                dbclient.Sql(true, sqls);


                //insert data for compare
                dbclient.Sql(string.Format("insert into {0}(T2) values({0}p1)", testTable,dialect.ParameterPrefix)
                    , DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable(testTable, "test", "test table 12", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 1000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 12)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                var column = table.Columns.SingleOrDefault(x => x.Name == "T3");
                column.IsDirty = true;
                column.PreviousColumn = new TrackingColumn
                {
                    ColumnName = "T3",
                    ColumnType = DbType.Int16.ToString(),
                    DefaultValue = "",
                    AllowNull = true,
                    CheckConstraint = "",
                    IsIndex = false,
                    IsUnique = false,
                    IsPK = false
                };

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);


                dbclient.Sql(string.Format("insert into {0}(T2) values('BBB')", testTable));

                var v = dbclient.SqlSelect(string.Format("select T3 from {0} where T2='AAA'", testTable));
                Assert.AreEqual("", v.ToString());

                v = dbclient.SqlSelect(string.Format("select T3 from {0} where T2='BBB'", testTable));
                Assert.AreEqual("12", v.ToString());
            }
            DropTable(testTable);

        }


    }
}



