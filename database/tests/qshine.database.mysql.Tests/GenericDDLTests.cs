using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Configuration;

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
        public void Unitwork_level1_Tests()
        {
            DropTable("sampleTable_o1");

            var table = new SampleTable_other();
            using (var dbBuilder = new SqlDDLBuilder(_testDb))
            {
                //Register Common tables
                dbBuilder.Database
                    .AddTable(table);

                dbBuilder.Build(BatchException.LastException, true);
            }

            using(var unitwork = new UnitOfWork())
            {
                using(var db = new DbClient(_testDb))
                {
                    byte[] b = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
                    db.Insert(table.TableName,
                        "v1_int32,v2_string,v3_long,v4_date,v5_decimal,v6_Boolean,v7_clob,v8_guid",//v9_binary",
                        99, "ABC", 123456789012345L, new DateTime(2010, 1, 2, 3, 4, 5, 6), 12.2M, true, "11111", Guid.NewGuid());//, b);
                }
                unitwork.Complete();
            }
            using (var db = new DbClient(_testDb))
            {
                var result = db.Retrieve<Dictionary<string, object>>(
                    (x) =>
                    {
                        var d = new Dictionary<string, object>();
                        d.Add("v1_int32", x.ReadInt32("v1_int32"));
                        d.Add("v2_string", x.ReadString("v2_string"));
                        d.Add("v4_date", x.ReadDateTime("v4_date"));
                        d.Add("v5_decimal", x.ReadDecimal("v5_decimal"));
                        d.Add("v6_Boolean", x.ReadBoolean("v6_Boolean"));
                        d.Add("v7_clob", x.ReadString("v7_clob"));
                        d.Add("v8_guid", x.ReadString("v8_guid"));
                        d.Add("v9_binary", x.ReadBytes("v9_binary"));
                        return d;
                    },
                    string.Format("select v1_int32,v2_string,v3_long,v4_date,v5_decimal,v6_Boolean,v7_clob,v8_guid,v9_binary from {0} where v3_long=123456789012345",
                    table.TableName));
                Assert.AreEqual(99, Convert.ToInt32(result[0]["v1_int32"]));
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


}
