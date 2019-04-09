using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Configuration;
using qshine.Logger;


namespace qshine.database.Tests
{
    [TestClass]
    public class GenericDDLTests
    {
        const int MaxNameLength = 30;
        private static Database _testDb;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            //Log.SysLoggerProvider = new TraceLoggerProvider();
            //Log.SysLogger.EnableLogging(System.Diagnostics.TraceEventType.Verbose);
            //UnitOfWork.Providers = new List<IUnitOfWorkProvider>();
            //UnitOfWork.Providers.Add(new TransactionScopeUnitOfWorkProvider());

            ApplicationEnvironment.Build("app.config");
            _testDb = new Database("testdb");
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
        }

        [TestMethod]
        public void Table_HashCode_Test()
        {
            var hash1 = new SampleTable2();
            var hash2 = new SampleTable2();

            for(int i = 0; i < hash1.Columns.Count;i++)
            {
                Assert.AreEqual(hash1.Columns[i].HashCode, hash2.Columns[i].HashCode);
            }

            Assert.AreEqual(hash1.HashCode, hash2.HashCode);
            //Assert.AreEqual(-12739959704L, hash2.HashCode);
        }


        [TestMethod]
        public void Table_DDL_Test()
        {
            DropTable("sample_t2");
            DropTable("sample_t12");
            DropTable("sample_t1");
            DropTable("sample_t11");

            using (var dbBuilder = new SqlDDLBuilder(_testDb))
            {
                //Register Common tables
                dbBuilder.Database
                    .AddTable(new SampleTable1())
                    .AddTable(new SampleTable2());

                dbBuilder.Build(BatchException.LastException, true);
            }
            //Update table
            using (var dbBuilder = new SqlDDLBuilder(_testDb))
            {
                dbBuilder.Database
                    //Register Common tables
                    .AddTable(new SampleTable1())
                    .AddTable(new SampleTable2_1());

                dbBuilder.Build(BatchException.LastException, true);

            }
            //Update rename
            using (var dbBuilder = new SqlDDLBuilder(_testDb))
            {
                dbBuilder.Database
                    //Register Common tables
                    .AddTable(new SampleTable1_rename())
                    .AddTable(new SampleTable2_2());

                dbBuilder.Build(BatchException.LastException, true);

            }
            //Update rename2
            using (var dbBuilder = new SqlDDLBuilder(_testDb))
            {
                dbBuilder.Database
                    //Register Common tables
                    .AddTable(new SampleTable1_rename2())
                    .AddTable(new SampleTable2_3());

                dbBuilder.Build(BatchException.LastException, true);

            }
        }

        [TestMethod]
        public void GetName_Test()
        {
            string unName = SqlDDLTable.GetUniqueKeyName("T12345678901234567890_1234567890", 1234567);
            Assert.IsTrue(unName.Length <= MaxNameLength);

            var table = new SqlDDLTable("T123", "C1", "Comment 1", "tableSpace1", "indexTableSpace", 12, "S1");

            Assert.AreEqual("T123", table.TableName);
            Assert.AreEqual("C1", table.Category);
            Assert.AreEqual("Comment 1", table.Comments);
            Assert.AreEqual("tableSpace1", table.TableSpace);
            Assert.AreEqual("indexTableSpace", table.IndexTableSpace);
            Assert.AreEqual(12, table.Version);
            Assert.AreEqual("S1", table.SchemaName);

        }

