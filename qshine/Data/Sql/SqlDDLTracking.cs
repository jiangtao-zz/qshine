using qshine.Globalization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;

namespace qshine.database
{
	/// <summary>
	/// Tracking DDL execution
	/// </summary>
	public class SqlDDLTracking
	{
        ISqlDialect _nativeDatabaseSyntax;
		List<TrackingTable> _trackingTables;
		List<TrackingColumn> _trackingTableColumns;
        List<TrackingName> _trackingNames;
        SqlDDLTable _ddlTrackingTable;
		SqlDDLTable _ddlTrackingColumnTable;
        SqlDDLTable _ddlTrackingRenameTable;

        DbClient _dbClient;

        #region Ctor
        /// <summary>
        /// Ctro.
        /// </summary>
        /// <param name="databaseSyntax"></param>
        /// <param name="dbClient"></param>
        public SqlDDLTracking(ISqlDialect databaseSyntax, DbClient dbClient)
		{
			_nativeDatabaseSyntax = databaseSyntax;
            _dbClient = dbClient;

        }
        #endregion

        #region public Properties

        /// <summary>
        /// The name of the tracking table.
        /// </summary>
        public const string TrackingTableName = "sys_ddl_object";
		/// <summary>
		/// The name of the tracking column table.
		/// </summary>
		public const string TrackingColumnTableName = "sys_ddl_column";

        /// <summary>
        /// The name of the tracking name table.
        /// </summary>
        public const string TrackingNameTableName = "sys_ddl_name";


        /// <summary>
        /// Gets the tracking table structure.
        /// </summary>
        /// <value>The tracking table.</value>
        public SqlDDLTable TrackingTable
		{
			get
			{
				if (_ddlTrackingTable == null)
				{
					_ddlTrackingTable = new SqlDDLTable(TrackingTableName, "System ddl object tracking table", "sysData", "sysIndex");
					_ddlTrackingTable
						.AddPKColumn("id", DbType.Int64)
						.AddColumn("schema_name", DbType.String, 100)
						.AddColumn("object_type", DbType.Int32, 0)
						.AddColumn("comments", DbType.String, 1000)
						.AddColumn("category", DbType.String, 150)
						.AddColumn("object_name", DbType.String, 100, isIndex:true)
						.AddColumn("object_hash", DbType.Int64, 0)
						.AddColumn("version", DbType.Int32, 0, defaultValue: 1)
						.AddColumn("created_on", DbType.DateTime, 0, defaultValue: SqlReservedWord.SysDate)
						.AddColumn("updated_on", DbType.DateTime, 0, defaultValue: SqlReservedWord.SysDate);

				}
				return _ddlTrackingTable;
			}
		}

		/// <summary>
		/// Gets the tracking column table structure.
		/// </summary>
		/// <value>The tracking column table.</value>
		public SqlDDLTable TrackingColumnTable
		{
			get
			{
				if (_ddlTrackingColumnTable == null)
				{
                    var trackingTable = this.TrackingTable;


                    _ddlTrackingColumnTable = new SqlDDLTable(TrackingColumnTableName, "System ddl table column tracking table", "sysData", "sysIndex");
					_ddlTrackingColumnTable
						.AddPKColumn("id", DbType.Int64)
						.AddColumn("table_id", DbType.Int64,0, reference: trackingTable.PkColumn)
                        .AddColumn("internal_id", DbType.Int64, 0)
                        .AddColumn("column_name", DbType.String, 100)
						.AddColumn("comments", DbType.String, 1000)
						.AddColumn("column_type", DbType.String, 100)
						.AddColumn("column_size", DbType.Int32, 0)
                        .AddColumn("scale", DbType.Int32, 0)
                        .AddColumn("default_value", DbType.String, 250)
						.AddColumn("allow_null", DbType.Boolean, 0)
						.AddColumn("reference", DbType.String, 100)
						.AddColumn("is_unique", DbType.Boolean, 0)
						.AddColumn("is_pk", DbType.Boolean, 0)
						.AddColumn("constraint_value", DbType.String, 250)
						.AddColumn("auto_increase", DbType.Boolean, 0)
						.AddColumn("is_index", DbType.Boolean, 0)
						.AddColumn("version", DbType.Int32, 0, defaultValue: 1)
                        .AddColumn("hash_code", DbType.Int64, 0)
                        .AddColumn("created_on", DbType.DateTime, 0, defaultValue: SqlReservedWord.SysDate)
						.AddColumn("updated_on", DbType.DateTime, 0, defaultValue: SqlReservedWord.SysDate);
				}
				return _ddlTrackingColumnTable;
			}
		}

