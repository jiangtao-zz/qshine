using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine;
using qshine.Configuration;
using qshine.database;
using System.Data;

namespace qshine.database.sqlite.Tests
{
    [TestClass]
    public class SetupAssemblyInitializer
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {

            Log.SysLoggerProvider = new TraceLoggerProvider();
            Log.SysLogger.EnableLogging(System.Diagnostics.TraceEventType.Verbose);

            //This is only running once. Ignore subsequently call ApplicationEnvironment.Boot().
            ApplicationEnvironment.Build("app.config");
        }
    }


    [TestClass]
    public class DialectTests
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
        }

        [TestMethod]
        public void CreateDatabase_BestCase()
        {

            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);

            Assert.IsTrue(dialect.CanCreate);
            if (!dialect.DatabaseExists())
            {
                Assert.IsTrue(dialect.CreateDatabase());
            }
            Assert.IsTrue(dialect.DatabaseExists());
        }

        [TestMethod]
        public void TableNotExists()
        {
            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
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
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table1", "test", "test table 1", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100)
                .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                .AddColumn("T6", DbType.Decimal, 0, defaultValue: 12.345678)
                .AddColumn("T7", DbType.Double, 0, defaultValue: 22.345678)
                .AddColumn("T8", DbType.SByte, 0, defaultValue: 8)
                .AddColumn("T9", DbType.Single, 0, defaultValue: 123.456)
                .AddColumn("T10", DbType.StringFixedLength, 3, defaultValue: "ABC")
                .AddColumn("T11", DbType.Time, 0, defaultValue: SqlReservedWord.SysDate)
                .AddColumn("T12", DbType.UInt16, 0, defaultValue: 123)
                .AddColumn("T13", DbType.UInt32, 0, defaultValue: 1234)
                .AddColumn("T14", DbType.UInt64, 0, defaultValue: 12345)
                .AddColumn("T15", DbType.VarNumeric, 12, defaultValue: 12.123)
                .AddColumn("T16", DbType.AnsiString, 100, defaultValue: "abcdefg")
                .AddColumn("T17", DbType.AnsiStringFixedLength, 100, defaultValue: "abcdefgf")
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
                ;

            var sql = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                var result = dbclient.Sql(true, sql);
                Assert.IsTrue(result);

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
                Assert.AreEqual("ABC", data.Rows[0]["T10"].ToString());
                TimeSpan ticket = DateTime.Now - (DateTime)data.Rows[0]["T11"];
                Assert.IsTrue(ticket.Seconds < 10);
                Assert.AreEqual("123", data.Rows[0]["T12"].ToString());
                Assert.AreEqual("1234", data.Rows[0]["T13"].ToString());
                Assert.AreEqual("12345", data.Rows[0]["T14"].ToString());
                Assert.AreEqual("12.123", data.Rows[0]["T15"].ToString());
                Assert.AreEqual("abcdefg", data.Rows[0]["T16"].ToString());
                Assert.AreEqual("abcdefgf", data.Rows[0]["T17"].ToString());
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

                dbclient.Sql("drop table table1;");
            }
        }

        [TestMethod]
        public void TableUpdate_Rename_column_add_new_column()
        {
            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table2", "test", "test table 2", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue:"A", version:2)
                .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var trackingTable = new TrackingTable(table);


            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                try { dbclient.Sql("drop table table2;"); } catch { }
                //Create a new table
                dbclient.Sql(true, sqls);

                //insert a new record
                dbclient.Sql("insert into table2(T2) values(@p1)", DbParameters.New.Input("p1", "AAA"));

            }
            //Update the table structure by adding a new column
            table.AddColumn("T6", DbType.Decimal, 0, defaultValue: 12.345678);

            //Update the table structure by rename an existing column and also update the size and default value.
            var column = table.Columns.SingleOrDefault(x => x.Name == "T1");
            column.Name = "T1x";
            column.Size = 120;
            column.DefaultValue = "X123";

            trackingTable.Version--;//produce a version change case.
            var trackingT1Column = trackingTable.Columns.SingleOrDefault(x => x.ColumnName == "T1");
            trackingT1Column.Version--;

            //Analyse the table change
            dialect.AnalyseTableChange(table, trackingTable);

            //Get table structure update SQLs
            sqls = dialect.TableUpdateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                var result = dbclient.Sql(false, sqls);

                var data = dbclient.SqlDataTable("select * from table2 where T2='AAA'");
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());
                Assert.AreEqual("12.345678", data.Rows[0]["T6"].ToString());
                Assert.AreEqual("A", data.Rows[0]["T1x"]);

                dbclient.Sql("insert into table2(T2) values(@p1)", DbParameters.New.Input("p1", "BBB"));
                data = dbclient.SqlDataTable("select * from table2 where T2='BBB'");
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());
                Assert.AreEqual("12.345678", data.Rows[0]["T6"].ToString());
                Assert.AreEqual("X123", data.Rows[0]["T1x"]);


                dbclient.Sql("drop table table2;");
            }

        }

        [TestMethod]
        public void TableUpdate_Rename_table()
        {
            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table3", "test", "test table 3", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                try { dbclient.Sql("drop table table3;"); } catch { }
                try { dbclient.Sql("drop table table3_1;"); } catch { }

                var result = dbclient.Sql(true, sqls);


                dbclient.Sql("insert into table3(T2) values(@p1)", DbParameters.New.Input("p1", "AAA"));

                var sql = dialect.TableRenameClause(table.TableName, "table3_1");
                dbclient.Sql(sql);

                var data = dbclient.SqlDataTable("select * from table3_1 where T2='AAA'");
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());

                dbclient.Sql("drop table table3_1;");
            }

        }

        [TestMethod]
        public void TableUpdate_remove_default()
        {
            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table4", "test", "test table 4", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                //drop table if exists
                try { dbclient.Sql("drop table table4;"); } catch { }

                //create a new table
                var result = dbclient.Sql(true, sqls);


                //insert data for compare
                dbclient.Sql("insert into table4(T2) values(@p1)", DbParameters.New.Input("p1", "AAA"));

                table = new SqlDDLTable("table4", "test", "test table 4", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddAuditColumn()
                    .AddColumn("T3", DbType.Int16, 0, version:2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;


                //Analyse the table change
                dialect.AnalyseTableChange(table, trackingTable);


                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);


                dbclient.Sql("insert into table4(T2) values(@p1)", DbParameters.New.Input("p1", "BBB"));


                var data = dbclient.SqlDataTable("select * from table4 where T2='AAA'");
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());

                data = dbclient.SqlDataTable("select * from table4 where T2='BBB'");
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());


                dbclient.Sql("drop table table4;");
            }
        }

        [TestMethod]
        public void TableUpdate_add_index_and_notnull()
        {
            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table5", "test", "test table 5", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                //drop table if exists
                try { dbclient.Sql("drop table table5;"); } catch { }

                //create a new table
                var result = dbclient.Sql(true, sqls);


                //insert data for compare
                dbclient.Sql("insert into table5(T2) values(@p1)", DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable("table5", "test", "test table 5", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddAuditColumn()
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16,isIndex:true,allowNull:false, version:2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                dialect.AnalyseTableChange(table, trackingTable);


                var count = dbclient.SqlSelect("select count(*) from sqlite_master  where type = 'index' and tbl_name = 'table5';");

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);


                var data = dbclient.SqlDataTable("select * from table5 where T2='AAA'");
                Assert.AreEqual(1, data.Rows.Count);
                Assert.AreEqual("16", data.Rows[0]["T3"].ToString());
                Assert.AreEqual("32", data.Rows[0]["T4"].ToString());
                Assert.AreEqual("1234567890", data.Rows[0]["T5"].ToString());

                try
                {
                    dbclient.SqlDataTable("insert into table5(T3) values(NULL)");
                    Assert.Fail("Not Null constraint failed.");
                }
                catch { }


                //check for index
                var count1 = dbclient.SqlSelect("select count(*) from sqlite_master  where type = 'index' and tbl_name = 'table5';");

                Assert.AreEqual(int.Parse(count.ToString())+1, int.Parse(count1.ToString()));

                dbclient.Sql("drop table table5;");
            }
        }

        [TestMethod]
        public void TableUpdate_add_checkConstraint()
        {
            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table6", "test", "test table 6", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                //drop table if exists
                try { dbclient.Sql("drop table table6;"); } catch { }

                //create a new table
                dbclient.Sql(true, sqls);

                //insert data for compare
                dbclient.Sql("insert into table6(T2) values(@p1)", DbParameters.New.Input("p1", "AAA"));

                table = new SqlDDLTable("table6", "test", "test table 6", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddAuditColumn()
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, checkConstraint:"T3>10", version:2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);

                try
                {
                    dbclient.SqlDataTable("insert into table6(T3) values(5)");
                    Assert.Fail("Check constraint failed.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("CHECK"));
                }

                dbclient.Sql("drop table table6;");
            }
        }

        [TestMethod]
        public void TableUpdate_remove_not_null()
        {
            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table7", "test", "test table 7", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                .AddAuditColumn()
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16,allowNull:false)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                //drop table if exists
                try { dbclient.Sql("drop table table7;"); } catch { }

                //create a new table
                dbclient.Sql(true, sqls);

                //insert data for compare
                dbclient.Sql("insert into table7(T2) values(@p1)", DbParameters.New.Input("p1", "AAA"));

                try
                {
                    dbclient.Sql("insert into table7(T2,T3) values(@p1,null)", DbParameters.New.Input("p1", "AAA2"));
                    Assert.Fail("Not null constraint.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("NOT NULL"));
                }


                table = new SqlDDLTable("table7", "test", "test table 7", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddAuditColumn()
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, version:3)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);

                var c = dbclient.Sql("insert into table7(T2, T3) values('BBB',null)");
                Assert.AreEqual("1", c.ToString());

                dbclient.Sql("drop table table7;");
            }
        }

        [TestMethod]
        public void TableUpdate_remove_CheckConstraint()
        {
            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table8", "test", "test table 8", "testspace1", "testindex1", 2, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 1)
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16,checkConstraint:"T3>10")
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                //drop table if exists
                try { dbclient.Sql("drop table table8;"); } catch { }

                //create a new table
                dbclient.Sql(true, sqls);

                //insert data for compare
                dbclient.Sql("insert into table8(T2) values(@p1)", DbParameters.New.Input("p1", "AAA"));

                try
                {
                    dbclient.Sql("insert into table8(T2,T3) values(@p1,5)", DbParameters.New.Input("p1", "AAA2"));
                    Assert.Fail("Not null constraint.");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("CHECK"));
                }


                table = new SqlDDLTable("table8", "test", "test table 8", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2, oldColumnNames: "T21,T22".Split(','))
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, version:2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);

                var c = dbclient.Sql("insert into table8(T2, T3) values('BBB',5)");
                Assert.AreEqual("1", c.ToString());

                dbclient.Sql("drop table table8;");
            }
        }

        [TestMethod]
        public void TableUpdate_remove_index()
        {
            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table9", "test", "test table 9", "testspace1", "testindex1", 1, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 1)
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, isIndex: true)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                //drop table if exists
                try { dbclient.Sql("drop table table9;"); } catch { }

                //create a new table
                dbclient.Sql(true, sqls);

                //insert data for compare
                dbclient.Sql("insert into table9(T2) values(@p1)", DbParameters.New.Input("p1", "AAA"));

                var count = dbclient.SqlSelect("select count(*) from sqlite_master  where type = 'index' and tbl_name = 'table9';");


                table = new SqlDDLTable("table9", "test", "test table 5", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, isIndex: false, version:3)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);

                //check for index
                var count1 = dbclient.SqlSelect("select count(*) from sqlite_master  where type = 'index' and tbl_name = 'table9';");

                Assert.AreEqual(int.Parse(count.ToString()), int.Parse(count1.ToString())+1);

                dbclient.Sql("drop table table9;");
            }
        }

        [TestMethod]
        public void TableUpdate_remove_unique_index()
        {
            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table10", "test", "test table 10", "testspace1", "testindex1", 1, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 1)
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, isIndex: true)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                //drop table if exists
                try { dbclient.Sql("drop table table10;"); } catch { }

                //create a new table
                dbclient.Sql(true, sqls);

                //insert data for compare
                dbclient.Sql("insert into table10(T2) values(@p1)", DbParameters.New.Input("p1", "AAA"));

                var count = dbclient.SqlSelect("select count(*) from sqlite_master  where type = 'index' and tbl_name = 'table10';");

                table = new SqlDDLTable("table10", "test", "test table 10", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 2000, 0, false, "ABC", "TEST C2", isUnique: true, isIndex: false, version: 2)
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 16, isIndex: true)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);

                //check for index
                var count1 = dbclient.SqlSelect("select count(*) from sqlite_master  where type = 'index' and tbl_name = 'table10';");

                Assert.AreEqual(int.Parse(count.ToString()), int.Parse(count1.ToString()) + 1);

                dbclient.Sql("drop table table10;");
            }
        }

        [TestMethod]
        public void TableUpdate_add_unique()
        {
            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table11", "test", "test table 11", "testspace1", "testindex1", 1, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                .AddColumn("T3", DbType.Int16, 0, defaultValue: 16)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                //drop table if exists
                try { dbclient.Sql("drop table table11;"); } catch { }

                //create a new table
                dbclient.Sql(true, sqls);

                //insert data for compare
                dbclient.Sql("insert into table11(T2) values(@p1)", DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable("table11", "test", "test table 11", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: "", isUnique: true, version:2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);

                try
                {
                    dbclient.SqlDataTable("insert into table11(T2, T3) values('BBB',16)");
                    Assert.Fail("Unique constraint failed.");
                }
                catch(Exception ex)
                {
                    Assert.IsTrue(ex.Message.Contains("UNIQUE"));
                }


                dbclient.Sql("drop table table11;");
            }
        }

        [TestMethod]
        public void TableUpdate_add_default()
        {
            var database = new Database("testdb");
            var dialectProvider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            var dialect = dialectProvider.GetSqlDialect(database.ConnectionString);
            dialect.CreateDatabase(); //ensure database exists

            var table = new SqlDDLTable("table12", "test", "test table 12", "testspace1", "testindex1", 1, "NewTest");
            table.AddPKColumn("PKC", DbType.UInt64)
                .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                .AddColumn("T3", DbType.Int16, 0)
                .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                ;

            var trackingTable = new TrackingTable(table);

            var sqls = dialect.TableCreateSqls(table);
            using (var dbclient = new DbClient(database))
            {
                //drop table if exists
                try { dbclient.Sql("drop table table12;"); } catch { }

                //create a new table
                dbclient.Sql(true, sqls);

                //insert data for compare
                dbclient.Sql("insert into table12(T2) values(@p1)", DbParameters.New.Input("p1", "AAA"));


                table = new SqlDDLTable("table12", "test", "test table 12", "testspace1", "testindex1", 3, "NewTest");
                table.AddPKColumn("PKC", DbType.UInt64)
                    .AddColumn("T1", DbType.String, 100, defaultValue: "A")
                    .AddColumn("T2", DbType.String, 2000, 12, false, "ABC", "TEST C2", isUnique: true, isIndex: true, version: 2)
                    .AddColumn("T3", DbType.Int16, 0, defaultValue: 12, version:2)
                    .AddColumn("T4", DbType.Int32, 0, defaultValue: 32)
                    .AddColumn("T5", DbType.Int64, 0, defaultValue: 1234567890)
                    ;

                dialect.AnalyseTableChange(table, trackingTable);

                sqls = dialect.TableUpdateSqls(table);
                //update table remove the default
                dbclient.Sql(true, sqls);

                dbclient.Sql("insert into table12(T2) values('BBB')");

                var v = dbclient.SqlSelect("select T3 from table12 where T2='AAA'");
                Assert.AreEqual("",v.ToString());

                v = dbclient.SqlSelect("select T3 from table12 where T2='BBB'");
                Assert.AreEqual("12",v.ToString());


                dbclient.Sql("drop table table12;");
            }
        }


    }
}