        [TestMethod]
        public void UoW_SimpleUoW_Test()
        {
            //Drop table
            DropTable("sampleTable_o1");
            //Create a new table
            var table = new SampleTable_other();
            using (var dbBuilder = new SqlDDLBuilder(_testDb))
            {
                //Register Common tables
                dbBuilder.Database
                    .AddTable(table);

                dbBuilder.Build(BatchException.LastException, true);
            }

            //Start UoW
            using(var unitwork = new UnitOfWork())
            {
                //database operation
                using(var db = new DbClient(_testDb))
                {
                    byte[] b = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
                    db.Insert(table.TableName,
                        "v1_int32,v2_string,v3_long,v4_date,v5_decimal,v6_Boolean,v7_clob,v8_guid",
                        99, "ABC", 123456789012345L, 
                        new DateTime(2010, 1, 2, 3, 4, 5, 6), 
                        12.2M, true, "11111", Guid.NewGuid());
                }
                //complete UoW
                unitwork.Complete();
            }
            using (var db = new DbClient(_testDb))
            {
                var result = db.Retrieve<Dictionary<string, object>>(
                    (x) =>
                    {
                        var d = new Dictionary<string, object>
                        {
                            { "v1_int32", x.ReadInt32("v1_int32") },
                            { "v2_string", x.ReadString("v2_string") },
                            { "v4_date", x.ReadDateTime("v4_date") },
                            { "v5_decimal", x.ReadDecimal("v5_decimal") },
                            { "v6_Boolean", x.ReadBoolean("v6_Boolean") },
                            { "v7_clob", x.ReadString("v7_clob") },
                            { "v8_guid", x.ReadGuid("v8_guid") },
                            { "v9_binary", x.ReadBytes("v9_binary") }
                        };
                        return d;
                    },
                    string.Format("select v1_int32,v2_string,v3_long,v4_date,v5_decimal,v6_Boolean,v7_clob,v8_guid,v9_binary from {0} where v3_long=123456789012345",
                    table.TableName));
                Assert.AreEqual(99, Convert.ToInt32(result[0]["v1_int32"]));
            }
        }

        [TestMethod]
        public void Unitwork_level2_Tests()
        {
            var table1 = new SampleTable_u("sampleTable_u1");
            var table2 = new SampleTable_u("sampleTable_u2");
            var table3 = new SampleTable_u("sampleTable_u3");
            DropTable(table1.TableName);
            DropTable(table2.TableName);
            DropTable(table3.TableName);

            using (var dbBuilder = new SqlDDLBuilder(_testDb))
            {
                //Register Common tables
                dbBuilder.Database
                    .AddTable(table1)
                    .AddTable(table2)
                    .AddTable(table3);

                dbBuilder.Build(BatchException.LastException, true);
            }

            //top-most UoW
            using (var uow = new UnitOfWork())
            {
                //databse operation
                using (var db = new DbClient(_testDb))
                {
                    db.Insert(table1.TableName,
                        "v1_int32,v2_string",
                        1, "A1");
                }
                //nested UoW
                using (var uow2 = new UnitOfWork())
                {
                    //database operation
                    using (var db = new DbClient(_testDb))
                    {
                        db.Insert(table2.TableName,
                            "v1_int32,v2_string",
                            2, "A2");
                    }
                    //complete nested UoW
                    uow2.Complete();
                }
                //database operation
                using (var db = new DbClient(_testDb))
                {
                    db.Insert(table3.TableName,
                        "v1_int32,v2_string",
                        3, "A3");
                }
                //complete UoW
                uow.Complete();
            }

            using (var db = new DbClient(_testDb))
            {
                var v1 = db.SqlSelect($"select v2_string from {table1.TableName} where v1_int32=1");
                var v2 = db.SqlSelect($"select v2_string from {table2.TableName} where v1_int32=2");
                var v3 = db.SqlSelect($"select v2_string from {table3.TableName} where v1_int32=3");
                Assert.AreEqual("A1", v1.ToString());
                Assert.AreEqual("A2", v2.ToString());
                Assert.AreEqual("A3", v3.ToString());
            }
        }