        /// <summary>
        /// Tracking table name change history.
        /// </summary>
        /// <value>The tracking column table.</value>
        public SqlDDLTable TrackingRenameTable
        {
            get
            {
                if (_ddlTrackingRenameTable == null)
                {
                    _ddlTrackingRenameTable = new SqlDDLTable(TrackingNameTableName, "System ddl table name tracking table", "sysData", "sysIndex");
                    _ddlTrackingRenameTable
                        .AddPKColumn("id", DbType.Int64)
                        .AddColumn("schema_name", DbType.String, 100)
                        .AddColumn("object_type", DbType.Int32, 0)
                        .AddColumn("object_id", DbType.Int64, 0)
                        .AddColumn("object_name", DbType.String, 100)
                        .AddColumn("hash_code", DbType.Int64, 0)
                        .AddColumn("version", DbType.Int32, 0)
                        .AddColumn("created_on", DbType.DateTime, 0, defaultValue: SqlReservedWord.SysDate);
                }
                return _ddlTrackingRenameTable;
            }
        }

        #endregion


        string ParameterName(string name)
        {
            return _nativeDatabaseSyntax.ParameterPrefix + name;
        }

        #region public Methods
        /// <summary>
        /// Load table tracking information.
        /// </summary>
        public void Load()
		{
            //Load table tracking
			var sql = string.Format(
@"select c.id, t.object_name, c.column_name, c.column_type, c.column_size, c.scale, c.default_value, c.allow_null,
c.reference, c.is_unique, c.is_pk, c.constraint_value, c.auto_increase, c.is_index, c.version, c.created_on, c.updated_on, c.comments,
c.internal_id, c.hash_code,t.id
from {0} c inner join {1} t on c.table_id=t.id
and t.object_type={2}", TrackingColumnTableName, TrackingTableName, ParameterName("p1"));

			_trackingTableColumns = _dbClient.Retrieve(
				(x) =>
			{
				return new TrackingColumn
				{
					Id = x.GetInt64(0),//id
					TableName = x.ReadString(1), //object_name
					ColumnName = x.ReadString(2),//column_name
					ColumnType = x.ReadString(3),//column_type
					Size = x.ReadInt32(4),//size
                    Scale = x.ReadInt32(5),//scale
                    DefaultValue = x.ReadString(6),//default_value
					AllowNull = x.ReadBoolean(7),//allow_null
					Reference = x.ReadString(8),//reference
					IsUnique = x.ReadBoolean(9),//is_unique
					IsPK = x.ReadBoolean(10),//is_pk
					CheckConstraint = x.ReadString(11),//constraint
					AutoIncrease = x.ReadBoolean(12),//auto_increase
					IsIndex = x.ReadBoolean(13),//is_index
					Version = x.GetInt32(14),
					CreatedOn = x.ReadDateTime(15),
					UpdatedOn = x.ReadDateTime(16),
					Comments = x.ReadString(17),
                    InternalId = x.ReadInt32(18),
                    HashCode = x.ReadInt64(19),
                    TableId = x.ReadInt64(20)
                };
			}, sql, DbParameters.New.Input("p1", (int)TrackingObjectType.Table, DbType.Int32));

            //Load column traacking
			sql = string.Format(
@"select t.id, t.schema_name, t.object_type, t.object_name, t.object_hash, t.version, t.created_on, t.updated_on, t.comments, t.category 
from {0} t where t.object_type={1}", TrackingTableName, ParameterName("p1"));
			
			_trackingTables = _dbClient.Retrieve(
				(x) =>
			{
				var tableName = x.GetString(3);
				return new TrackingTable
				{
					Id = x.GetInt64(0),//id
					SchemaName = x.ReadString(1), //schema name
					ObjectType = (TrackingObjectType)x.GetInt32(2), //object type
					ObjectName = tableName,
					HashCode = x.GetInt64(4),
					Version = x.GetInt32(5),
					CreatedOn = x.ReadDateTime(6),
					UpdatedOn = x.ReadDateTime(7),
					Comments = x.ReadString(8),
					Category = x.ReadString(9),
					Columns = _trackingTableColumns.FindAll(y=>y.TableName==tableName)
				};
			}, sql, DbParameters.New.Input("p1",  (int)TrackingObjectType.Table, DbType.Int32));

            //load rename tracking
            sql = string.Format(
@"select id, schema_name, object_type, object_id, object_name,hash_code, version, created_on
from {0}", TrackingNameTableName);

            _trackingNames = _dbClient.Retrieve(
                (x) =>
                {
                    return new TrackingName
                    {
                        Id = x.GetInt64(0),//id
                        SchemaName = x.ReadString(1), //schema name
                        ObjectType = (TrackingObjectType)x.GetInt32(2), //object type
                        ObjectId = x.ReadInt64(3),
                        ObjectName = x.GetString(4),
                        HashCode = x.GetInt64(5),
                        Version = x.GetInt32(6),
                        CreatedOn = x.ReadDateTime(7)
                    };
                }, sql);
        }

        /// <summary>
        /// Remove tracking table by id
        /// </summary>
        /// <param name="id"></param>
        public void RemoveTrackingTable(long id)
        {
            //Found the table was deleted outside the control. We need delete the tracking table record and re-create it after.
            //UpdateTrackingTable(existingTrackingTable, table);
            //delete tracking table column records
            _dbClient.Sql(
                string.Format("delete from {0} where table_id in (select id from  {1} where id={2})"
                , TrackingColumnTableName, TrackingTableName,
                ParameterName("p1")
                ),
                DbParameters.New
                .Input("p1", id)
                );

            //delete tracking table records
            _dbClient.Sql(
                string.Format("delete from {0} where id={1}", TrackingTableName,
                ParameterName("p1")
                ),
                DbParameters.New
                .Input("p1", id)
                );

            //delete tracking name table records
            _dbClient.Sql(
                string.Format("delete from {0} where object_id={1} and object_type={2}", TrackingNameTableName,
                ParameterName("p1"),
                ParameterName("p2")
                ),
                DbParameters.New
                .Input("p1", id)
                .Input("p2", (int)TrackingObjectType.Table)
                );
            //Remove the tracking record from the tracking view
            _trackingTableColumns.RemoveAll(x => x.TableId == id);
            _trackingNames.RemoveAll(x => x.ObjectId == id && x.ObjectType==TrackingObjectType.Table);
            _trackingTables.RemoveAll(x => x.Id == id);
        }

        /// <summary>
        /// Adds to tracking history.
        /// </summary>
        /// <param name="table">Table.</param>
        public void AddToTrackingTable(SqlDDLTable table)
		{
            var existingTrackingTable = _trackingTables.SingleOrDefault(x => 
            (table.Id>0 && x.Id==table.Id) || (x.SchemaName == table.SchemaName && x.ObjectName == table.TableName));

            if (existingTrackingTable!=null)
            {
                RemoveTrackingTable(existingTrackingTable.Id);
            }

            //Create a tracking table record
            var sql = string.Format("insert into {0} ({8}schema_name, object_type,object_name,object_hash,version, comments, category) values ({9}{1},{2},{3},{4},{5},{6},{7})",
                TrackingTableName,
                ParameterName("p1"),
                ParameterName("p2"),
                ParameterName("p3"),
                ParameterName("p4"),
                ParameterName("p5"),
                ParameterName("p6"),
                ParameterName("p7"),
                table.Id>0?"id,":"",
                table.Id > 0 ? ParameterName("p0")+",":""
                );

            var parameters =DbParameters.New;
            if (table.Id > 0)
            {
                parameters.Input("p0", table.Id);
            }

            parameters.
                Input("p1", table.SchemaName, System.Data.DbType.String)
                .Input("p2", (int)TrackingObjectType.Table)
                .Input("p3", table.TableName)
                .Input("p4", table.HashCode)
                .Input("p5", table.Version)
                .Input("p6", table.Comments)
                .Input("p7", table.Category);


            _dbClient.Sql(sql, parameters);
			var tableIdObject = _dbClient.SqlSelect(
                string.Format("select id from {0} where object_type={1} and object_name={2} and {3}",
                TrackingTableName, 
                ParameterName("p1"),
                ParameterName("p2"),
                _nativeDatabaseSyntax.ToSqlCondition("schema_name", "=", table.SchemaName)),
                DbParameters.New
                .Input("p1", (int)TrackingObjectType.Table)
                .Input("p2", table.TableName));

            if (tableIdObject != null)
			{
                long tableId = Convert.ToInt64(tableIdObject);

				//Create tracking table column records
				foreach (var column in table.Columns)
				{
					AddNewTrackingColumn(tableId, column);
				}
			}
			else
			{
				throw new Exception("Couldn't find tracking table Id for table {0}"._G(table.TableName));
			}
		}

        private StreamingContext _G(string tableName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the tracking table columns if any column structure changed.
        /// </summary>
        /// <param name="trackingTable">Tracking table.</param>
        /// <param name="table">the table to be updated.</param>
        public void UpdateTrackingTableColumns(TrackingTable trackingTable, SqlDDLTable table)
		{
			string sql;

			//Create tracking table column records
			foreach (var column in table.Columns)
			{
				bool columnChanged = false;
				var trackingColumn = trackingTable.Columns.SingleOrDefault(x => x.ColumnName == column.Name);

				if (trackingColumn != null)
				{
                    if (column.Version > trackingColumn.Version ||
                        (column.ColumnNameHistory != null && column.ColumnNameHistory.Contains(trackingColumn.ColumnName)))
                    {
                        //column name changed
                        columnChanged = true;
                    }
				}
				else
				{
					//new column
					AddNewTrackingColumn(trackingTable.Id, column);
				}

				if (columnChanged)
				{
					//Column attribute changed
					sql = string.Format(@"update {0} set column_name={1}, column_type={2}, column_size={3}, scale={4}, default_value={5}, allow_null={6}, 
reference={7}, is_unique={8}, is_pk={9}, constraint_value={10}, auto_increase={11}, is_index={12},
version={13}, updated_on={14}, comments={15}, internal_id = {16}, hash_code={17}
where id={18}", 
                        TrackingColumnTableName,
                        ParameterName("p1"),
                        ParameterName("p2"),
                        ParameterName("p3"),
                        ParameterName("p4"),
                        ParameterName("p5"),
                        ParameterName("p6"),
                        ParameterName("p7"),
                        ParameterName("p8"),
                        ParameterName("p9"),
                        ParameterName("p10"),
                        ParameterName("p11"),
                        ParameterName("p12"),
                        ParameterName("p13"),
                        ParameterName("p14"),
                        ParameterName("p15"),
                        ParameterName("p16"),
                        ParameterName("p17"),
                        ParameterName("p18")
                        );

                    _dbClient.Sql(sql, DbParameters.New
                        .Input("p1", column.Name)
                        .Input("p2", column.DbType.ToString())
						.Input("p3", column.Size)
                        .Input("p4", column.Scale)
                        .Input("p5", Convert.ToString(column.DefaultValue))
						.Input("p6", column.AllowNull)
						.Input("p7", column.ToReferenceClause())
						.Input("p8", column.IsUnique)
						.Input("p9", column.IsPK)
						.Input("p10", column.CheckConstraint)
						.Input("p11", column.AutoIncrease)
						.Input("p12", column.IsIndex)
						.Input("p13", column.Version)
						.Input("p14", DateTime.Now)
						.Input("p15", column.Comments)
						.Input("p16", column.InternalId)
                        .Input("p17", column.HashCode)
                        .Input("p18", trackingColumn.Id)
                    );
				}
			}
		}


		/// <summary>
		/// Updates the tracking table if table name change.
		/// </summary>
		/// <param name="trackingTable">Tracking table.</param>
		/// <param name="table">Table.</param>
		public void UpdateTrackingTableForTableRename(TrackingTable trackingTable, SqlDDLTable table)
        {
            //Add previous table name in name tracking table
            var sql = string.Format(
                "insert into {0} (schema_name, object_type, object_id, object_name, hash_code, version) values ({1}, {2}, {3}, {4}, {5}, {6})",
                TrackingNameTableName,
                ParameterName("p1"),
                ParameterName("p2"),
                ParameterName("p3"),
                ParameterName("p4"),
                ParameterName("p5"),
                ParameterName("p6")
                );

            _dbClient.Sql(sql, DbParameters.New
                .Input("p1", trackingTable.SchemaName)
                .Input("p2", (int)trackingTable.ObjectType)
                .Input("p3", trackingTable.Id)
                .Input("p4", trackingTable.ObjectName)
                .Input("p5", trackingTable.HashCode)
                .Input("p6", trackingTable.Version)
                );

            //update the new table name
            sql = string.Format(
                "update {0} set schema_name={1}, object_name={2}, object_hash={3}, version={4}, comments={5}, category={6}, updated_on={7} where id={8}",
                TrackingTableName,
                ParameterName("p1"),
                ParameterName("p2"),
                ParameterName("p3"),
                ParameterName("p4"),
                ParameterName("p5"),
                ParameterName("p6"),
                ParameterName("p7"),
                ParameterName("p8")
                );

            _dbClient.Sql(sql, DbParameters.New
				.Input("p1", table.SchemaName, System.Data.DbType.String)
				.Input("p2", table.TableName)
				.Input("p3", table.HashCode)
				.Input("p4", table.Version)
				.Input("p5", table.Comments)
				.Input("p6", table.Category)
				.Input("p7", DateTime.Now)
                .Input("p8", trackingTable.Id)
                );

            //Update the tracking table
            trackingTable.ObjectName = table.TableName;
            trackingTable.ObjectType = TrackingObjectType.Table;
            trackingTable.Version = table.Version;
            trackingTable.HashCode = table.HashCode;
            trackingTable.Category = table.Category;
            trackingTable.Comments = table.Comments;
            trackingTable.Id = table.Id;

        }

        /// <summary>
        /// Gets the tracking table.
        /// </summary>
        /// <returns>The tracking table.</returns>
        /// <param name="tableName">Table name.</param>
        public TrackingTable GetTrackingTable(string tableName)
		{
			if (_trackingTables != null)
			{
				return _trackingTables.SingleOrDefault(x => x.ObjectName == tableName && x.ObjectType == TrackingObjectType.Table);
			}
			return null;
		}

        /// <summary>
        /// Find tracking table based on given table information.
        /// </summary>
        /// <returns>The tracking table.</returns>
        /// <param name="table">table.</param>
        public TrackingTable FindSameTrackingTable(SqlDDLTable table)
        {
            TrackingTable trackingTable = null;
            if (_trackingTables != null)
            {
                //find same version tracking table
                trackingTable =  _trackingTables.SingleOrDefault(x => x.ObjectName == table.TableName
                && x.ObjectType == TrackingObjectType.Table && x.HashCode == table.HashCode 
                && x.Version == table.Version);

                //try to find from rename history
                if (trackingTable == null && _trackingNames!=null)
                {
                    var trackingName = _trackingNames.SingleOrDefault(x=>x.ObjectName==table.TableName
                    && x.ObjectType == TrackingObjectType.Table && x.HashCode == table.HashCode
                    && x.Version == table.Version);
                    if (trackingName != null)
                    {
                        trackingTable = _trackingTables.SingleOrDefault(x => x.Id == trackingName.Id);
                    }
                }
            }
            return trackingTable;
        }


        /// <summary>
        /// Gets the tracking table by internal Id.
        /// </summary>
        /// <returns>The tracking table.</returns>
        /// <param name="Id">Table internal Id.</param>
        public TrackingTable GetTrackingTable(long Id)
        {
            if (_trackingTables != null)
            {
                return _trackingTables.SingleOrDefault(x => x.Id == Id && x.ObjectType == TrackingObjectType.Table);
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Adds the new tracking column.
        /// </summary>
        /// <param name="tableId">Table identifier.</param>
        /// <param name="column">Column.</param>
        void AddNewTrackingColumn(long tableId, SqlDDLColumn column)
        {

            var sql = string.Format(@"insert into {0} (table_id, column_name, column_type, column_size, scale, default_value, allow_null, reference, is_unique, is_pk, constraint_value, auto_increase, is_index, version, comments,
internal_id, hash_code)
values({1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17})",
                TrackingColumnTableName,
                ParameterName("p1"),
                ParameterName("p2"),
                ParameterName("p3"),
                ParameterName("p4"),
                ParameterName("p5"),
                ParameterName("p6"),
                ParameterName("p7"),
                ParameterName("p8"),
                ParameterName("p9"),
                ParameterName("p10"),
                ParameterName("p11"),
                ParameterName("p12"),
                ParameterName("p13"),
                ParameterName("p14"),
                ParameterName("p15"),
                ParameterName("p16"),
                ParameterName("p17")
                );

            _dbClient.Sql(sql, DbParameters.New
                .Input("p1", tableId)
                .Input("p2", column.Name)
                .Input("p3", column.DbType.ToString())
                .Input("p4", column.Size)
                .Input("p5", column.Scale)
                .Input("p6", Convert.ToString(column.DefaultValue))
                .Input("p7", column.AllowNull)
                .Input("p8", column.ToReferenceClause())
                .Input("p9", column.IsUnique)
                .Input("p10", column.IsPK)
                .Input("p11", column.CheckConstraint)
                .Input("p12", column.AutoIncrease)
                .Input("p13", column.IsIndex)
                .Input("p14", column.Version)
                .Input("p15", column.Comments)
                .Input("p16", column.InternalId)
                .Input("p17", column.HashCode)
                );
        }

    }
}
