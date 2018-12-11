using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace qshine.database
{
    public class SqlDDLDatabase
    {
        public Database Database { get; private set; }

        //List of tables
        public List<SqlDDLTable> Tables { get; private set; }

        public SqlDDLDatabase(Database database)
        {
            this.Database = database;

            Tables = new List<SqlDDLTable>();
        }

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