        [TestMethod]
        public void UoW_level2_Tests()
        {
            var table1 = new SampleTable_u("sampleTable_u11");
            var table2 = new SampleTable_u("sampleTable_u12");
            var table3 = new SampleTable_u("sampleTable_u13");
            DropTable(table1.TableName);
            DropTable(table2.TableName);
            DropTable(table3.TableName);

            using (var dbBuilder = new SqlDDLBuilder(_testDb))
            {
                //Register Common tables
                dbBuilder.Database
                    .AddTable(table1)
                    .AddTable(table2)
                    .AddTable(table3);

                dbBuilder.Build(BatchException.LastException, true);
            }
            //child roleback
            using (var unitwork = new UnitOfWork())
            {
                using (var db = new DbClient(_testDb))
                {
                    db.Insert(table1.TableName,
                        "v1_int32,v2_string",
                        1, "A1");
                }

                using (var uow2 = new UnitOfWork())
                {
                    using (var db = new DbClient(_testDb))
                    {
                        db.Insert(table2.TableName,
                            "v1_int32,v2_string",
                            2, "A2");
                    }
                    //Note::TransactionScope do not allow failed nested transaction continue running to next connection.
                    //throw exception to prevent continue next

                    //uow2.Complete();
                }

                using (var db = new DbClient(_testDb))
                {
                    db.Insert(table3.TableName,
                        "v1_int32,v2_string",
                        3, "A3");
                }

                unitwork.Complete();
            }

            using (var db = new DbClient(_testDb))
            {
                var v1 = db.SqlSelect($"select v2_string from {table1.TableName} where v1_int32=1");
                var v2 = db.SqlSelect($"select v2_string from {table2.TableName} where v1_int32=2");
                var v3 = db.SqlSelect($"select v2_string from {table3.TableName} where v1_int32=3");
                Assert.IsNull(v1);
                Assert.IsNull(v2);
                Assert.IsNull(v3);
            }

            //parent rollback
            using (var unitwork = new UnitOfWork())
            {
                using (var db = new DbClient(_testDb))
                {
                    db.Insert(table1.TableName,
                        "v1_int32,v2_string",
                        1, "A1");
                }

                using (var uow2 = new UnitOfWork())
                {
                    using (var db = new DbClient(_testDb))
                    {
                        db.Insert(table2.TableName,
                            "v1_int32,v2_string",
                            2, "A2");
                    }
                    uow2.Complete();
                }

                using (var db = new DbClient(_testDb))
                {
                    db.Insert(table3.TableName,
                        "v1_int32,v2_string",
                        3, "A3");
                }

                //unitwork.Complete();
            }

            using (var db = new DbClient(_testDb))
            {
                var v1 = db.SqlSelect($"select v2_string from {table1.TableName} where v1_int32=1");
                var v2 = db.SqlSelect($"select v2_string from {table2.TableName} where v1_int32=2");
                var v3 = db.SqlSelect($"select v2_string from {table3.TableName} where v1_int32=3");
                Assert.IsNull(v1);
                Assert.IsNull(v2);
                Assert.IsNull(v3);
            }

            //parent rollback and child isolate commit
            Task task1=null;
            bool isSqlite = false;
            using (var unitwork = new UnitOfWork())
            {
                using (var db = new DbClient(_testDb))
                {
                    db.Insert(table1.TableName,
                        "v1_int32,v2_string",
                        1, "A1");

                    isSqlite = db.Session.Database.ProviderName.Contains("SQLite");
                }

                //Sqlite cannot have nested transaction in same thread.
                //using Non block thread to process second transaction
                if (isSqlite)
                {
                    task1 = AddDataAsyncUoW(table2.TableName, 2, "A2", true, true);
                }
                else
                {
                    using (var uow2 = new UnitOfWork(UnitOfWorkOption.Suppress))
                    {
                        using (var db = new DbClient(_testDb))
                        {
                            db.Insert(table2.TableName,
                                "v1_int32,v2_string",
                                2, "A2");
                        }
                        uow2.Complete();
                    }
                }

                using (var db = new DbClient(_testDb))
                {
                    db.Insert(table3.TableName,
                        "v1_int32,v2_string",
                        3, "A3");
                }

                //unitwork.Complete();
            }

            if (isSqlite)
            {
                task1.Wait();
            }

            using (var db = new DbClient(_testDb))
            {
                var v1 = db.SqlSelect($"select v2_string from {table1.TableName} where v1_int32=1");
                var v2 = db.SqlSelect($"select v2_string from {table2.TableName} where v1_int32=2");
                var v3 = db.SqlSelect($"select v2_string from {table3.TableName} where v1_int32=3");
                Assert.IsNull(v1);
                Assert.AreEqual("A2",v2.ToString());
                Assert.IsNull(v3);
            }
        }

