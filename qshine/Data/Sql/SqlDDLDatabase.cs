using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace qshine.database
{
    /// <summary>
    /// Database Sql DDL
    /// </summary>
    public class SqlDDLDatabase
    {
        /// <summary>
        /// Ctro.
        /// </summary>
        /// <param name="database"></param>
        public SqlDDLDatabase(Database database)
        {
            this.Database = database;

            Tables = new List<SqlDDLTable>();
        }

        /// <summary>
        /// Get Database property
        /// </summary>
        public Database Database { get; private set; }

        /// <summary>
        /// Get DDLs for table creation
        /// </summary>
        public List<SqlDDLTable> Tables { get; private set; }

        /// <summary>
        /// Add table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public SqlDDLDatabase AddTable(SqlDDLTable table)
        {
            Check.Assert<ArgumentException>(!Tables.Any(x => x.TableName.AreEqual(table.TableName)),
                "Table {0} already added in database", table.TableName);

            table.Database = this;
            Tables.Add(table);

            return this;
        }

    }
}