        [TestMethod]
        public void UoW_IsolateUoW_Tests()
        {
            var table1 = new SampleTable_u("sampleTable_u111");
            var table2 = new SampleTable_u("sampleTable_u112");
            var table3 = new SampleTable_u("sampleTable_u113");
            DropTable(table1.TableName);
            DropTable(table2.TableName);
            DropTable(table3.TableName);

            bool isSqlite = false;

            using (var dbBuilder = new SqlDDLBuilder(_testDb))
            {
                //Register Common tables
                dbBuilder.Database
                    .AddTable(table1)
                    .AddTable(table2)
                    .AddTable(table3);

                dbBuilder.Build(BatchException.LastException, true);
                isSqlite = dbBuilder.Database.Database.ProviderName.Contains("SQLite");
            }

            //parent UoW contains isolated UoW
            using (var parentUoW = new UnitOfWork())
            {
                //database operation
                using (var db = new DbClient(_testDb))
                {
                    db.Insert(table1.TableName,
                        "v1_int32,v2_string", 1, "A1");
                }
                if(!isSqlite)
                {
                    //isolated UoW
                    using (var uow2 = new UnitOfWork(UnitOfWorkOption.RequiresNew))
                    {
                        //isolated database operation. 
                        //Failed operation will not affect parent transaction.
                        using (var db = new DbClient(_testDb))
                        {
                            db.Insert(table2.TableName,
                                "v1_int32,v2_string", 2, "A2");
                        }
                        uow2.Complete();
                    }
                }
                //database operation
                using (var db = new DbClient(_testDb))
                {
                    db.Insert(table3.TableName,
                        "v1_int32,v2_string", 3, "A3");
                }

                parentUoW.Complete();
            }

            using (var db = new DbClient(_testDb))
            {
                var v1 = db.SqlSelect($"select v2_string from {table1.TableName} where v1_int32=1");
                var v2 = db.SqlSelect($"select v2_string from {table2.TableName} where v1_int32=2");
                var v3 = db.SqlSelect($"select v2_string from {table3.TableName} where v1_int32=3");
                Assert.AreEqual("A1", v1.ToString());
                if (!isSqlite)
                    Assert.AreEqual("A2", v2.ToString());
                Assert.AreEqual("A3", v3.ToString());
            }
        }

        [TestMethod]
        public void UoW_level2_Multi_Threads_Tests()
        {
            var table1 = new SampleTable_u("sampleTable_u21");
            var table2 = new SampleTable_u("sampleTable_u22");
            var table3 = new SampleTable_u("sampleTable_u23");
            DropTable(table1.TableName);
            DropTable(table2.TableName);
            DropTable(table3.TableName);

            bool isSqlite = false;


            using (var dbBuilder = new SqlDDLBuilder(_testDb))
            {
                //Register Common tables
                dbBuilder.Database
                    .AddTable(table1)
                    .AddTable(table2)
                    .AddTable(table3);

                dbBuilder.Build(BatchException.LastException, true);
                isSqlite = dbBuilder.Database.Database.ProviderName.Contains("SQLite");
            }
            
            using (var unitwork = new UnitOfWork())
            {
                //parent level
                AddData(table1.TableName, 1, "B1");

                using (var uow2 = new UnitOfWork())
                {
                    //child level
                    AddData(table2.TableName, 1, "B2");
                    uow2.Complete();
                }

                //child thread but same call context
                var task = AddDataAsync(table3.TableName, 1, "B3");
                task.Wait();

                using (var uow2 = new UnitOfWork())
                {
                    //child level
                    AddData(table2.TableName, 2, "B4");
                    uow2.Complete();
                }

                unitwork.Complete();
            }

            Assert.AreEqual("B1", GetData(table1.TableName,1));
            Assert.AreEqual("B2", GetData(table2.TableName, 1));
            Assert.AreEqual("B3", GetData(table3.TableName, 1));
            Assert.AreEqual("B4", GetData(table2.TableName, 2));

            //Rollback parent
            using (var unitwork = new UnitOfWork())
            {
                //parent level
                AddData(table1.TableName, 11, "B1");

                using (var uow2 = new UnitOfWork())
                {
                    //child level
                    AddData(table2.TableName, 11, "B2");
                    uow2.Complete();
                }

                //child thread but same call context
                var task = AddDataAsync(table3.TableName, 11, "B3");
                task.Wait();

                using (var uow2 = new UnitOfWork())
                {
                    //child level
                    AddData(table2.TableName, 12, "B4");
                    uow2.Complete();
                }

                //unitwork.Complete();
            }

            Assert.IsNull(GetData(table1.TableName, 11));
            Assert.IsNull(GetData(table2.TableName, 11));
            Assert.IsNull(GetData(table3.TableName, 11));
            Assert.IsNull(GetData(table2.TableName, 12));

            //Rollback child
            using (var unitwork = new UnitOfWork())
            {
                //parent level
                AddData(table1.TableName, 11, "B1");

                using (var uow2 = new UnitOfWork())
                {
                    //child level
                    AddData(table2.TableName, 11, "B2");
                    uow2.Complete();
                }

                //child thread but same call context
                var task = AddDataAsync(table3.TableName, 11, "B3");
                task.Wait();

                using (var uow2 = new UnitOfWork())
                {
                    //child level
                    AddData(table2.TableName, 12, "B4");
                    //uow2.Complete();
                }

                unitwork.Complete();
            }

            Assert.IsNull(GetData(table1.TableName, 11));
            Assert.IsNull(GetData(table2.TableName, 11));
            Assert.IsNull(GetData(table3.TableName, 11));
            Assert.IsNull(GetData(table2.TableName, 12));

            //Using non-uow persistence operation if an unwanted uow operations in separated thread.
            //Rollback child uows, but not non-uow operation
            var task1 = AddDataAsyncWithoutUow(table3.TableName, 11, "B3");

            using (var unitwork = new UnitOfWork())
            {
                //parent level
                AddData(table1.TableName, 11, "B1");

                using (var uow2 = new UnitOfWork())
                {
                    //child level
                    AddData(table2.TableName, 11, "B2");
                    uow2.Complete();
                }

                using (var uow2 = new UnitOfWork())
                {
                    //child level
                    AddData(table2.TableName, 12, "B4");
                    //uow2.Complete();
                }
                if(!isSqlite)
                    task1.Wait();
                unitwork.Complete();
            }
            if (isSqlite)
                task1.Wait();


            Assert.IsNull(GetData(table1.TableName, 11));
            Assert.IsNull(GetData(table2.TableName, 11));
            Assert.AreEqual("B3", GetData(table3.TableName, 11));
            Assert.IsNull(GetData(table2.TableName, 12));

        }

        [TestMethod]
        public async Task UoW_Parallel_TestsAsync()
        {
            var table1 = new SampleTable_u("sampleTable_u31");
            var table2 = new SampleTable_u("sampleTable_u32");
            var table3 = new SampleTable_u("sampleTable_u33");
            DropTable(table1.TableName);
            DropTable(table2.TableName);
            DropTable(table3.TableName);

            using (var dbBuilder = new SqlDDLBuilder(_testDb))
            {
                //Register Common tables
                dbBuilder.Database
                    .AddTable(table1)
                    .AddTable(table2)
                    .AddTable(table3);

                dbBuilder.Build(BatchException.LastException, true);
            }

            var task1 = AddDataAsyncUoW(table1.TableName, 11, "B1", true);
            var task2 = AddDataAsyncUoW(table2.TableName, 11, "B2", true);
            var task3 = AddDataAsyncUoW(table3.TableName, 11, "B3", false);

            await Task.WhenAll(task1, task2, task3);

            Assert.AreEqual("B1", GetData(table1.TableName, 11));
            Assert.AreEqual("B2", GetData(table2.TableName, 11));
            Assert.AreEqual("B3", GetData(table3.TableName, 11));

        }

        static async Task AddDataAsyncUoW(string tableName, int id, string value, bool uowControl, bool suppress=false)
        {
            if(uowControl)
                await Task.Run(() => AddDataUoW(tableName, id, value,
                    suppress? UnitOfWorkOption.Suppress:
                    UnitOfWorkOption.Required));
            else
                await Task.Run(() => AddData(tableName, id, value));
        }

        static private void AddDataUoW(string tableName, int id, string value, UnitOfWorkOption option)
        {
            using (var uow = new UnitOfWork(option))
            {
                using (var db = new DbClient(_testDb))
                {
                    db.Insert(tableName,
                        "v1_int32,v2_string",
                        id, value);
                }
                uow.Complete();
            }
        }

        static async Task AddDataAsync(string tableName, int id, string value)
        {
            await Task.Run(()=> AddData(tableName, id, value));
        }

        static private void AddData(string tableName, int id, string value)
        {
            using (var db = new DbClient(_testDb))
            {
                db.Insert(tableName,
                    "v1_int32,v2_string",
                    id, value);
            }
        }

        static async Task AddDataAsyncWithoutUow(string tableName, int id, string value)
        {
            await Task.Run(() => AddDataWithoutUoW(tableName, id, value));
        }

        static private void AddDataWithoutUoW(string tableName, int id, string value)
        {
            using(var u = new DbUnitOfWork(UnitOfWorkOption.Suppress))
            using (var db = new DbClient(_testDb))
            {
                db.Insert(tableName,
                    "v1_int32,v2_string",
                    id, value);
            }
        }

        private string GetData(string tableName, int id)
        {
            using (var db = new DbClient(_testDb))
            {
                var v1 = db.SqlSelect($"select v2_string from {tableName} where v1_int32={id}");
                if (v1 == null) return null;
                return v1.ToString();
            }
        }



        public void DropTable(string tableName)
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
    }

    public class SampleTable1 : SqlDDLTable
    {
        public SampleTable1()
            : base("sample_t1", "UnitTest", "Unit test sample table 1.", "utData", "utIndex")
        {
            //Id = 1;
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("v1_int32", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0, comments: "sample C1.")
                .AddColumn("v2_string", System.Data.DbType.String, 150, allowNull: false, comments: "sample C2", isUnique: true, isIndex: true)
                .AddColumn("v3_date", System.Data.DbType.Date, 1, allowNull: true, comments: "sample C3.")
                .AddAuditColumn();
        }
    }

    public class SampleTable1_rename : SqlDDLTable
    {
        public SampleTable1_rename()
            : base("sample_t11", "UnitTest", "Unit test sample table 1.", "utData", "utIndex",version:2)
        {
            //Id = 1;
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("v1_int32", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0, comments: "sample C1.")
                .AddColumn("v2_string", System.Data.DbType.String, 150, allowNull: false, comments: "sample C2", isUnique: true, isIndex: true)
                .AddColumn("v3_date", System.Data.DbType.Date, 1, allowNull: true, comments: "sample C3.")
                .AddAuditColumn();

            //To rename table you must know the internal table unique Id from tracking application table.
            //Also add historic table names from previous version
            long internalId = "select id from sys_ddl_object where object_name='sample_t1' and object_type=1".SqlSelect<long>();

            RenameTable(internalId, "sample_t1", 1);//table name
        }
    }

    public class SampleTable1_rename2 : SqlDDLTable
    {
        public SampleTable1_rename2()
            : base("sample_t12", "UnitTest", "Unit test sample table 1.", "utData", "utIndex", version: 3)
        {
            //Id = 1;
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("v1_int32", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0, comments: "sample C1.")
                .AddColumn("v2_string", System.Data.DbType.String, 150, allowNull: false, comments: "sample C2", isUnique: true, isIndex: true)
                .AddColumn("v3_date", System.Data.DbType.Date, 1, allowNull: true, comments: "sample C3.")
                .AddAuditColumn();

            //To rename table you must know the internal table unique Id from tracking application table.
            //Also add historic table names from previous version
            long internalId = "select id from sys_ddl_object where object_name='sample_t11' and object_type=1".SqlSelect<long>();
            RenameTable(internalId, "sample_t1", 1);//rename 1
            RenameTable(internalId, "sample_t11", 2);//rename 2

        }
    }


    public class SampleTable2 : SqlDDLTable
    {
        public SampleTable2()
            : base("sample_t2", "UnitTest", "Unit test sample table 2.", "utData", "utIndex")
        {
            //Sql Server do not support modifying AUTO increase column
            var provider = ApplicationEnvironment.GetProvider<ISqlDialectProvider>();
            bool isSqlServer = (provider != null && provider.GetType().FullName.Contains(".sqlserver"));
            SqlDDLTable table;
            if (isSqlServer)
            {
                table = AddPKColumn("id", System.Data.DbType.Int64);
            }
            else
            {
                //Create column without PK
                table = AddColumn("id", System.Data.DbType.Int64, 0);
            }
            //Create column without PK
            table
                .AddColumn("v1_int32", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0, comments: "sample C1.")
                .AddColumn("v2_string", System.Data.DbType.String, 150, allowNull: false, comments: "sample C2", isUnique: true, isIndex: true)
                .AddColumn("v3_long", System.Data.DbType.Int64, 150, allowNull: false, comments: "sample C3", isIndex: true, reference: new SampleTable1().PkColumn)
                .AddColumn("v4_date", System.Data.DbType.Date, 1, allowNull: true, comments: "sample C4.")
                .AddAuditColumn();
            //for (int i = 0; i < table.Columns.Count; i++)
            //{
            //    Log.DevDebug("Hash {0}={1}", table.Columns[i].Name, table.Columns[i].HashCode);
            //}
            //Log.DevDebug("Table Hash {0}", table.HashCode);
        }
    }

    public class SampleTable2_1 : SqlDDLTable
    {
        public SampleTable2_1()
            : base("sample_t2", "UnitTest", "Unit test sample table 2.", "utData", "utIndex", version:2)
        {
            //Update PK column with auto increase.
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("v1_int32", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0, comments: "sample C1.")
                .AddColumn("v2_string", System.Data.DbType.String, 200, allowNull: false, comments: "sample C2", isUnique: true, isIndex: true)
                .AddColumn("v3_long", System.Data.DbType.Int64, 150, allowNull: false, comments: "sample C3", isIndex: true, reference: new SampleTable1().PkColumn)
                .AddColumn("v4_date", System.Data.DbType.Date, 1, allowNull: true, comments: "sample C4.", isIndex:true)
                .AddAuditColumn();
        }
    }

    public class SampleTable2_2 : SqlDDLTable
    {
        public SampleTable2_2()
            : base("sample_t2", "UnitTest", "Unit test sample table 2.", "utData", "utIndex", version: 3)
        {
            //Update PK column with auto increase.
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("v1_int32", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0, comments: "sample C1.")
                .AddColumn("v2_string", System.Data.DbType.String, 200, allowNull: false, comments: "sample C2", isUnique: true, isIndex: true)
                .AddColumn("v3_long", System.Data.DbType.Int64, 150, allowNull: false, comments: "sample C3", isIndex: true, reference: new SampleTable1_rename().PkColumn)
                .AddColumn("v4_date", System.Data.DbType.Date, 1, allowNull: true, comments: "sample C4.", isIndex: true)
                .AddAuditColumn();
        }
    }

    public class SampleTable2_3 : SqlDDLTable
    {
        public SampleTable2_3()
            : base("sample_t2", "UnitTest", "Unit test sample table 2.", "utData", "utIndex", version: 3)
        {
            //Update PK column with auto increase.
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("v1_int32", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0, comments: "sample C1.")
                .AddColumn("v2_string", System.Data.DbType.String, 200, allowNull: false, comments: "sample C2", isUnique: true, isIndex: true)
                .AddColumn("v3_long", System.Data.DbType.Int64, 150, allowNull: false, comments: "sample C3", isIndex: true, reference: new SampleTable1_rename2().PkColumn)
                .AddColumn("v4_date", System.Data.DbType.Date, 1, allowNull: true, comments: "sample C4.", isIndex: true)
                .AddAuditColumn();
        }
    }

    public class SampleTable_other : SqlDDLTable
    {
        public SampleTable_other()
            : base("SampleTable_o1", "UnitTest", "Other Unit test table 1.", "utData", "utIndex")
        {
            //Update PK column with auto increase.
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("v1_int32", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0, comments: "sample Other 1.")
                .AddColumn("v2_string", System.Data.DbType.String, 200, allowNull: false, comments: "sample O2", isUnique: true, isIndex: true)
                .AddColumn("v3_long", System.Data.DbType.Int64, 150, allowNull: false, comments: "sample O3", isIndex: true)
                .AddColumn("v4_date", System.Data.DbType.Date, 1, allowNull: true, comments: "sample O4.", isIndex: true)
                .AddColumn("v5_decimal", System.Data.DbType.Decimal, 0, allowNull: true, comments: "sample O5.")
                .AddColumn("v6_Boolean", System.Data.DbType.Boolean, 1, allowNull: true, comments: "sample O6.")
                .AddColumn("v7_clob", System.Data.DbType.String, -1, allowNull: true, comments: "sample big data.")
                .AddColumn("v8_guid", System.Data.DbType.Guid, -1, allowNull: true, comments: "sample Guid.")
                .AddColumn("v9_binary", System.Data.DbType.Binary, -1, allowNull: true, comments: "sample binary.")
                .AddAuditColumn();
        }
    }

    public class SampleTable_u : SqlDDLTable
    {
        public SampleTable_u(string name)
            : base(name, "UnitTest", "Unit test table "+ name, "utData", "utIndex")
        {
            //Update PK column with auto increase.
            AddPKColumn("id", System.Data.DbType.Int64)
                .AddColumn("v1_int32", System.Data.DbType.Int32, 0, allowNull: false, defaultValue: 0, comments: "u1")
                .AddColumn("v2_string", System.Data.DbType.String, 200, allowNull: false, comments: "u2");
        }
    }


}
